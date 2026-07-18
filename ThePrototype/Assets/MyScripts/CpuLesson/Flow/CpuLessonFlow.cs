using System;
using UnityEngine;

/// <summary>
/// Scene-facing lesson flow root.
/// This stays attached to the lesson object and owns:
/// - serialized scene references
/// - public lesson API used by UI and phase controllers
/// - step / feedback events
/// - orchestration across the smaller lesson services
/// </summary>
[DisallowMultipleComponent]
public sealed class CpuLessonFlow : MonoBehaviour
{
    const string k_LogPrefix = "[CpuLessonFlow]";

    [SerializeField]
    InstructionDefinition m_CurrentInstruction;

    [SerializeField]
    string m_DefaultInstructionResourcePath = "InstructionDefinitions/Add";

    [SerializeField]
    LessonMode m_CurrentMode = LessonMode.Learning;

    [SerializeField]
    PracticeInstructionDefinition m_CurrentPracticeInstruction;

    [SerializeField]
    string m_DefaultPracticeInstructionResourcePath = "PracticeInstructionDefinitions/Add";

    [SerializeField]
    RegisterBank m_RegisterBank;

    [SerializeField]
    ImmediateExtender m_ImmediateExtender;

    [SerializeField]
    InstructionTerminal m_FetchUploadTerminal;

    [SerializeField]
    InstructionTerminal m_DecodeDownloadTerminal;

    [SerializeField]
    InstructionRuntimeSelection m_RuntimeSelection = new();

    LessonState m_State;
    FetchFlow m_Fetch;
    DecodeFlow m_Decode;
    FlowProgress m_Progress;
    LessonLifecycle m_Lifecycle;
    LessonStepActions m_Actions;

    public event Action<CpuLessonFlow> StepChanged;
    public event Action<string, bool> FeedbackChanged;
    public event Action PracticeDecodeScannerFailed;

    public InstructionDefinition CurrentInstruction => m_CurrentInstruction;
    public PracticeInstructionDefinition CurrentPracticeInstruction => m_CurrentPracticeInstruction;
    public LessonMode CurrentMode => m_CurrentMode;
    public InstructionRuntimeSelection RuntimeSelection => m_State.RuntimeSelection;
    public int CurrentStepIndex => m_State.CurrentStepIndex;
    public int CurrentRegisterSelectionIndex => m_State.CurrentRegisterSelectionIndex;
    public bool HasStarted => m_State != null && m_State.HasStarted;
    public RegisterBank RegisterBank => m_RegisterBank;
    public bool RegisterSelectionReadyToContinue => m_State.RegisterSelectionReadyToContinue;
    public bool UsesInstructionTerminals => m_Fetch.UsesTerminals;
    public bool IsInstructionReadyForDecode => !UsesInstructionTerminals || m_Fetch.HasDownloadedInstructionModule();
    public bool UsesLearningInstructionSelection => m_CurrentMode == LessonMode.Learning;
    public bool UsesInstructionSelection => LessonInstrResolver.UsesInstructionSelection(m_CurrentMode);
    public bool CanStartSelectedMode => LessonInstrResolver.CanStart(m_CurrentMode, m_CurrentInstruction, m_CurrentPracticeInstruction);
    public bool IsFetchStepActive => m_Fetch.IsFetchStep(CurrentStep);
    public bool IsPracticeDecodeScannerFailureAwaitingReset => m_Decode != null && m_Decode.IsPracticeScannerFailureAwaitingReset;
    public string CurrentFetchDisplayText => LessonInstrResolver.GetFetchDisplayText(m_CurrentMode, m_CurrentInstruction, m_CurrentPracticeInstruction);
    public string CurrentPracticeBinaryText => m_CurrentPracticeInstruction != null
        ? m_CurrentPracticeInstruction.GetNormalizedBinaryInstruction()
        : string.Empty;

    public InstructionFlowStep CurrentStep => GetStepAt(m_State.CurrentStepIndex);

    internal string LogPrefix => k_LogPrefix;
    internal InstructionDefinition ActiveInstruction => m_CurrentInstruction;
    internal RegisterBank RegisterBankRef => m_RegisterBank;
    internal ImmediateExtender ImmediateExtenderRef => m_ImmediateExtender;
    internal InstructionTerminal FetchUploadTerminal => m_FetchUploadTerminal;
    internal InstructionTerminal DecodeDownloadTerminal => m_DecodeDownloadTerminal;
    internal FlowProgress ProgressRef => m_Progress;

    /// <summary>
    /// Builds the backing lesson services before any other component queries the flow.
    /// </summary>
    void Awake()
    {
        EnsureServices();
    }

    /// <summary>
    /// Rebinds scene-driven decode listeners every time the flow component becomes active.
    /// </summary>
    void OnEnable()
    {
        EnsureServices();
        m_Decode.Bind();
    }

    /// <summary>
    /// Unbinds scene-driven decode listeners to avoid duplicate scanner callbacks.
    /// </summary>
    void OnDisable()
    {
        m_Decode?.Unbind();
    }

    /// <summary>
    /// Stores the selected top-level lesson mode.
    /// For now only Learning is executable; Practice/Test stay disabled until
    /// their dedicated startup and decode flows are implemented.
    /// </summary>
    public void SetLessonMode(LessonMode mode)
    {
        EnsureServices();

        if (mode == m_CurrentMode)
            return;

        m_CurrentMode = mode;

        if (m_CurrentMode == LessonMode.Learning && m_CurrentInstruction == null)
            m_CurrentInstruction = LoadDefaultInstruction();

        if (m_CurrentMode == LessonMode.Practice && m_CurrentPracticeInstruction == null)
            m_CurrentPracticeInstruction = LoadDefaultPracticeInstruction();

        m_RegisterBank?.RestoreAuthoredRegisterValues();

        if (HasStarted)
            ResetLesson();

        RaiseStepChanged();
    }

    /// <summary>
    /// Inspector-friendly wrapper for binding a TMP dropdown directly to the flow.
    /// Invalid values fall back to Learning so accidental scene wiring cannot trap
    /// the lesson in an undefined state.
    /// </summary>
    public void SetLessonModeFromDropdown(int dropdownValue)
    {
        var requestedMode = Enum.IsDefined(typeof(LessonMode), dropdownValue)
            ? (LessonMode)dropdownValue
            : LessonMode.Learning;

        SetLessonMode(requestedMode);
    }

    /// <summary>
    /// Lets the authored intro UI decide which instruction asset should drive
    /// the next walkthrough.
    /// </summary>
    public void SetCurrentInstruction(InstructionDefinition instruction)
    {
        EnsureServices();

        // Named instruction selection belongs only to Learning mode. Practice
        // will use its own encoded-instruction asset path instead.
        if (m_CurrentMode != LessonMode.Learning ||
            instruction == null ||
            instruction == m_CurrentInstruction)
            return;

        SetActiveInstructionInternal(instruction);

        // Restart if the learner switches lessons mid-run so authored state,
        // spawned objects, and register values cannot drift.
        if (HasStarted)
            ResetLesson();

        RaiseStepChanged();
    }

    /// <summary>
    /// Stores the currently selected Practice instruction without disturbing the
    /// existing Learning instruction asset pipeline.
    /// </summary>
    public void SetCurrentPracticeInstruction(PracticeInstructionDefinition instruction)
    {
        EnsureServices();

        if (instruction == null || instruction == m_CurrentPracticeInstruction)
            return;

        m_CurrentPracticeInstruction = instruction;

        if (HasStarted && m_CurrentMode == LessonMode.Practice)
            ResetLesson();

        RaiseStepChanged();
    }

    public void StartLesson()
    {
        EnsureServices();

        if (!CanStartSelectedMode)
        {
            Debug.LogWarning(
                $"{k_LogPrefix} {m_CurrentMode} mode has not been wired yet. Staying on the pre-start screen.",
                this);
            return;
        }

        m_Lifecycle.StartLesson();
    }

    /// <summary>
    /// Handles progression requests from lesson UI panels and phase stations.
    /// Each interaction type decides for itself whether progression is currently allowed.
    /// </summary>
    public void Advance()
    {
        EnsureServices();
        m_Actions.Advance();
    }

    /// <summary>
    /// Returns the lesson to its initial state without changing the currently selected instruction.
    /// </summary>
    public void ResetLesson()
    {
        EnsureServices();
        m_Lifecycle.ResetLesson();
    }

    /// <summary>
    /// Stores the executed ALU result and advances into the next authored lesson phase.
    /// </summary>
    public void CompleteAluExecution(int resultValue)
    {
        EnsureServices();
        m_Actions.CompleteAluExecution(resultValue);
    }

    /// <summary>
    /// Applies the final write-back value after the authored WB prefab has
    /// validated control signals and completed its transfer sequence.
    /// </summary>
    public void CompleteWriteBackExecution(string destinationRegister, int resultValue)
    {
        EnsureServices();
        m_Actions.CompleteWriteBackExecution(destinationRegister, resultValue);
    }

    /// <summary>
    /// Lets the fetch terminal announce that a fresh lesson instruction is now
    /// loaded into the physical module.
    /// </summary>
    public void NotifyInstructionUploaded(InstructionDefinition instruction)
    {
        EnsureServices();
        m_Fetch.NotifyInstructionUploaded(instruction);
    }

    /// <summary>
    /// Called by the decode terminal once the learner docks the carried module.
    /// This unlocks progression out of fetch into the decode phase.
    /// </summary>
    public void NotifyInstructionModuleDownloaded(InstructionModule module)
    {
        EnsureServices();
        m_Fetch.NotifyInstructionModuleDownloaded(module);
    }

    internal InstructionFlowStep GetStepAt(int stepIndex)
    {
        if (m_CurrentInstruction == null ||
            m_CurrentInstruction.flowSteps == null ||
            stepIndex < 0 ||
            stepIndex >= m_CurrentInstruction.flowSteps.Length)
        {
            return null;
        }

        return m_CurrentInstruction.flowSteps[stepIndex];
    }

    /// <summary>
    /// Raises the shared step-changed event consumed by lesson UI.
    /// </summary>
    internal void RaiseStepChanged()
    {
        StepChanged?.Invoke(this);
    }

    /// <summary>
    /// Raises learner-facing feedback for whichever panel currently owns the lesson.
    /// </summary>
    internal void RaiseFeedback(string message, bool isFailure)
    {
        FeedbackChanged?.Invoke(message, isFailure);
    }

    internal void NotifyPracticeDecodeScannerFailed()
    {
        PracticeDecodeScannerFailed?.Invoke();
    }

    /// <summary>
    /// Loads the fallback lesson instruction used when the authored scene has not
    /// explicitly selected one yet.
    /// </summary>
    internal InstructionDefinition LoadDefaultInstruction()
    {
        return LessonInstrResolver.LoadDefaultLearning(m_DefaultInstructionResourcePath);
    }

    /// <summary>
    /// Loads the fallback Practice instruction used when the scene has not yet
    /// been assigned one explicitly.
    /// </summary>
    internal PracticeInstructionDefinition LoadDefaultPracticeInstruction()
    {
        return LessonInstrResolver.LoadDefaultPractice(m_DefaultPracticeInstructionResourcePath);
    }

    /// <summary>
    /// Resolves which Learning instruction should drive the shared lesson runtime
    /// for the currently selected top-level mode.
    /// </summary>
    internal InstructionDefinition ResolveLessonInstructionForCurrentMode()
    {
        return LessonInstrResolver.ResolveRuntimeInstruction(m_CurrentMode, m_CurrentInstruction, m_CurrentPracticeInstruction);
    }

    /// <summary>
    /// Lets the guide configure the Practice decode scanner budget without
    /// owning the scanner-validation logic itself.
    /// </summary>
    public void ConfigurePracticeDecodeScannerAttempts(int maxAttempts)
    {
        EnsureServices();
        m_Decode.ConfigurePracticeScannerAttempts(maxAttempts);
    }

    /// <summary>
    /// Dev-mode helper that bypasses the decode phase and prepares the runtime
    /// operand state expected by later phases.
    /// </summary>
    public void DevForceCompleteDecodePhase()
    {
        EnsureServices();
        m_Decode.ForceCompleteDecodePhase();
    }

    /// <summary>
    /// Updates the active instruction and mirrors it into the runtime selection state.
    /// Internal services use this to keep authored instruction swaps centralized.
    /// </summary>
    internal void SetActiveInstructionInternal(InstructionDefinition instruction)
    {
        DisposeTransientPracticeInstruction();
        m_CurrentInstruction = instruction;

        if (m_State != null)
            m_State.RuntimeSelection.definition = m_CurrentInstruction;
    }

    /// <summary>
    /// Lazily creates the lesson services that now hold progression, fetch, and decode logic.
    /// </summary>
    void EnsureServices()
    {
        if (m_State != null)
            return;

        if (m_CurrentInstruction == null)
            m_CurrentInstruction = LoadDefaultInstruction();

        if (m_CurrentPracticeInstruction == null)
            m_CurrentPracticeInstruction = LoadDefaultPracticeInstruction();

        m_State = new LessonState(m_RuntimeSelection);
        m_Fetch = new FetchFlow(this, m_State);
        m_Decode = new DecodeFlow(this, m_State);
        m_Progress = new FlowProgress(this, m_State, m_Decode, m_Fetch);
        m_Lifecycle = new LessonLifecycle(this, m_State, m_Decode, m_Fetch);
        m_Actions = new LessonStepActions(this, m_State, m_Decode, m_Fetch, m_Progress, m_Lifecycle);
        m_RegisterBank?.SetLessonInactivePreviewMode(true);
    }

    /// <summary>
    /// Practice-mode runtime instructions are cloned on demand so they can
    /// carry per-practice operand overrides. Dispose the previous transient
    /// clone before replacing it with the next one.
    /// </summary>
    void DisposeTransientPracticeInstruction()
    {
        if (m_CurrentMode != LessonMode.Practice || m_CurrentInstruction == null)
            return;

        if (m_CurrentInstruction.hideFlags != HideFlags.DontSave)
            return;

        if (Application.isPlaying)
            Destroy(m_CurrentInstruction);
        else
            DestroyImmediate(m_CurrentInstruction);
    }
}

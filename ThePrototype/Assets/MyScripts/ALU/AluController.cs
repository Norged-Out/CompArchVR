using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Owns the authored ALU station, its execute-phase UI, and the physical
/// ALUOp / ALUSrc controls.
/// </summary>
[DisallowMultipleComponent]
public sealed class AluController : MonoBehaviour
{
    [Header("Station")]
    [SerializeField]
    AluInputScanner m_InputA;

    [SerializeField]
    AluInputScanner m_InputB;

    [SerializeField]
    TMP_Text m_OperationLabelText;

    [SerializeField]
    Transform m_ResultSpawnTransform;

    [SerializeField]
    ParticleSystem m_ComputeParticles;

    [SerializeField]
    DataPacketToken m_ResultPacketPrefab;

    [Header("Audio")]
    [SerializeField]
    LessonUiAudioCueSet m_LessonAudioCues = new();

    [Header("Physical Buttons")]
    [SerializeField]
    Transform m_AluOpButtonRoot;

    [SerializeField]
    Transform m_AluSrcButtonRoot;

    [Header("Lesson Panel")]
    [SerializeField]
    AluLessonPanelRefs m_LessonPanel;

    [Header("Hint Panel")]
    [SerializeField]
    PhaseHintPanelRefs m_HintPanel;

    [SerializeField]
    AluHintInfoRefs m_LearningHints;

    [Header("Interaction Panel")]
    [SerializeField]
    GameObject m_AluUiRoot;

    [SerializeField]
    AluInteractionPanelRefs m_InteractionPanel;

    [SerializeField]
    PhaseSharedInteractionRefs m_SharedInteraction;

    [Header("Practice")]
    [SerializeField]
    int m_PracticeValidationAttempts = 3;

    [SerializeField]
    int m_PracticeScannerAttempts = 3;

    [SerializeField]
    int m_PracticeHints = 2;

    [Header("Timing")]
    [SerializeField]
    float m_ResultSpawnDelaySeconds = 1.25f;

    [Header("Labels")]
    [SerializeField]
    string m_ExecuteButtonText = "Execute";

    [SerializeField]
    string m_ResultReadyButtonText = "Continue";

    [SerializeField]
    Color m_SuccessFeedbackColor = new(0.78f, 0.96f, 0.82f, 1f);

    [SerializeField]
    Color m_FailureFeedbackColor = new(1f, 0.55f, 0.55f, 1f);

    InstructionDefinition m_CurrentInstruction;
    Coroutine m_ComputeRoutine;
    DataPacketToken m_SpawnedResultPacket;
    AluExecutionService m_ExecutionService;
    readonly AluPracticeFlow m_PracticeFlow = new();
    bool m_IsPhaseActive;
    bool m_HasProducedResult;
    bool m_IsAwaitingContinue;
    int m_LastResultValue;
    string m_CurrentAluOpValue = "00";
    string m_CurrentAluSrcValue = "0";
    AluOperation m_SelectedFunctOperation = AluOperation.Add;
    bool m_HasExplicitFunctSelection;
    LessonMode m_CurrentMode = LessonMode.Learning;

    public event Action<int> ExecutionCompleted;
    public event Action PracticeResetRequested;

    public InstructionDefinition CurrentInstruction => m_CurrentInstruction;
    public bool IsPhaseActive => m_IsPhaseActive;
    public bool HasProducedResult => m_HasProducedResult;
    public bool IsAwaitingContinue => m_IsAwaitingContinue;
    public string CurrentAluOpValue => m_CurrentAluOpValue;
    public string CurrentAluSrcValue => m_CurrentAluSrcValue;
    public AluOperation SelectedFunctOperation => m_SelectedFunctOperation;
    public bool HasExplicitFunctSelection => m_HasExplicitFunctSelection;
    public int LastResultValue => m_LastResultValue;
    public LessonMode CurrentMode => m_CurrentMode;
    public bool IsPracticeMode => m_CurrentMode == LessonMode.Practice;
    public bool IsAssessmentMode => LessonModePolicy.IsAssessmentMode(m_CurrentMode);
    public bool IsPracticeAwaitingReset => m_PracticeFlow.IsAwaitingReset;
    public AluInputScanner InputA => m_InputA;
    public AluInputScanner InputB => m_InputB;
    public TMP_Dropdown FunctDropdown => m_InteractionPanel.FunctDropdown;
    public TMP_Dropdown HintDropdown => m_HintPanel.InfoDropdown;
    public TMP_Text LessonRuntimeText => m_LessonPanel.RuntimeText;
    public TMP_Text AluOpStatusText => m_InteractionPanel.AluOpStatusText;
    public TMP_Text AluSrcStatusText => m_InteractionPanel.AluSrcStatusText;
    public TMP_Text Input1StatusText => m_InteractionPanel.Input1StatusText;
    public TMP_Text Input2StatusText => m_InteractionPanel.Input2StatusText;
    public TMP_Text FeedbackText => m_SharedInteraction.FeedbackText;
    public TMP_Text ExecuteButtonLabel => m_SharedInteraction.ActionLabel;
    public TMP_Text OperationLabelText => m_OperationLabelText;
    public Button ExecuteButton => m_SharedInteraction.ActionButton;
    public TMP_Text HintAluOpText => m_LearningHints.AluOpText;
    public TMP_Text HintAluSrcText => m_LearningHints.AluSrcText;
    public TMP_Text HintAluControlText => m_LearningHints.AluControlText;
    public Button PracticeHintButton => m_HintPanel.HintButton;
    public TMP_Text PracticeHintText => m_HintPanel.HintText;
    public PhaseHintPanelRefs HintPanel => m_HintPanel;
    public GameObject LessonPanelRoot => m_LessonPanel.Root;
    public GameObject HintPanelRoot => m_HintPanel.Root;
    public string ExecuteButtonText => m_ExecuteButtonText;
    public string ResultReadyButtonText => m_ResultReadyButtonText;
    public float ResultSpawnDelaySeconds => m_ResultSpawnDelaySeconds;
    public Transform ResultSpawnTransform => m_ResultSpawnTransform;
    public DataPacketToken ResultPacketPrefab => m_ResultPacketPrefab;
    public DataPacketToken SpawnedResultPacket => m_SpawnedResultPacket;
    public ParticleSystem ComputeParticles => m_ComputeParticles;
    public Coroutine ComputeRoutine => m_ComputeRoutine;

    void Awake()
    {
        m_ExecutionService = new AluExecutionService();
        ConfigurePracticeFlow();
        CacheLocalReferences();
        HookRuntimeBindings(true);
        AluPresentation.PopulateHintDropdown(HintDropdown);
        RefreshPresentation();
        SetFeedback(string.Empty, false);
    }

    void OnEnable()
    {
        m_ExecutionService ??= new AluExecutionService();
        ConfigurePracticeFlow();
        CacheLocalReferences();
        HookRuntimeBindings(true);
        AluPresentation.PopulateHintDropdown(HintDropdown);
        RefreshPresentation();
    }

    void OnDisable()
    {
        HookRuntimeBindings(false);
    }

    /// <summary>
    /// Polls the dynamic ALU input status while the phase is active so the UI
    /// can react to scanner-side issue changes that do not raise events.
    /// </summary>
    void Update()
    {
        if (!m_IsPhaseActive || m_HasProducedResult || m_ComputeRoutine != null)
            return;

        RefreshPresentation();
    }

    /// <summary>
    /// Activates or deactivates the ALU phase for the current instruction.
    /// </summary>
    public void SetPhaseState(bool isActive, LessonMode lessonMode, InstructionDefinition instruction)
    {
        CacheLocalReferences();

        var instructionChanged = instruction != null && instruction != m_CurrentInstruction;
        var modeChanged = lessonMode != m_CurrentMode;
        var isEnteringPhase = isActive && !m_IsPhaseActive;

        m_IsPhaseActive = isActive;
        m_CurrentMode = lessonMode;
        m_CurrentInstruction = instruction != null ? instruction : InstructionDefaults.CreateFallbackAdd();

        if (isEnteringPhase || instructionChanged || modeChanged)
        {
            ConfigurePracticeFlow();
            m_PracticeFlow.Reset();
            m_ExecutionService.PrepareForExecution(this);
        }

        if (m_AluUiRoot != null)
            m_AluUiRoot.SetActive(isActive);

        m_InputA?.SetActive(isActive);
        m_InputB?.SetActive(isActive);
        RefreshPresentation();
    }

    /// <summary>
    /// Clears all runtime ALU state between lesson runs.
    /// </summary>
    public void ResetExecutionState()
    {
        if (m_ComputeRoutine != null)
        {
            StopCoroutine(m_ComputeRoutine);
            m_ComputeRoutine = null;
        }

        if (m_ComputeParticles != null)
            m_ComputeParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        m_CurrentInstruction = null;
        m_CurrentAluOpValue = "00";
        m_CurrentAluSrcValue = "0";
        m_HasProducedResult = false;
        m_IsAwaitingContinue = false;
        m_LastResultValue = 0;
        m_IsPhaseActive = false;
        m_HasExplicitFunctSelection = false;
        m_SelectedFunctOperation = AluOperation.Add;
        m_CurrentMode = LessonMode.Learning;
        m_PracticeFlow.Reset();

        if (FunctDropdown != null)
        {
            FunctDropdown.SetValueWithoutNotify(0);
            FunctDropdown.RefreshShownValue();
        }

        m_InputA?.ResetScanner();
        m_InputB?.ResetScanner();
        m_ExecutionService?.ClearSpawnedResultPacket(this);
        SetFeedback(string.Empty, false);
        RefreshPresentation();

        if (m_AluUiRoot != null)
            m_AluUiRoot.SetActive(false);
    }

    /// <summary>
    /// Handles the authored execute / continue button.
    /// </summary>
    public void HandleExecutePressed()
    {
        if (!m_IsPhaseActive || m_ComputeRoutine != null)
            return;

        if (m_PracticeFlow.IsAwaitingReset)
        {
            PracticeResetRequested?.Invoke();
            return;
        }

        if (m_HasProducedResult && m_IsAwaitingContinue)
        {
            m_IsAwaitingContinue = false;
            ExecutionCompleted?.Invoke(m_LastResultValue);
            return;
        }

        if (!m_ExecutionService.TryValidateExecutionSetup(this, out var validationMessage))
        {
            if (IsAssessmentMode)
            {
                var didFail = m_PracticeFlow.HandleValidationFailure(validationMessage, out var practiceFeedback);
                if (didFail)
                    EnterPracticeFailureState(practiceFeedback);
                else
                    SetFeedback(practiceFeedback, true);
            }
            else
            {
                SetFeedback(validationMessage, true);
            }

            RefreshPresentation();
            return;
        }

        SetFeedback($"Executing {AluPresentation.GetOperationDisplayName(this, m_ExecutionService)}...", false);
        PlayPhaseCompletedCue();
        RefreshPresentation();
        m_ComputeRoutine = StartCoroutine(m_ExecutionService.RunExecutionRoutine(this));
    }

    /// <summary>
    /// Toggles the physical ALUOp button through its authored three-state
    /// cycle.
    /// </summary>
    public void HandleAluOpPressed(SelectEnterEventArgs _)
    {
        if (!m_IsPhaseActive || m_HasProducedResult)
            return;

        m_CurrentAluOpValue = m_CurrentAluOpValue switch
        {
            "00" => "01",
            "01" => "10",
            _ => "00",
        };

        SetFeedback(string.Empty, false);
        RefreshPresentation();
    }

    /// <summary>
    /// Handles the authored ALUSrc toggle and re-evaluates input 2 whenever the
    /// second operand source changes.
    /// </summary>
    public void HandleAluSrcPressed(SelectEnterEventArgs _)
    {
        if (!m_IsPhaseActive || m_HasProducedResult)
            return;

        m_CurrentAluSrcValue = m_CurrentAluSrcValue == "1" ? "0" : "1";
        m_ExecutionService.RefreshExpectedInputRoles(this, true);
        SetFeedback(string.Empty, false);
        RefreshPresentation();
    }

    /// <summary>
    /// Stores the learner's explicit R-type funct selection.
    /// </summary>
    public void HandleFunctDropdownChanged(int selectedIndex)
    {
        m_SelectedFunctOperation = m_ExecutionService.GetDropdownOperation(FunctDropdown, selectedIndex);
        m_HasExplicitFunctSelection = true;
        SetFeedback(string.Empty, false);
        RefreshPresentation();
    }

    /// <summary>
    /// Rebuilds hint visibility whenever the hint dropdown changes.
    /// </summary>
    public void HandleHintDropdownChanged(int _)
    {
        RefreshPresentation();
    }

    /// <summary>
    /// Reveals the next Practice-mode hint without affecting learning-mode
    /// dropdown hints.
    /// </summary>
    public void HandlePracticeHintPressed()
    {
        if (!IsPracticeMode)
            return;

        if (PracticeHintText != null)
            PracticeHintText.text = m_PracticeFlow.BuildHint(this, m_ExecutionService);

        RefreshPresentation();
    }

    /// <summary>
    /// Clears stale feedback whenever one of the ALU inputs accepts a packet.
    /// </summary>
    public void HandlePacketAccepted(AluInputScanner _, DataPacketToken __)
    {
        SetFeedback(string.Empty, false);
        RefreshPresentation();
    }

    /// <summary>
    /// Charges Practice-mode scanner budgets when a stable but invalid packet
    /// is placed on one of the ALU inputs.
    /// </summary>
    public void HandlePacketRejected(AluInputScanner scanner, DataPacketToken packetToken, AluInputScanner.PacketIssue issue)
    {
        if (!IsAssessmentMode || !m_IsPhaseActive || m_HasProducedResult || IsPracticeAwaitingReset)
            return;

        var didFail = m_PracticeFlow.HandleScannerFailure(scanner, packetToken, issue, out var feedbackText);
        if (didFail)
            EnterPracticeFailureState(feedbackText);
        else
            SetFeedback(feedbackText, true);
        RefreshPresentation();
    }

    /// <summary>
    /// Builds the full ALU presentation from the current runtime state.
    /// </summary>
    public void RefreshPresentation()
    {
        m_ExecutionService.RefreshExpectedInputRoles(this, false);
        AluPresentation.Refresh(this, m_ExecutionService);
    }

    /// <summary>
    /// Updates the shared ALU feedback field using the authored success and
    /// failure colors.
    /// </summary>
    public void SetFeedback(string message, bool isFailure, bool playIncorrectCue = true)
    {
        AluPresentation.SetFeedback(FeedbackText, message, isFailure, m_SuccessFeedbackColor, m_FailureFeedbackColor);

        if (playIncorrectCue && isFailure && !string.IsNullOrWhiteSpace(message))
            PlayIncorrectCue();
    }

    public void SetCurrentInstruction(InstructionDefinition instruction) => m_CurrentInstruction = instruction;
    public void SetProducedResultState(bool hasProducedResult, bool isAwaitingContinue, int lastResultValue)
    {
        m_HasProducedResult = hasProducedResult;
        m_IsAwaitingContinue = isAwaitingContinue;
        m_LastResultValue = lastResultValue;
    }

    public void SetSpawnedResultPacket(DataPacketToken resultPacket) => m_SpawnedResultPacket = resultPacket;
    public void SetComputeRoutine(Coroutine routine) => m_ComputeRoutine = routine;
    public void SetCurrentAluOpValue(string aluOpValue) => m_CurrentAluOpValue = aluOpValue;
    public void SetCurrentAluSrcValue(string aluSrcValue) => m_CurrentAluSrcValue = aluSrcValue;
    public void SetSelectedFunctOperation(AluOperation selectedOperation, bool hasExplicitSelection)
    {
        m_SelectedFunctOperation = selectedOperation;
        m_HasExplicitFunctSelection = hasExplicitSelection;
    }

    public void ShowPracticeBudgetSummary()
    {
        if (!IsAssessmentMode)
            return;

        SetFeedback(m_PracticeFlow.BuildBudgetSummary("Set the ALU controls and inputs, then validate."), false);
    }

    /// <summary>
    /// Centralizes all ALU-side event wiring so enable/disable paths stay
    /// symmetrical.
    /// </summary>
    void HookRuntimeBindings(bool subscribe)
    {
        HookPhysicalButtons(subscribe);
        HookInputEvents(subscribe);
    }

    /// <summary>
    /// Wires the authored physical ALUOp / ALUSrc buttons.
    /// </summary>
    void HookPhysicalButtons(bool subscribe)
    {
        if (subscribe)
        {
            BinarySignalButtonBinder.Bind(m_AluOpButtonRoot, HandleAluOpPressed);
            BinarySignalButtonBinder.Bind(m_AluSrcButtonRoot, HandleAluSrcPressed);
        }
        else
        {
            BinarySignalButtonBinder.Unbind(m_AluOpButtonRoot, HandleAluOpPressed);
            BinarySignalButtonBinder.Unbind(m_AluSrcButtonRoot, HandleAluSrcPressed);
        }
    }

    /// <summary>
    /// Wires scanner acceptance callbacks for both ALU inputs.
    /// </summary>
    void HookInputEvents(bool subscribe)
    {
        HookInputEvent(m_InputA, subscribe);
        HookInputEvent(m_InputB, subscribe);
    }

    /// <summary>
    /// Applies subscribe / unsubscribe behavior to a single ALU input scanner.
    /// </summary>
    void HookInputEvent(AluInputScanner inputScanner, bool subscribe)
    {
        if (inputScanner == null)
            return;

        inputScanner.PacketAccepted -= HandlePacketAccepted;
        inputScanner.PacketRejected -= HandlePacketRejected;
        if (subscribe)
        {
            inputScanner.PacketAccepted += HandlePacketAccepted;
            inputScanner.PacketRejected += HandlePacketRejected;
        }
    }

    /// <summary>
    /// Fills in optional local references from the authored ALU hierarchy so
    /// prefab duplication remains lightweight.
    /// </summary>
    void CacheLocalReferences()
    {
        m_InputA ??= FindChildComponent<AluInputScanner>("Input 1");
        m_InputB ??= FindChildComponent<AluInputScanner>("Input 2");
        m_OperationLabelText ??= FindChildText("Screen Canvas/Operation Label");
        m_ResultSpawnTransform ??= transform.Find("Data Packet Spawn");
        m_ComputeParticles ??= GetComponentInChildren<ParticleSystem>(true);
        m_AluOpButtonRoot ??= transform.Find("ALUOp Button");
        m_AluSrcButtonRoot ??= transform.Find("ALUSrc Button");
    }

    /// <summary>
    /// Looks for a named child under the authored visuals first, then falls
    /// back to a direct child of the ALU root.
    /// </summary>
    T FindChildComponent<T>(string childName) where T : Component
    {
        var childTransform = transform.Find($"Visuals/{childName}");
        if (childTransform == null)
            childTransform = transform.Find(childName);

        return childTransform != null ? childTransform.GetComponent<T>() : null;
    }

    /// <summary>
    /// Resolves a text component from a known child path in the authored ALU
    /// hierarchy.
    /// </summary>
    TMP_Text FindChildText(string childPath)
    {
        var childTransform = transform.Find(childPath);
        return childTransform != null ? childTransform.GetComponent<TMP_Text>() : null;
    }

    void ConfigurePracticeFlow()
    {
        m_PracticeFlow.Configure(
            LessonModePolicy.ResolveValidationAttempts(m_CurrentMode, m_PracticeValidationAttempts),
            LessonModePolicy.ResolveScannerAttempts(m_CurrentMode, m_PracticeScannerAttempts),
            LessonModePolicy.ResolveHintAttempts(m_CurrentMode, m_PracticeHints));
    }

    public void PlayPhaseActivatedCue()
    {
        m_LessonAudioCues.PlayPhaseActivatedCue();
    }

    public void PlayPhaseCompletedCue()
    {
        m_LessonAudioCues.PlayPhaseCompletedCue();
    }

    public void PlayIncorrectCue()
    {
        m_LessonAudioCues.PlayIncorrectCue();
    }

    public void PlayLessonCompletedCue()
    {
        m_LessonAudioCues.PlayLessonCompletedCue();
    }

    public void PlayFailureCue()
    {
        m_LessonAudioCues.PlayFailureCue();
    }

    /// <summary>
    /// Dev-mode helper that stamps the expected ALU result into the scene and
    /// forwards the phase completion without requiring manual setup.
    /// </summary>
    public void DevForceCompletePhase(int resultValue)
    {
        if (!m_IsPhaseActive || m_CurrentInstruction == null)
            return;

        m_ExecutionService.SpawnResultPacket(this, resultValue);
        SetProducedResultState(true, false, resultValue);
        SetFeedback(AluPresentation.BuildPostExecuteFeedback(m_CurrentInstruction, resultValue), false);
        ExecutionCompleted?.Invoke(resultValue);
        RefreshPresentation();
    }

    /// <summary>
    /// Mirrors the normal two-step phase ending pattern for Practice failure:
    /// play the failure cue now, then wait for Restart on the next press.
    /// </summary>
    void EnterPracticeFailureState(string feedbackText)
    {
        m_IsAwaitingContinue = false;
        m_InputA?.SetActive(false);
        m_InputB?.SetActive(false);
        SetFeedback(m_PracticeFlow.BuildFailureResetText(feedbackText), true, false);
        PlayFailureCue();
    }
}

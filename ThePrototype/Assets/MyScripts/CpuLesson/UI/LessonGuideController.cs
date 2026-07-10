using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Root coordinator for the authored lesson guide panels.
/// It binds the flow model, the authored panel controllers, and the phase-specific
/// station controllers into a single runtime lesson experience.
/// </summary>
[DisallowMultipleComponent]
public sealed class LessonGuideController : MonoBehaviour
{
    const string k_LogPrefix = "[LessonGuideController]";

    [Header("Lesson Flow")]
    [SerializeField]
    CpuLessonFlow m_LessonFlow;

    [Header("Button Labels")]
    [SerializeField]
    string m_StartButtonLabel = "Start Lesson";

    [SerializeField]
    string m_ContinueButtonLabel = "Continue";

    [SerializeField]
    string m_RestartButtonLabel = "Restart";

    [Header("Shared Lesson Panels")]
    [SerializeField]
    IntroPanelController m_IntroPanel;

    [SerializeField]
    DecodePanelController m_DecodePanel;

    [Header("Execution Phase")]
    [SerializeField]
    GameObject m_ExecutePanelRoot;

    [SerializeField]
    AluExecutionController m_ExecuteController;

    [Header("Memory Phase")]
    [SerializeField]
    GameObject m_MemoryPanelRoot;

    [SerializeField]
    MemoryUnitController m_MemoryController;

    [Header("Write-Back Phase")]
    [SerializeField]
    GameObject m_WriteBackPanelRoot;

    [SerializeField]
    WriteBackController m_WriteBackController;

    [Header("Program Counter Update Phase")]
    [SerializeField]
    GameObject m_PcUpdatePanelRoot;

    [SerializeField]
    PcUpdateController m_PcUpdateController;

    readonly InstructionCatalog m_InstructionCatalog = new();
    readonly LessonPhaseRouter m_PhaseRouter = new();
    readonly DecodeGuideFlow m_DecodeGuideFlow = new();
    readonly List<InstructionDefinition> m_AvailableInstructions = new();

    LessonGuideView m_View;
    bool m_IsRefreshingInstructionDropdown;
    bool m_IsRefreshingDecodeDropdowns;

    /// <summary>
    /// Prepares authored dropdown data and UI event hooks before the scene starts.
    /// </summary>
    void Awake()
    {
        EnsureView();
        RefreshInstructionLibrary();
        BindPanelInputs();
        RefreshView();
    }

    /// <summary>
    /// Subscribes runtime events every time the guide root becomes active.
    /// </summary>
    void OnEnable()
    {
        EnsureView();
        RefreshInstructionLibrary();
        BindPanelInputs();
        SubscribePhaseEvents();
        SubscribeLessonFlowEvents();
        RefreshView();
    }

    /// <summary>
    /// Unsubscribes runtime events to avoid duplicate handlers after re-enable.
    /// </summary>
    void OnDisable()
    {
        UnsubscribePhaseEvents();
        UnsubscribeLessonFlowEvents();
    }

    /// <summary>
    /// Rebuilds the authored instruction list used by the intro and decode panels.
    /// </summary>
    void RefreshInstructionLibrary()
    {
        if (m_LessonFlow == null)
            return;

        m_AvailableInstructions.Clear();
        m_AvailableInstructions.AddRange(m_InstructionCatalog.LoadAvailable(m_LessonFlow.CurrentInstruction));

        m_IntroPanel?.PopulateInstructionDropdown(m_AvailableInstructions, m_LessonFlow.CurrentInstruction, ref m_IsRefreshingInstructionDropdown);
        m_DecodePanel?.PopulateDropdowns(m_AvailableInstructions, m_LessonFlow.CurrentInstruction, ref m_IsRefreshingDecodeDropdowns);
    }

    /// <summary>
    /// Hooks the authored panel buttons to the lesson controller entry points.
    /// </summary>
    void BindPanelInputs()
    {
        m_IntroPanel?.BindAction(HandleIntroActionPressed);
        m_IntroPanel?.BindInstructionSelection(HandleInstructionChanged);
        m_DecodePanel?.BindAction(HandleDecodeActionPressed);
        m_DecodePanel?.BindSelectionDropdowns(HandleDecodeSelectionChanged);
        m_DecodePanel?.BindHintDropdown(HandleDecodeHintChanged);
    }

    /// <summary>
    /// Subscribes to events raised by the physical phase stations.
    /// </summary>
    void SubscribePhaseEvents()
    {
        if (m_ExecuteController != null)
            m_ExecuteController.ExecutionCompleted += HandleAluExecutionCompleted;

        if (m_WriteBackController != null)
        {
            m_WriteBackController.WriteBackApplied += HandleWriteBackApplied;
            m_WriteBackController.ContinueRequested += HandleWriteBackContinueRequested;
        }

        if (m_MemoryController != null)
            m_MemoryController.ContinueRequested += HandleMemoryContinueRequested;

        if (m_PcUpdateController != null)
            m_PcUpdateController.ContinueRequested += HandlePcUpdateContinueRequested;
    }

    /// <summary>
    /// Removes phase-station event subscriptions.
    /// </summary>
    void UnsubscribePhaseEvents()
    {
        if (m_ExecuteController != null)
            m_ExecuteController.ExecutionCompleted -= HandleAluExecutionCompleted;

        if (m_WriteBackController != null)
        {
            m_WriteBackController.WriteBackApplied -= HandleWriteBackApplied;
            m_WriteBackController.ContinueRequested -= HandleWriteBackContinueRequested;
        }

        if (m_MemoryController != null)
            m_MemoryController.ContinueRequested -= HandleMemoryContinueRequested;

        if (m_PcUpdateController != null)
            m_PcUpdateController.ContinueRequested -= HandlePcUpdateContinueRequested;
    }

    /// <summary>
    /// Subscribes to the underlying lesson flow state changes.
    /// </summary>
    void SubscribeLessonFlowEvents()
    {
        if (m_LessonFlow == null)
            return;

        m_LessonFlow.StepChanged += HandleStepChanged;
        m_LessonFlow.FeedbackChanged += HandleFeedbackChanged;
    }

    /// <summary>
    /// Removes lesson flow event subscriptions.
    /// </summary>
    void UnsubscribeLessonFlowEvents()
    {
        if (m_LessonFlow == null)
            return;

        m_LessonFlow.StepChanged -= HandleStepChanged;
        m_LessonFlow.FeedbackChanged -= HandleFeedbackChanged;
    }

    /// <summary>
    /// Starts the lesson or advances the intro/fetch panel when its action button is pressed.
    /// </summary>
    void HandleIntroActionPressed()
    {
        if (m_LessonFlow == null)
            return;

        Debug.Log($"{k_LogPrefix} Intro button pressed | hasStarted={m_LessonFlow.HasStarted} step={m_LessonFlow.CurrentStep?.stepName} frame={Time.frameCount}", this);

        if (!m_LessonFlow.HasStarted)
            m_LessonFlow.StartLesson();
        else
            m_LessonFlow.Advance();
    }

    /// <summary>
    /// Routes the decode button to either opcode/funct validation or normal lesson advancement.
    /// </summary>
    void HandleDecodeActionPressed()
    {
        if (m_LessonFlow == null)
            return;

        Debug.Log($"{k_LogPrefix} Decode button pressed | hasStarted={m_LessonFlow.HasStarted} step={m_LessonFlow.CurrentStep?.stepName} frame={Time.frameCount}", this);

        if (!m_LessonFlow.HasStarted)
        {
            m_LessonFlow.StartLesson();
            return;
        }

        m_DecodeGuideFlow.HandleContinue(m_LessonFlow, m_DecodePanel, HandleFeedbackChanged, ref m_IsRefreshingDecodeDropdowns);
        RefreshView();
    }

    /// <summary>
    /// Updates the selected instruction from the intro dropdown and keeps decode choices in sync.
    /// </summary>
    void HandleInstructionChanged(int selectedIndex)
    {
        if (m_IsRefreshingInstructionDropdown || m_LessonFlow == null)
            return;

        if (selectedIndex < 0 || selectedIndex >= m_AvailableInstructions.Count)
            return;

        m_LessonFlow.SetCurrentInstruction(m_AvailableInstructions[selectedIndex]);
        m_DecodePanel?.PopulateDropdowns(m_AvailableInstructions, m_LessonFlow.CurrentInstruction, ref m_IsRefreshingDecodeDropdowns);
        RefreshView();
    }

    /// <summary>
    /// Rebuilds decode text whenever the learner changes opcode or funct selections.
    /// </summary>
    void HandleDecodeSelectionChanged(int _)
    {
        if (m_IsRefreshingDecodeDropdowns)
            return;

        RefreshView();
    }

    /// <summary>
    /// Refreshes the decode hint panel whenever the learner chooses a different help topic.
    /// </summary>
    void HandleDecodeHintChanged(int _)
    {
        if (m_IsRefreshingDecodeDropdowns)
            return;

        m_DecodePanel?.RefreshHintText(m_AvailableInstructions);
    }

    /// <summary>
    /// Re-renders the guide whenever the lesson flow changes step.
    /// </summary>
    void HandleStepChanged(CpuLessonFlow _)
    {
        Debug.Log($"{k_LogPrefix} StepChanged | step={m_LessonFlow?.CurrentStep?.stepName} frame={Time.frameCount}", this);
        RefreshView();
    }

    /// <summary>
    /// Forwards the ALU result into the lesson flow state machine.
    /// </summary>
    void HandleAluExecutionCompleted(int resultValue)
    {
        m_LessonFlow?.CompleteAluExecution(resultValue);
    }

    /// <summary>
    /// Forwards a successful write-back result into the lesson flow state machine.
    /// </summary>
    void HandleWriteBackApplied(string destinationRegister, int resultValue)
    {
        m_LessonFlow?.CompleteWriteBackExecution(destinationRegister, resultValue);
    }

    /// <summary>
    /// Advances the lesson once the write-back station's final continue button is pressed.
    /// </summary>
    void HandleWriteBackContinueRequested()
    {
        m_LessonFlow?.Advance();
    }

    /// <summary>
    /// Advances the lesson once the memory station's final continue button is pressed.
    /// </summary>
    void HandleMemoryContinueRequested()
    {
        m_LessonFlow?.Advance();
    }

    /// <summary>
    /// Resets the lesson once the program-counter update station completes the walkthrough.
    /// </summary>
    void HandlePcUpdateContinueRequested()
    {
        m_LessonFlow?.ResetLesson();
    }

    /// <summary>
    /// Routes shared lesson feedback to whichever authored panel is currently active.
    /// </summary>
    void HandleFeedbackChanged(string message, bool isFailure)
    {
        EnsureView();
        m_View?.RouteFeedback(m_LessonFlow, message, isFailure, m_AvailableInstructions);
    }

    /// <summary>
    /// Re-evaluates every authored panel and phase station from the current lesson state.
    /// </summary>
    void RefreshView()
    {
        EnsureView();

        if (m_LessonFlow == null || m_View == null)
            return;

        Debug.Log(
            $"{k_LogPrefix} RefreshView | step={m_LessonFlow.CurrentStep?.stepName} frame={Time.frameCount}",
            this);

        m_View.Refresh(
            m_LessonFlow,
            m_AvailableInstructions,
            m_DecodeGuideFlow,
            m_StartButtonLabel,
            m_ContinueButtonLabel,
            m_RestartButtonLabel,
            ref m_IsRefreshingDecodeDropdowns);
    }

    /// <summary>
    /// Lazily creates the view helper that owns the heavy panel presentation logic.
    /// </summary>
    void EnsureView()
    {
        if (m_View != null)
            return;

        m_View = new LessonGuideView(
            m_PhaseRouter,
            m_IntroPanel,
            m_DecodePanel,
            m_ExecutePanelRoot,
            m_ExecuteController,
            m_MemoryPanelRoot,
            m_MemoryController,
            m_WriteBackPanelRoot,
            m_WriteBackController,
            m_PcUpdatePanelRoot,
            m_PcUpdateController);
    }
}

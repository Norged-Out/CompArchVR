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

    [SerializeField]
    string m_GoBackButtonLabel = "Go Back";

    [Header("Shared Lesson Panels")]
    [SerializeField]
    IntroPanelController m_IntroPanel;

    [SerializeField]
    DecodePanelController m_DecodePanel;

    [Header("Execution Phase")]
    [SerializeField]
    GameObject m_ExecutePanelRoot;

    [SerializeField]
    AluController m_ExecuteController;

    [Header("Memory Phase")]
    [SerializeField]
    GameObject m_MemoryPanelRoot;

    [SerializeField]
    MemoryController m_MemoryController;

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
    readonly List<PracticeInstructionDefinition> m_AvailablePracticeInstructions = new();
    readonly List<string> m_IntroSelectionLabels = new();

    LessonGuideView m_View;
    bool m_IsRefreshingModeDropdown;
    bool m_IsRefreshingInstructionDropdown;
    bool m_IsRefreshingDecodeDropdowns;
    LessonCuePhase m_LastCuePhase = LessonCuePhase.None;

    /// <summary>
    /// Prepares authored dropdown data and UI event hooks before the scene starts.
    /// </summary>
    void Awake()
    {
        EnsureView();
        RefreshInstructionLibrary();
        SyncLessonCueState();
        RefreshView();
    }

    /// <summary>
    /// Subscribes runtime events every time the guide root becomes active.
    /// </summary>
    void OnEnable()
    {
        EnsureView();
        RefreshInstructionLibrary();
        SubscribePhaseEvents();
        SubscribeLessonFlowEvents();
        SyncLessonCueState();
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
    /// Rebuilds the authored intro selections and keeps the decode panel synced to
    /// the Learning instruction bank that still powers the current runtime flow.
    /// </summary>
    void RefreshInstructionLibrary()
    {
        if (m_LessonFlow == null)
            return;

        m_IntroPanel?.PopulateModeDropdown(m_LessonFlow.CurrentMode, ref m_IsRefreshingModeDropdown);

        m_AvailableInstructions.Clear();
        m_AvailableInstructions.AddRange(m_InstructionCatalog.LoadAvailable(LessonMode.Learning, m_LessonFlow.CurrentInstruction));

        m_AvailablePracticeInstructions.Clear();
        m_AvailablePracticeInstructions.AddRange(m_InstructionCatalog.LoadPracticeAvailable(m_LessonFlow.CurrentPracticeInstruction));

        BuildIntroSelectionLabels(out var currentSelectionIndex);

        m_IntroPanel?.PopulateSelectionDropdown(m_IntroSelectionLabels, currentSelectionIndex, ref m_IsRefreshingInstructionDropdown);
        m_DecodePanel?.PopulateDropdowns(m_AvailableInstructions, m_LessonFlow.CurrentInstruction, ref m_IsRefreshingDecodeDropdowns);
    }

    /// <summary>
    /// Rebuilds the intro dropdown labels for the currently selected lesson mode.
    /// </summary>
    void BuildIntroSelectionLabels(out int currentSelectionIndex)
    {
        m_IntroSelectionLabels.Clear();
        currentSelectionIndex = 0;

        if (m_LessonFlow == null)
            return;

        if (m_LessonFlow.CurrentMode == LessonMode.Learning)
        {
            for (var index = 0; index < m_AvailableInstructions.Count; index++)
            {
                var instruction = m_AvailableInstructions[index];
                m_IntroSelectionLabels.Add(instruction != null ? instruction.displayName : "Instruction");

                if (instruction == m_LessonFlow.CurrentInstruction)
                    currentSelectionIndex = index;
            }

            return;
        }

        for (var index = 0; index < m_AvailablePracticeInstructions.Count; index++)
        {
            var instruction = m_AvailablePracticeInstructions[index];
            m_IntroSelectionLabels.Add(instruction != null ? instruction.displayName : "Practice Instruction");

            if (instruction == m_LessonFlow.CurrentPracticeInstruction)
                currentSelectionIndex = index;
        }
    }

    /// <summary>
    /// Updates the active lesson mode and refreshes the second intro dropdown so it
    /// shows the correct content bank for that mode.
    /// </summary>
    public void HandleModeChanged(int selectedMode)
    {
        if (m_IsRefreshingModeDropdown || m_LessonFlow == null)
            return;

        m_LessonFlow.SetLessonModeFromDropdown(selectedMode);
        RefreshInstructionLibrary();
        SyncLessonCueState();
        RefreshView();
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
        {
            m_MemoryController.ContinueRequested += HandleMemoryContinueRequested;
            m_MemoryController.MemoryTransferCompleted += HandleMemoryTransferCompleted;
        }

        if (m_PcUpdateController != null)
        {
            m_PcUpdateController.ContinueRequested += HandlePcUpdateContinueRequested;
            m_PcUpdateController.PcUpdateConfirmed += HandlePcUpdateConfirmed;
        }
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
        {
            m_MemoryController.ContinueRequested -= HandleMemoryContinueRequested;
            m_MemoryController.MemoryTransferCompleted -= HandleMemoryTransferCompleted;
        }

        if (m_PcUpdateController != null)
        {
            m_PcUpdateController.ContinueRequested -= HandlePcUpdateContinueRequested;
            m_PcUpdateController.PcUpdateConfirmed -= HandlePcUpdateConfirmed;
        }
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
    public void HandleIntroActionPressed()
    {
        if (m_LessonFlow == null)
            return;

        Debug.Log($"{k_LogPrefix} Intro button pressed | hasStarted={m_LessonFlow.HasStarted} step={m_LessonFlow.CurrentStep?.stepName} frame={Time.frameCount}", this);

        if (!m_LessonFlow.HasStarted)
            m_LessonFlow.StartLesson();
        else if (m_LessonFlow.IsFetchStepActive)
            m_LessonFlow.ResetLesson();
        else
            m_LessonFlow.Advance();
    }

    /// <summary>
    /// Routes the decode button to either opcode/funct validation or normal lesson advancement.
    /// </summary>
    public void HandleDecodeActionPressed()
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
    public void HandleInstructionChanged(int selectedIndex)
    {
        if (m_IsRefreshingInstructionDropdown || m_LessonFlow == null)
            return;

        switch (m_LessonFlow.CurrentMode)
        {
            case LessonMode.Learning:
                if (selectedIndex < 0 || selectedIndex >= m_AvailableInstructions.Count)
                    return;

                m_LessonFlow.SetCurrentInstruction(m_AvailableInstructions[selectedIndex]);
                m_DecodePanel?.PopulateDropdowns(m_AvailableInstructions, m_LessonFlow.CurrentInstruction, ref m_IsRefreshingDecodeDropdowns);
                break;

            case LessonMode.Practice:
            case LessonMode.Test:
                if (selectedIndex < 0 || selectedIndex >= m_AvailablePracticeInstructions.Count)
                    return;

                m_LessonFlow.SetCurrentPracticeInstruction(m_AvailablePracticeInstructions[selectedIndex]);
                break;
        }

        RefreshView();
    }

    /// <summary>
    /// Rebuilds decode text whenever the learner changes opcode or funct selections.
    /// </summary>
    public void HandleDecodeSelectionChanged(int _)
    {
        if (m_IsRefreshingDecodeDropdowns)
            return;

        RefreshView();
    }

    /// <summary>
    /// Refreshes the decode hint panel whenever the learner chooses a different help topic.
    /// </summary>
    public void HandleDecodeHintChanged(int _)
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
        HandlePhaseCueTransition();
    }

    /// <summary>
    /// Forwards the ALU result into the lesson flow state machine.
    /// </summary>
    void HandleAluExecutionCompleted(int resultValue)
    {
        m_LessonFlow?.CompleteAluExecution(resultValue);
    }

    /// <summary>
    /// Plays the shared phase-clear cue when Memory Access finishes its transfer.
    /// </summary>
    void HandleMemoryTransferCompleted()
    {
        m_MemoryController?.PlayPhaseCompletedCue();
    }

    /// <summary>
    /// Forwards a successful write-back result into the lesson flow state machine.
    /// </summary>
    void HandleWriteBackApplied(string destinationRegister, int resultValue)
    {
        if (m_LessonFlow != null &&
            m_LessonFlow.CurrentInstruction != null &&
            !m_LessonFlow.CurrentInstruction.UsesPcUpdatePhase())
        {
            m_WriteBackController?.PlayLessonCompletedCue();
        }
        else
        {
            m_WriteBackController?.PlayPhaseCompletedCue();
        }

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
    /// Treats a confirmed Program Counter update as the end of the current walkthrough.
    /// </summary>
    void HandlePcUpdateConfirmed()
    {
        m_PcUpdateController?.PlayLessonCompletedCue();
    }

    /// <summary>
    /// Routes shared lesson feedback to whichever authored panel is currently active.
    /// </summary>
    void HandleFeedbackChanged(string message, bool isFailure)
    {
        if (isFailure && !string.IsNullOrWhiteSpace(message))
            PlayIncorrectCueForCurrentOwner();

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
            m_GoBackButtonLabel,
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

    void SyncLessonCueState()
    {
        m_LastCuePhase = ResolveCuePhase();
    }

    void HandlePhaseCueTransition()
    {
        var currentPhase = ResolveCuePhase();
        var previousPhase = m_LastCuePhase;

        if (currentPhase == previousPhase)
        {
            RefreshView();
            return;
        }

        PlayPhaseCompletedCue(previousPhase, currentPhase);
        RefreshView();
        PlayPhaseActivatedCue(currentPhase);

        m_LastCuePhase = currentPhase;
    }

    LessonCuePhase ResolveCuePhase()
    {
        if (m_LessonFlow == null)
            return LessonCuePhase.None;

        if (!m_LessonFlow.HasStarted)
            return LessonCuePhase.BeforeStart;

        var currentStep = m_LessonFlow.CurrentStep;
        if (currentStep == null)
            return LessonCuePhase.None;

        if (currentStep.requiredInteraction == InstructionStepInteractionType.Completion)
            return LessonCuePhase.Completion;

        if (m_PhaseRouter.ShouldShowPcUpdatePanel(m_LessonFlow))
            return LessonCuePhase.PcUpdate;

        if (m_PhaseRouter.ShouldShowWriteBackPanel(m_LessonFlow))
            return LessonCuePhase.WriteBack;

        if (m_PhaseRouter.ShouldShowMemoryPanel(m_LessonFlow))
            return LessonCuePhase.Memory;

        if (m_PhaseRouter.ShouldShowExecutionPanel(m_LessonFlow))
            return LessonCuePhase.Execute;

        if (m_PhaseRouter.ShouldShowDecodePanel(m_LessonFlow))
            return LessonCuePhase.Decode;

        if (m_PhaseRouter.ShouldShowIntroPanel(m_LessonFlow))
            return LessonCuePhase.Fetch;

        return LessonCuePhase.None;
    }

    void PlayPhaseCompletedCue(LessonCuePhase previousPhase, LessonCuePhase currentPhase)
    {
        // Fetch transitions straight into Decode at the terminal, so it needs
        // an explicit completion cue before the shared intro panel is hidden.
        if (previousPhase == LessonCuePhase.Fetch && currentPhase == LessonCuePhase.Decode)
            m_IntroPanel?.PlayPhaseCompletedCue();
    }

    void PlayPhaseActivatedCue(LessonCuePhase phase)
    {
        switch (phase)
        {
            case LessonCuePhase.Fetch:
                m_IntroPanel?.PlayPhaseActivatedCue();
                break;

            case LessonCuePhase.Decode:
                m_DecodePanel?.PlayPhaseActivatedCue();
                break;

            case LessonCuePhase.Execute:
                m_ExecuteController?.PlayPhaseActivatedCue();
                break;

            case LessonCuePhase.Memory:
                m_MemoryController?.PlayPhaseActivatedCue();
                break;

            case LessonCuePhase.WriteBack:
                m_WriteBackController?.PlayPhaseActivatedCue();
                break;

            case LessonCuePhase.PcUpdate:
                m_PcUpdateController?.PlayPhaseActivatedCue();
                break;
        }
    }

    void PlayIncorrectCueForCurrentOwner()
    {
        switch (ResolveCuePhase())
        {
            case LessonCuePhase.Fetch:
                m_IntroPanel?.PlayIncorrectCue();
                break;

            case LessonCuePhase.Decode:
                m_DecodePanel?.PlayIncorrectCue();
                break;

            case LessonCuePhase.Execute:
                m_ExecuteController?.PlayIncorrectCue();
                break;

            case LessonCuePhase.Memory:
                m_MemoryController?.PlayIncorrectCue();
                break;

            case LessonCuePhase.WriteBack:
                m_WriteBackController?.PlayIncorrectCue();
                break;

            case LessonCuePhase.PcUpdate:
                m_PcUpdateController?.PlayIncorrectCue();
                break;
        }
    }

    enum LessonCuePhase
    {
        None,
        BeforeStart,
        Fetch,
        Decode,
        Execute,
        Memory,
        WriteBack,
        PcUpdate,
        Completion,
    }
}

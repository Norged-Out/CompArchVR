using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Owns lesson-guide presentation rules that are shared across the intro, decode,
/// execution, memory, write-back, and PC-update panels.
/// This class is intentionally not a MonoBehaviour: the scene component keeps
/// event wiring, while this class keeps the heavy view logic and phase toggling.
/// </summary>
sealed class LessonGuideView
{
    readonly LessonPhaseRouter m_PhaseRouter;
    readonly IntroPanelController m_IntroPanel;
    readonly DecodePanelController m_DecodePanel;
    readonly GameObject m_ExecutePanelRoot;
    readonly AluController m_ExecuteController;
    readonly GameObject m_MemoryPanelRoot;
    readonly MemoryController m_MemoryController;
    readonly GameObject m_WriteBackPanelRoot;
    readonly WriteBackController m_WriteBackController;
    readonly GameObject m_PcUpdatePanelRoot;
    readonly PcUpdateController m_PcUpdateController;

    /// <summary>
    /// Captures every authored panel and physical station controlled by the lesson guide.
    /// </summary>
    public LessonGuideView(
        LessonPhaseRouter phaseRouter,
        IntroPanelController introPanel,
        DecodePanelController decodePanel,
        GameObject executePanelRoot,
        AluController executeController,
        GameObject memoryPanelRoot,
        MemoryController memoryController,
        GameObject writeBackPanelRoot,
        WriteBackController writeBackController,
        GameObject pcUpdatePanelRoot,
        PcUpdateController pcUpdateController)
    {
        m_PhaseRouter = phaseRouter;
        m_IntroPanel = introPanel;
        m_DecodePanel = decodePanel;
        m_ExecutePanelRoot = executePanelRoot;
        m_ExecuteController = executeController;
        m_MemoryPanelRoot = memoryPanelRoot;
        m_MemoryController = memoryController;
        m_WriteBackPanelRoot = writeBackPanelRoot;
        m_WriteBackController = writeBackController;
        m_PcUpdatePanelRoot = pcUpdatePanelRoot;
        m_PcUpdateController = pcUpdateController;
    }

    /// <summary>
    /// Routes shared learner feedback to the panel that currently owns the step.
    /// </summary>
    public void RouteFeedback(
        CpuLessonFlow lessonFlow,
        string message,
        bool isFailure,
        IReadOnlyList<InstructionDefinition> availableInstructions)
    {
        if (m_PhaseRouter.ShouldShowDecodePanel(lessonFlow))
        {
            m_DecodePanel?.SetFeedback(message, isFailure, availableInstructions);
            return;
        }

        if (m_PhaseRouter.ShouldShowMemoryPanel(lessonFlow) ||
            m_PhaseRouter.ShouldShowExecutionPanel(lessonFlow) ||
            m_PhaseRouter.ShouldShowPcUpdatePanel(lessonFlow))
        {
            return;
        }
    }

    /// <summary>
    /// Re-evaluates every authored panel and physical phase station from the current lesson state.
    /// </summary>
    public void Refresh(
        CpuLessonFlow lessonFlow,
        IReadOnlyList<InstructionDefinition> availableInstructions,
        PracticeInstructionDefinition currentPracticeInstruction,
        DecodeGuideFlow decodeGuideFlow,
        PracticeDecodeFlow practiceDecodeFlow,
        string startButtonLabel,
        string continueButtonLabel,
        string goBackButtonLabel,
        string restartButtonLabel,
        ref bool isRefreshingDecodeDropdowns)
    {
        if (lessonFlow == null || m_IntroPanel == null)
            return;

        var showDecodePanel = m_PhaseRouter.ShouldShowDecodePanel(lessonFlow);
        var showExecutionPanel = m_PhaseRouter.ShouldShowExecutionPanel(lessonFlow);
        var showMemoryPanel = m_PhaseRouter.ShouldShowMemoryPanel(lessonFlow);
        var showWriteBackPanel = m_PhaseRouter.ShouldShowWriteBackPanel(lessonFlow);
        var showPcUpdatePanel = m_PhaseRouter.ShouldShowPcUpdatePanel(lessonFlow);

        ApplyExecutionPanelState(showExecutionPanel, lessonFlow);
        ApplyMemoryPanelState(showMemoryPanel, lessonFlow);
        ApplyWriteBackPanelState(showWriteBackPanel, lessonFlow);
        ApplyPcUpdatePanelState(showPcUpdatePanel, lessonFlow);

        m_IntroPanel.SetVisible(m_PhaseRouter.ShouldShowIntroPanel(lessonFlow));
        m_DecodePanel?.SetVisible(showDecodePanel);

        if (!lessonFlow.HasStarted)
        {
            ResetPhasePanels();
            decodeGuideFlow.Reset(m_DecodePanel, ref isRefreshingDecodeDropdowns);
            practiceDecodeFlow.Reset(m_DecodePanel, ref isRefreshingDecodeDropdowns);
            m_IntroPanel.ShowBeforeStart(
                lessonFlow.CurrentInstruction,
                startButtonLabel,
                lessonFlow.UsesInstructionSelection,
                lessonFlow.CanStartSelectedMode);
            return;
        }

        var step = lessonFlow.CurrentStep;
        if (step == null)
            return;

        m_IntroPanel.SetInstructionDropdownInteractable(false);

        if (showExecutionPanel || showMemoryPanel || showWriteBackPanel || showPcUpdatePanel)
        {
            m_IntroPanel.HideAction();
            m_DecodePanel?.HideAction();
            return;
        }

        if (!showDecodePanel)
        {
            m_IntroPanel.ShowStep(lessonFlow, step, continueButtonLabel, goBackButtonLabel, restartButtonLabel);
            return;
        }

        m_DecodePanel?.Refresh(
            lessonFlow,
            step,
            availableInstructions,
            currentPracticeInstruction,
            decodeGuideFlow.GetSelectionMode(lessonFlow),
            practiceDecodeFlow,
            continueButtonLabel,
            restartButtonLabel);
    }

    /// <summary>
    /// Syncs the execution panel root and its physical ALU controller.
    /// </summary>
    void ApplyExecutionPanelState(bool isVisible, CpuLessonFlow lessonFlow)
    {
        if (m_ExecutePanelRoot != null)
            m_ExecutePanelRoot.SetActive(isVisible);

        m_ExecuteController?.SetPhaseState(
            isVisible,
            lessonFlow != null ? lessonFlow.CurrentMode : LessonMode.Learning,
            lessonFlow != null ? lessonFlow.CurrentInstruction : null);
    }

    /// <summary>
    /// Syncs the memory panel root and its physical memory controller.
    /// </summary>
    void ApplyMemoryPanelState(bool isVisible, CpuLessonFlow lessonFlow)
    {
        if (m_MemoryPanelRoot != null)
            m_MemoryPanelRoot.SetActive(isVisible);

        if (m_MemoryController == null)
            return;

        m_MemoryController.SetPhaseState(
            isVisible,
            lessonFlow != null ? lessonFlow.CurrentMode : LessonMode.Learning,
            lessonFlow != null ? lessonFlow.CurrentInstruction : null);
    }

    /// <summary>
    /// Syncs the write-back panel root and its physical write-back controller.
    /// </summary>
    void ApplyWriteBackPanelState(bool isVisible, CpuLessonFlow lessonFlow)
    {
        if (m_WriteBackPanelRoot != null)
            m_WriteBackPanelRoot.SetActive(isVisible);

        if (m_WriteBackController == null)
            return;

        m_WriteBackController.SetPhaseState(
            isVisible,
            lessonFlow != null ? lessonFlow.CurrentMode : LessonMode.Learning,
            lessonFlow != null ? lessonFlow.CurrentInstruction : null,
            lessonFlow != null ? lessonFlow.RegisterBank : null);

        if (!isVisible)
            m_WriteBackController.ResetWriteBackState();
    }

    /// <summary>
    /// Syncs the PC update panel root and its physical PC update controller.
    /// </summary>
    void ApplyPcUpdatePanelState(bool isVisible, CpuLessonFlow lessonFlow)
    {
        if (m_PcUpdatePanelRoot != null)
            m_PcUpdatePanelRoot.SetActive(isVisible);

        if (m_PcUpdateController == null)
            return;

        m_PcUpdateController.SetPhaseState(
            isVisible,
            lessonFlow != null ? lessonFlow.CurrentMode : LessonMode.Learning,
            lessonFlow != null ? lessonFlow.CurrentInstruction : null);
        if (!isVisible)
            m_PcUpdateController.ResetPcUpdateState();
    }

    /// <summary>
    /// Resets every physical phase station when the lesson returns to its not-started state.
    /// </summary>
    void ResetPhasePanels()
    {
        if (m_ExecutePanelRoot != null)
            m_ExecutePanelRoot.SetActive(false);

        m_ExecuteController?.ResetExecutionState();

        if (m_MemoryPanelRoot != null)
            m_MemoryPanelRoot.SetActive(false);

        m_MemoryController?.ResetMemoryState();

        if (m_WriteBackPanelRoot != null)
            m_WriteBackPanelRoot.SetActive(false);

        m_WriteBackController?.ResetWriteBackState();

        if (m_PcUpdatePanelRoot != null)
            m_PcUpdatePanelRoot.SetActive(false);

        m_PcUpdateController?.ResetPcUpdateState();
    }
}

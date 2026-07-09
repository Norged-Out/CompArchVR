using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// View orchestration for the authored lesson guide panels placed in the scene.
/// </summary>
public partial class LessonGuideController
{
    void RefreshView()
    {
        if (m_LessonFlow == null || m_IntroRoot == null)
            return;

        var showIDPanel = ShouldShowIDPanel();
        var showAluPanel = ShouldShowAluPanel();
        var showMemoryPanel = ShouldShowMemoryPanel();
        var showWriteBackPanel = ShouldShowWriteBackPanel();
        var showPcUpdatePanel = ShouldShowPcUpdatePanel();

        Debug.Log(
            $"{k_LogPrefix} RefreshView | step={m_LessonFlow.CurrentStep?.stepName} decode={showIDPanel} alu={showAluPanel} mem={showMemoryPanel} wb={showWriteBackPanel} pc={showPcUpdatePanel} frame={Time.frameCount}",
            this);

        // Panels are authored in the scene and simply toggled on/off as the
        // lesson advances. That keeps layout work in edit mode instead of runtime.
        if (m_IDRoot != null)
            m_IDRoot.SetActive(showIDPanel);

        if (m_AluRoot != null)
            m_AluRoot.SetActive(showAluPanel);

        if (m_MemRoot != null)
            m_MemRoot.SetActive(showMemoryPanel);

        if (m_WriteBackRoot != null)
            m_WriteBackRoot.SetActive(showWriteBackPanel);

        if (m_PcUpdateRoot != null)
            m_PcUpdateRoot.SetActive(showPcUpdatePanel);

        m_AluController?.SetPhaseState(showAluPanel, m_LessonFlow.CurrentInstruction);
        m_MemoryController?.SetPhaseState(showMemoryPanel, m_LessonFlow.CurrentInstruction);
        m_WriteBackController?.SetPhaseState(showWriteBackPanel, m_LessonFlow.CurrentInstruction, m_LessonFlow.RegisterBank);
        m_PcUpdateController?.SetPhaseState(showPcUpdatePanel, m_LessonFlow.CurrentInstruction);
        if (m_MemoryController != null && !showMemoryPanel)
            m_MemoryController.ResetMemoryState();
        if (m_WriteBackController != null && !showWriteBackPanel)
            m_WriteBackController.ResetWriteBackState();
        if (m_PcUpdateController != null && !showPcUpdatePanel)
            m_PcUpdateController.ResetPcUpdateState();

        m_IntroRoot.SetActive(!showIDPanel && !showAluPanel && !showMemoryPanel && !showWriteBackPanel && !showPcUpdatePanel);

        if (!m_LessonFlow.HasStarted)
        {
            m_AluController?.ResetExecutionState();
            m_MemoryController?.ResetMemoryState();
            m_WriteBackController?.ResetWriteBackState();
            if (m_IDRoot != null)
                m_IDRoot.SetActive(false);
            if (m_AluRoot != null)
                m_AluRoot.SetActive(false);
            if (m_MemRoot != null)
                m_MemRoot.SetActive(false);
            if (m_WriteBackRoot != null)
                m_WriteBackRoot.SetActive(false);
            if (m_PcUpdateRoot != null)
                m_PcUpdateRoot.SetActive(false);
            SetText(
                m_IntroBody,
                $"Lesson Introduction\n\nSelected instruction: {m_LessonFlow.CurrentInstruction?.assemblyInstructionText ?? "add t2, t0, t1"}\n\nPress Start Lesson to begin.");
            SetText(m_IntroFeedback, string.Empty);
            SetButtonState(m_IntroActionButton, m_IntroActionLabel, m_StartButtonLabel, true);
            if (m_InstructionDropdown != null)
                m_InstructionDropdown.interactable = true;
            ResetDecodeDropdowns();
            SetButtonState(m_IDActionButton, m_IDActionLabel, m_ContinueButtonLabel, false);
            RefreshLayout(m_IntroRoot, m_IntroBody, m_IntroFeedback, m_IntroActionButton);
            RefreshLayout(m_IDRoot, m_IDOpcodeSelectionText, m_IDFeedback, m_IDActionButton);
            return;
        }

        var step = m_LessonFlow.CurrentStep;
        if (step == null)
            return;

        if (m_InstructionDropdown != null)
            m_InstructionDropdown.interactable = false;

        if (showAluPanel || showWriteBackPanel || showPcUpdatePanel || showMemoryPanel)
        {
            SetButtonState(m_IntroActionButton, m_IntroActionLabel, m_ContinueButtonLabel, false);
            SetButtonState(m_IDActionButton, m_IDActionLabel, m_ContinueButtonLabel, false);
            RefreshLayout(m_IntroRoot, m_IntroBody, m_IntroFeedback, m_IntroActionButton);
            RefreshLayout(m_IDRoot, m_IDOpcodeSelectionText, m_IDFeedback, m_IDActionButton);
            return;
        }

        if (!showIDPanel)
        {
            SetText(m_IntroBody, BuildIntroBody(step));
            SetText(m_IntroFeedback, string.Empty);
            SetButtonState(
                m_IntroActionButton,
                m_IntroActionLabel,
                step.requiredInteraction == InstructionStepInteractionType.Completion ? m_RestartButtonLabel : m_ContinueButtonLabel,
                step.requiredInteraction == InstructionStepInteractionType.ContinueButton ||
                step.requiredInteraction == InstructionStepInteractionType.Completion);
            SetButtonState(m_IDActionButton, m_IDActionLabel, m_ContinueButtonLabel, false);
            RefreshLayout(m_IntroRoot, m_IntroBody, m_IntroFeedback, m_IntroActionButton);
            RefreshLayout(m_IDRoot, m_IDOpcodeSelectionText, m_IDFeedback, m_IDActionButton);
            return;
        }

        RefreshDecodeTextBlocks(step);
        RefreshDecodeDropdownState(step);
        RefreshDecodeHintText();

        var showContinue = IsDecodeOpcodeSelectionStep() ||
                           IsDecodeFunctSelectionStep() ||
                           step.requiredInteraction == InstructionStepInteractionType.ContinueButton ||
                           step.requiredInteraction == InstructionStepInteractionType.Completion ||
                           (step.requiredInteraction == InstructionStepInteractionType.RegisterSelection &&
                            m_LessonFlow.RegisterSelectionReadyToContinue);
        SetButtonState(
            m_IDActionButton,
            m_IDActionLabel,
            step.requiredInteraction == InstructionStepInteractionType.Completion ? m_RestartButtonLabel : m_ContinueButtonLabel,
            showContinue);
        SetButtonState(m_IntroActionButton, m_IntroActionLabel, m_ContinueButtonLabel, false);
        RefreshLayout(m_IntroRoot, m_IntroBody, m_IntroFeedback, m_IntroActionButton);
        RefreshLayout(m_IDRoot, m_IDOpcodeSelectionText, m_IDFeedback, m_IDActionButton);
    }

    bool ShouldShowIDPanel()
    {
        if (ShouldShowAluPanel())
            return false;

        var step = m_LessonFlow?.CurrentStep;
        if (step == null)
            return false;

        return step.highlightedNode == DatapathNodeId.InstructionMemory ||
               step.requiredInteraction == InstructionStepInteractionType.RegisterSelection;
    }

    bool ShouldShowAluPanel()
    {
        var step = m_LessonFlow?.CurrentStep;
        if (step == null)
            return false;

        return step.requiredInteraction == InstructionStepInteractionType.AluExecution;
    }

    bool ShouldShowMemoryPanel()
    {
        if (ShouldShowAluPanel() || ShouldShowWriteBackPanel())
            return false;

        var step = m_LessonFlow?.CurrentStep;
        if (step == null)
            return false;

        return step.highlightedNode == DatapathNodeId.DataMemory;
    }

    bool ShouldShowWriteBackPanel()
    {
        var step = m_LessonFlow?.CurrentStep;
        if (step == null)
            return false;

        return step.requiredInteraction == InstructionStepInteractionType.WriteBackExecution;
    }

    bool ShouldShowPcUpdatePanel()
    {
        var step = m_LessonFlow?.CurrentStep;
        if (step == null)
            return false;

        return step.requiredInteraction == InstructionStepInteractionType.PcUpdateExecution;
    }

    void PopulateInstructionDropdown()
    {
        if (m_LessonFlow == null)
            return;

        m_AvailableInstructions.Clear();
        var loadedInstructions = Resources.LoadAll<InstructionDefinition>("InstructionDefinitions");
        if (loadedInstructions != null && loadedInstructions.Length > 0)
        {
            m_AvailableInstructions.AddRange(loadedInstructions);
            m_AvailableInstructions.Sort((left, right) =>
                string.Compare(left != null ? left.displayName : string.Empty,
                    right != null ? right.displayName : string.Empty,
                    System.StringComparison.OrdinalIgnoreCase));
        }

        if (m_AvailableInstructions.Count == 0 && m_LessonFlow.CurrentInstruction != null)
            m_AvailableInstructions.Add(m_LessonFlow.CurrentInstruction);

        if (m_InstructionDropdown == null)
            return;

        m_IsRefreshingInstructionDropdown = true;
        m_InstructionDropdown.ClearOptions();

        var optionLabels = new List<string>();
        foreach (var instruction in m_AvailableInstructions)
            optionLabels.Add(instruction != null ? instruction.displayName : "Instruction");

        if (optionLabels.Count > 0)
            m_InstructionDropdown.AddOptions(optionLabels);

        var currentIndex = 0;
        for (var index = 0; index < m_AvailableInstructions.Count; index++)
        {
            if (m_AvailableInstructions[index] == m_LessonFlow.CurrentInstruction)
            {
                currentIndex = index;
                break;
            }
        }

        if (m_InstructionDropdown.options.Count > 0)
            m_InstructionDropdown.SetValueWithoutNotify(currentIndex);

        m_IsRefreshingInstructionDropdown = false;
    }
}

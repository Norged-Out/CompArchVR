using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Root coordinator for the authored lesson guide panels.
/// It owns scene bindings and delegates each panel's presentation logic to a
/// dedicated controller class.
/// </summary>
[DisallowMultipleComponent]
public sealed class LessonGuideController : MonoBehaviour
{
    const string k_LogPrefix = "[LessonGuideController]";

    [SerializeField]
    CpuLessonFlow m_LessonFlow;

    [SerializeField]
    string m_StartButtonLabel = "Start Lesson";

    [SerializeField]
    string m_ContinueButtonLabel = "Continue";

    [SerializeField]
    string m_RestartButtonLabel = "Restart";

    [Header("Panel Components")]
    [SerializeField]
    IntroPanelController m_IntroPanel;

    [SerializeField]
    DecodePanelController m_DecodePanel;

    [SerializeField]
    ExecutePanelController m_ExecutePanel;

    [SerializeField]
    MemoryPanelController m_MemoryPanel;

    [SerializeField]
    WriteBackPanelController m_WriteBackPanel;

    [SerializeField]
    PcUpdatePanelController m_PcUpdatePanel;

    readonly List<InstructionDefinition> m_AvailableInstructions = new();
    bool m_IsRefreshingInstructionDropdown;
    bool m_IsRefreshingDecodeDropdowns;
    bool m_IsDecodeFunctStepActive;

    void Awake()
    {
        RefreshInstructionLibrary();
        HookButtons();
        HookDropdowns();
        RefreshView();
    }

    void OnEnable()
    {
        RefreshInstructionLibrary();
        HookDropdowns();

        var aluController = m_ExecutePanel != null ? m_ExecutePanel.PhaseController : null;
        if (aluController != null)
            aluController.ExecutionCompleted += HandleAluExecutionCompleted;

        var writeBackController = m_WriteBackPanel != null ? m_WriteBackPanel.PhaseController : null;
        if (writeBackController != null)
        {
            writeBackController.WriteBackApplied += HandleWriteBackApplied;
            writeBackController.ContinueRequested += HandleWriteBackContinueRequested;
        }

        var memoryController = m_MemoryPanel != null ? m_MemoryPanel.PhaseController : null;
        if (memoryController != null)
            memoryController.ContinueRequested += HandleMemoryContinueRequested;

        var pcUpdateController = m_PcUpdatePanel != null ? m_PcUpdatePanel.PhaseController : null;
        if (pcUpdateController != null)
            pcUpdateController.ContinueRequested += HandlePcUpdateContinueRequested;

        if (m_LessonFlow != null)
        {
            m_LessonFlow.StepChanged += HandleStepChanged;
            m_LessonFlow.FeedbackChanged += HandleFeedbackChanged;
        }

        RefreshView();
    }

    void OnDisable()
    {
        var aluController = m_ExecutePanel != null ? m_ExecutePanel.PhaseController : null;
        if (aluController != null)
            aluController.ExecutionCompleted -= HandleAluExecutionCompleted;

        var writeBackController = m_WriteBackPanel != null ? m_WriteBackPanel.PhaseController : null;
        if (writeBackController != null)
        {
            writeBackController.WriteBackApplied -= HandleWriteBackApplied;
            writeBackController.ContinueRequested -= HandleWriteBackContinueRequested;
        }

        var memoryController = m_MemoryPanel != null ? m_MemoryPanel.PhaseController : null;
        if (memoryController != null)
            memoryController.ContinueRequested -= HandleMemoryContinueRequested;

        var pcUpdateController = m_PcUpdatePanel != null ? m_PcUpdatePanel.PhaseController : null;
        if (pcUpdateController != null)
            pcUpdateController.ContinueRequested -= HandlePcUpdateContinueRequested;

        if (m_LessonFlow != null)
        {
            m_LessonFlow.StepChanged -= HandleStepChanged;
            m_LessonFlow.FeedbackChanged -= HandleFeedbackChanged;
        }
    }

    void RefreshInstructionLibrary()
    {
        if (m_LessonFlow == null)
            return;

        m_AvailableInstructions.Clear();
        var loadedInstructions = Resources.LoadAll<InstructionDefinition>("InstructionDefinitions");
        if (loadedInstructions != null && loadedInstructions.Length > 0)
        {
            m_AvailableInstructions.AddRange(loadedInstructions);
            m_AvailableInstructions.Sort((left, right) =>
                string.Compare(
                    left != null ? left.displayName : string.Empty,
                    right != null ? right.displayName : string.Empty,
                    StringComparison.OrdinalIgnoreCase));
        }

        if (m_AvailableInstructions.Count == 0 && m_LessonFlow.CurrentInstruction != null)
            m_AvailableInstructions.Add(m_LessonFlow.CurrentInstruction);

        m_IntroPanel?.PopulateInstructionDropdown(m_AvailableInstructions, m_LessonFlow.CurrentInstruction, ref m_IsRefreshingInstructionDropdown);
        m_DecodePanel?.PopulateDropdowns(m_AvailableInstructions, m_LessonFlow.CurrentInstruction, ref m_IsRefreshingDecodeDropdowns);
    }

    void HookButtons()
    {
        if (m_IntroPanel?.ActionButton != null)
        {
            m_IntroPanel.ActionButton.onClick.RemoveAllListeners();
            m_IntroPanel.ActionButton.onClick.AddListener(HandleIntroActionPressed);
        }

        if (m_DecodePanel?.ActionButton != null)
        {
            m_DecodePanel.ActionButton.onClick.RemoveAllListeners();
            m_DecodePanel.ActionButton.onClick.AddListener(HandleIDActionPressed);
        }
    }

    void HookDropdowns()
    {
        if (m_IntroPanel?.InstructionDropdown != null)
        {
            m_IntroPanel.InstructionDropdown.onValueChanged.RemoveListener(HandleInstructionChanged);
            m_IntroPanel.InstructionDropdown.onValueChanged.AddListener(HandleInstructionChanged);
        }

        if (m_DecodePanel?.OpcodeDropdown != null)
        {
            m_DecodePanel.OpcodeDropdown.onValueChanged.RemoveListener(HandleDecodeOpcodeChanged);
            m_DecodePanel.OpcodeDropdown.onValueChanged.AddListener(HandleDecodeOpcodeChanged);
        }

        if (m_DecodePanel?.FunctDropdown != null)
        {
            m_DecodePanel.FunctDropdown.onValueChanged.RemoveListener(HandleDecodeFunctChanged);
            m_DecodePanel.FunctDropdown.onValueChanged.AddListener(HandleDecodeFunctChanged);
        }

        if (m_DecodePanel?.HintDropdown != null)
        {
            m_DecodePanel.HintDropdown.onValueChanged.RemoveListener(HandleDecodeHintChanged);
            m_DecodePanel.HintDropdown.onValueChanged.AddListener(HandleDecodeHintChanged);
        }
    }

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

    void HandleIDActionPressed()
    {
        if (m_LessonFlow == null)
            return;

        Debug.Log($"{k_LogPrefix} ID button pressed | hasStarted={m_LessonFlow.HasStarted} step={m_LessonFlow.CurrentStep?.stepName} frame={Time.frameCount}", this);

        if (!m_LessonFlow.HasStarted)
            m_LessonFlow.StartLesson();
        else if (m_DecodePanel.IsOpcodeSelectionStep(m_LessonFlow, m_IsDecodeFunctStepActive))
            HandleDecodeOpcodeContinue();
        else if (m_DecodePanel.IsFunctSelectionStep(m_LessonFlow, m_IsDecodeFunctStepActive))
            HandleDecodeFunctContinue();
        else
            m_LessonFlow.Advance();
    }

    void HandleInstructionChanged(int selectedIndex)
    {
        if (m_IsRefreshingInstructionDropdown)
            return;

        if (selectedIndex < 0 || selectedIndex >= m_AvailableInstructions.Count)
            return;

        m_LessonFlow?.SetCurrentInstruction(m_AvailableInstructions[selectedIndex]);
        m_DecodePanel?.PopulateDropdowns(m_AvailableInstructions, m_LessonFlow != null ? m_LessonFlow.CurrentInstruction : null, ref m_IsRefreshingDecodeDropdowns);
        RefreshView();
    }

    void HandleDecodeOpcodeChanged(int _)
    {
        if (m_IsRefreshingDecodeDropdowns)
            return;

        RefreshView();
    }

    void HandleDecodeFunctChanged(int _)
    {
        if (m_IsRefreshingDecodeDropdowns)
            return;

        RefreshView();
    }

    void HandleDecodeHintChanged(int _)
    {
        if (m_IsRefreshingDecodeDropdowns)
            return;

        m_DecodePanel?.RefreshHintText(m_AvailableInstructions);
    }

    void HandleStepChanged(CpuLessonFlow _)
    {
        Debug.Log($"{k_LogPrefix} StepChanged | step={m_LessonFlow?.CurrentStep?.stepName} frame={Time.frameCount}", this);
        RefreshView();
    }

    void HandleAluExecutionCompleted(int resultValue)
    {
        m_LessonFlow?.CompleteAluExecution(resultValue);
    }

    void HandleWriteBackApplied(string destinationRegister, int resultValue)
    {
        m_LessonFlow?.CompleteWriteBackExecution(destinationRegister, resultValue);
    }

    void HandleWriteBackContinueRequested()
    {
        m_LessonFlow?.Advance();
    }

    void HandleMemoryContinueRequested()
    {
        m_LessonFlow?.Advance();
    }

    void HandlePcUpdateContinueRequested()
    {
        m_LessonFlow?.ResetLesson();
    }

    void HandleFeedbackChanged(string message, bool isFailure)
    {
        if (ShouldShowIDPanel())
        {
            m_DecodePanel?.SetFeedback(message, isFailure, m_AvailableInstructions);
            return;
        }

        if (ShouldShowMemoryPanel() || ShouldShowAluPanel() || ShouldShowPcUpdatePanel())
            return;

        m_IntroPanel?.SetFeedback(message, isFailure);
    }

    void HandleDecodeOpcodeContinue()
    {
        if (m_LessonFlow == null || m_LessonFlow.CurrentInstruction == null || m_DecodePanel == null)
            return;

        var selectedOpcode = m_DecodePanel.GetSelectedOpcode();
        if (string.IsNullOrWhiteSpace(selectedOpcode))
        {
            HandleFeedbackChanged("Select an opcode first.", true);
            return;
        }

        var expectedOpcode = m_LessonFlow.CurrentInstruction.opcodeBits != null
            ? m_LessonFlow.CurrentInstruction.opcodeBits.Trim()
            : string.Empty;

        if (!string.Equals(selectedOpcode, expectedOpcode, StringComparison.Ordinal))
        {
            HandleFeedbackChanged("That opcode does not match the selected instruction.", true);
            return;
        }

        if (DecodePanelController.InstructionUsesDecodeFunct(m_LessonFlow.CurrentInstruction))
        {
            m_IsDecodeFunctStepActive = true;
            m_DecodePanel.ResetFunctDropdown(ref m_IsRefreshingDecodeDropdowns);
            HandleFeedbackChanged("Opcode confirmed. Now identify the funct field.", false);
            RefreshView();
            return;
        }

        HandleFeedbackChanged("Opcode confirmed. Continue into operand setup.", false);
        m_LessonFlow.Advance();
    }

    void HandleDecodeFunctContinue()
    {
        if (m_LessonFlow == null || m_LessonFlow.CurrentInstruction == null || m_DecodePanel == null)
            return;

        var selectedFunct = m_DecodePanel.GetSelectedFunct();
        if (string.IsNullOrWhiteSpace(selectedFunct))
        {
            HandleFeedbackChanged("Select a funct value first.", true);
            return;
        }

        var expectedFunct = m_LessonFlow.CurrentInstruction.functBits != null
            ? m_LessonFlow.CurrentInstruction.functBits.Trim()
            : string.Empty;

        if (!string.Equals(selectedFunct, expectedFunct, StringComparison.Ordinal))
        {
            HandleFeedbackChanged("That funct value does not match the selected instruction.", true);
            return;
        }

        m_IsDecodeFunctStepActive = false;
        HandleFeedbackChanged("Funct confirmed. Continue into operand setup.", false);
        m_LessonFlow.Advance();
    }

    void RefreshView()
    {
        if (m_LessonFlow == null || m_IntroPanel == null)
            return;

        var showAluPanel = ShouldShowAluPanel();
        var showWriteBackPanel = ShouldShowWriteBackPanel();
        var showMemoryPanel = ShouldShowMemoryPanel();
        var showPcUpdatePanel = ShouldShowPcUpdatePanel();
        var showIDPanel = ShouldShowIDPanel();

        Debug.Log(
            $"{k_LogPrefix} RefreshView | step={m_LessonFlow.CurrentStep?.stepName} decode={showIDPanel} alu={showAluPanel} mem={showMemoryPanel} wb={showWriteBackPanel} pc={showPcUpdatePanel} frame={Time.frameCount}",
            this);

        m_ExecutePanel?.ApplyState(showAluPanel, m_LessonFlow.CurrentInstruction);
        m_MemoryPanel?.ApplyState(showMemoryPanel, m_LessonFlow.CurrentInstruction);
        m_WriteBackPanel?.ApplyState(showWriteBackPanel, m_LessonFlow.CurrentInstruction, m_LessonFlow.RegisterBank);
        m_PcUpdatePanel?.ApplyState(showPcUpdatePanel, m_LessonFlow.CurrentInstruction);

        m_IntroPanel.SetVisible(!showIDPanel && !showAluPanel && !showMemoryPanel && !showWriteBackPanel && !showPcUpdatePanel);
        m_DecodePanel?.SetVisible(showIDPanel);

        if (!m_LessonFlow.HasStarted)
        {
            m_ExecutePanel?.Reset();
            m_MemoryPanel?.Reset();
            m_WriteBackPanel?.Reset();
            m_PcUpdatePanel?.Reset();
            m_DecodePanel?.ResetDropdowns(ref m_IsRefreshingDecodeDropdowns, ref m_IsDecodeFunctStepActive);
            m_IntroPanel.ShowBeforeStart(m_LessonFlow.CurrentInstruction, m_StartButtonLabel);
            return;
        }

        var step = m_LessonFlow.CurrentStep;
        if (step == null)
            return;

        m_IntroPanel.SetInstructionDropdownInteractable(false);

        if (showAluPanel || showWriteBackPanel || showPcUpdatePanel || showMemoryPanel)
        {
            m_IntroPanel.HideAction();
            m_DecodePanel?.HideAction();
            return;
        }

        if (!showIDPanel)
        {
            m_IntroPanel.ShowStep(m_LessonFlow, step, m_ContinueButtonLabel, m_RestartButtonLabel);
            return;
        }

        m_DecodePanel?.Refresh(
            m_LessonFlow,
            step,
            m_AvailableInstructions,
            m_IsDecodeFunctStepActive,
            m_ContinueButtonLabel,
            m_RestartButtonLabel);
    }

    bool ShouldShowIDPanel()
    {
        if (ShouldShowAluPanel())
            return false;

        var step = m_LessonFlow != null ? m_LessonFlow.CurrentStep : null;
        if (step == null)
            return false;

        return step.highlightedNode == DatapathNodeId.InstructionMemory ||
               step.requiredInteraction == InstructionStepInteractionType.RegisterSelection;
    }

    bool ShouldShowAluPanel()
    {
        var step = m_LessonFlow != null ? m_LessonFlow.CurrentStep : null;
        return step != null && step.requiredInteraction == InstructionStepInteractionType.AluExecution;
    }

    bool ShouldShowMemoryPanel()
    {
        if (ShouldShowAluPanel() || ShouldShowWriteBackPanel())
            return false;

        var step = m_LessonFlow != null ? m_LessonFlow.CurrentStep : null;
        return step != null && step.highlightedNode == DatapathNodeId.DataMemory;
    }

    bool ShouldShowWriteBackPanel()
    {
        var step = m_LessonFlow != null ? m_LessonFlow.CurrentStep : null;
        return step != null && step.requiredInteraction == InstructionStepInteractionType.WriteBackExecution;
    }

    bool ShouldShowPcUpdatePanel()
    {
        var step = m_LessonFlow != null ? m_LessonFlow.CurrentStep : null;
        return step != null && step.requiredInteraction == InstructionStepInteractionType.PcUpdateExecution;
    }
}

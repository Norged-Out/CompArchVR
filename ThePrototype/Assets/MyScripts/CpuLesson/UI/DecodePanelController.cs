using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Presentation owner for the instruction-decode lesson panel.
/// </summary>
[DisallowMultipleComponent]
public sealed class DecodePanelController : LessonPanelBase
{
    enum DecodePromptKind
    {
        None,
        Opcode,
        Funct,
        Registers,
    }

    [SerializeField]
    TMP_Text m_OpcodeLessonText;

    [SerializeField]
    TMP_Text m_RegisterLessonText;

    [SerializeField]
    TMP_Text m_FunctLessonText;

    [SerializeField]
    GameObject m_OpcodeGroupRoot;

    [SerializeField]
    GameObject m_FunctGroupRoot;

    [SerializeField]
    TMP_Text m_RegisterBodyText;

    [SerializeField]
    TMP_Text m_OpcodeSelectionText;

    [SerializeField]
    TMP_Text m_RegisterSelectionText;

    [SerializeField]
    TMP_Text m_FunctSelectionText;

    [SerializeField]
    TMP_Text m_Feedback;

    [SerializeField]
    Button m_ActionButton;

    [SerializeField]
    TMP_Text m_ActionLabel;

    [SerializeField]
    TMP_Dropdown m_OpcodeDropdown;

    [SerializeField]
    TMP_Dropdown m_FunctDropdown;

    [SerializeField]
    TMP_Dropdown m_HintDropdown;

    [SerializeField]
    TMP_Text m_HintText;

    readonly DecodeTextBuilder m_TextBuilder = new();
    DecodeDropdownView m_Dropdowns;

    /// <summary>
    /// Shows or hides the authored decode panel root.
    /// </summary>
    public void SetVisible(bool isVisible)
    {
        SetPanelVisible(isVisible);
    }

    /// <summary>
    /// Hides the authored decode action button when another phase owns progression.
    /// </summary>
    public void HideAction()
    {
        SetButtonState(m_ActionButton, m_ActionLabel, string.Empty, false);
        RefreshPanelLayout(m_ActionButton);
    }

    /// <summary>
    /// Rebuilds all decode dropdowns from the authored instruction catalog.
    /// </summary>
    public void PopulateDropdowns(IReadOnlyList<InstructionDefinition> availableInstructions, InstructionDefinition currentInstruction, ref bool isRefreshing)
    {
        EnsureDropdowns();
        m_Dropdowns.Populate(availableInstructions, currentInstruction, ref isRefreshing);
    }

    /// <summary>
    /// Resets all decode dropdowns to their authored placeholder values.
    /// </summary>
    public void ResetDropdowns(ref bool isRefreshing)
    {
        EnsureDropdowns();
        m_Dropdowns.Reset(ref isRefreshing);
    }

    /// <summary>
    /// Resets the funct dropdown when opcode validation unlocks the R-type funct step.
    /// </summary>
    public void ResetFunctDropdown(ref bool isRefreshing)
    {
        EnsureDropdowns();
        m_Dropdowns.ResetFunct(ref isRefreshing);
    }

    /// <summary>
    /// Refreshes the authored decode panel for the currently active decode sub-step.
    /// </summary>
    public void Refresh(
        CpuLessonFlow lessonFlow,
        InstructionFlowStep step,
        IReadOnlyList<InstructionDefinition> availableInstructions,
        DecodeSelectionMode selectionMode,
        string continueLabel,
        string restartLabel)
    {
        if (lessonFlow == null || step == null)
            return;

        SetVisible(true);

        var promptKind = GetPromptKind(selectionMode);
        ApplyPromptVisibility(promptKind);
        RefreshSelectionTexts(promptKind, lessonFlow, step);
        EnsureDropdowns();
        m_Dropdowns.RefreshVisibleDropdowns(
            promptKind == DecodePromptKind.Opcode,
            promptKind == DecodePromptKind.Funct);
        RefreshHintText(availableInstructions);

        SetButtonState(
            m_ActionButton,
            m_ActionLabel,
            step.requiredInteraction == InstructionStepInteractionType.Completion ? restartLabel : continueLabel,
            ShouldShowContinue(promptKind, step, lessonFlow));

        RefreshPanelLayout(m_ActionButton);
    }

    /// <summary>
    /// Applies decode feedback styling and keeps the hint panel layout in sync.
    /// </summary>
    public void SetFeedback(string message, bool isFailure, IReadOnlyList<InstructionDefinition> availableInstructions)
    {
        SetFeedbackField(m_Feedback, message, isFailure);
        RefreshHintText(availableInstructions);
        RefreshPanelLayout(m_ActionButton);
    }

    /// <summary>
    /// Refreshes the currently selected hint text.
    /// </summary>
    public void RefreshHintText(IReadOnlyList<InstructionDefinition> availableInstructions)
    {
        EnsureDropdowns();
        m_Dropdowns.RefreshHintText(availableInstructions);
        RefreshPanelLayout(m_ActionButton);
    }

    /// <summary>
    /// Returns the learner's currently selected opcode bits.
    /// </summary>
    public string GetSelectedOpcode()
    {
        EnsureDropdowns();
        return m_Dropdowns.GetSelectedOpcode();
    }

    /// <summary>
    /// Returns the learner's currently selected funct bits.
    /// </summary>
    public string GetSelectedFunct()
    {
        EnsureDropdowns();
        return m_Dropdowns.GetSelectedFunct();
    }

    /// <summary>
    /// Collapses the authored decode selection mode into the panel's three visible states.
    /// </summary>
    static DecodePromptKind GetPromptKind(DecodeSelectionMode selectionMode)
    {
        return selectionMode switch
        {
            DecodeSelectionMode.Opcode => DecodePromptKind.Opcode,
            DecodeSelectionMode.Funct => DecodePromptKind.Funct,
            DecodeSelectionMode.Registers => DecodePromptKind.Registers,
            _ => DecodePromptKind.None,
        };
    }

    /// <summary>
    /// Shows only the authored lesson/body/selection fields that match the active decode state.
    /// </summary>
    void ApplyPromptVisibility(DecodePromptKind promptKind)
    {
        var isOpcodeStep = promptKind == DecodePromptKind.Opcode;
        var isFunctStep = promptKind == DecodePromptKind.Funct;
        var isRegisterStep = promptKind == DecodePromptKind.Registers;

        SetTextFieldActive(m_OpcodeLessonText, isOpcodeStep);
        SetTextFieldActive(m_FunctLessonText, isFunctStep);
        SetTextFieldActive(m_RegisterLessonText, isRegisterStep);
        SetObjectActive(m_OpcodeGroupRoot, isOpcodeStep);
        SetObjectActive(m_FunctGroupRoot, isFunctStep);
        SetTextFieldActive(m_RegisterBodyText, isRegisterStep);
        SetTextFieldActive(m_RegisterSelectionText, isRegisterStep);
    }

    /// <summary>
    /// Rebuilds only the active runtime selection text for the decode step currently in view.
    /// </summary>
    void RefreshSelectionTexts(DecodePromptKind promptKind, CpuLessonFlow lessonFlow, InstructionFlowStep step)
    {
        var assemblySelectionText = m_TextBuilder.BuildAssemblySelectionText(lessonFlow.CurrentInstruction);

        SetTextField(
            m_OpcodeSelectionText,
            promptKind == DecodePromptKind.Opcode ? assemblySelectionText : string.Empty);
        SetTextField(
            m_FunctSelectionText,
            promptKind == DecodePromptKind.Funct ? assemblySelectionText : string.Empty);
        SetTextField(
            m_RegisterSelectionText,
            promptKind == DecodePromptKind.Registers ? m_TextBuilder.BuildRegisterSelectionText(lessonFlow, step) : string.Empty);
    }

    /// <summary>
    /// Lazily creates the decode dropdown helper once the authored UI fields exist.
    /// </summary>
    void EnsureDropdowns()
    {
        m_Dropdowns ??= new DecodeDropdownView(
            m_OpcodeDropdown,
            m_FunctDropdown,
            m_HintDropdown,
            m_HintText);
    }

    /// <summary>
    /// Returns whether the current decode state should expose the continue button.
    /// </summary>
    static bool ShouldShowContinue(DecodePromptKind promptKind, InstructionFlowStep step, CpuLessonFlow lessonFlow)
    {
        var isSelectionStep = promptKind == DecodePromptKind.Opcode || promptKind == DecodePromptKind.Funct;
        return isSelectionStep ||
               step.requiredInteraction == InstructionStepInteractionType.ContinueButton ||
               step.requiredInteraction == InstructionStepInteractionType.Completion ||
               (step.requiredInteraction == InstructionStepInteractionType.RegisterSelection &&
                lessonFlow.RegisterSelectionReadyToContinue);
    }
}

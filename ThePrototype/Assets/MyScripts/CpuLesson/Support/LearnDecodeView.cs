using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Owns the Learning-mode decode presentation pieces so the decode panel can
/// switch between learning and practice without keeping both UI layouts tangled
/// in one class.
/// </summary>
sealed class LearnDecodeView
{
    enum PromptKind
    {
        None,
        Opcode,
        Funct,
        Registers,
    }

    readonly TMP_Text m_OpcodeLessonText;
    readonly TMP_Text m_RegisterLessonText;
    readonly TMP_Text m_FunctLessonText;
    readonly GameObject m_OpcodeGroupRoot;
    readonly GameObject m_FunctGroupRoot;
    readonly TMP_Text m_RegisterBodyText;
    readonly TMP_Text m_OpcodeSelectionText;
    readonly TMP_Text m_RegisterSelectionText;
    readonly TMP_Text m_FunctSelectionText;
    readonly TMP_Dropdown m_OpcodeDropdown;
    readonly TMP_Dropdown m_FunctDropdown;
    readonly TMP_Dropdown m_HintDropdown;
    readonly TMP_Text m_HintText;

    readonly DecodeTextBuilder m_TextBuilder = new();
    DecodeDropdownView m_Dropdowns;

    public LearnDecodeView(
        TMP_Text opcodeLessonText,
        TMP_Text registerLessonText,
        TMP_Text functLessonText,
        GameObject opcodeGroupRoot,
        GameObject functGroupRoot,
        TMP_Text registerBodyText,
        TMP_Text opcodeSelectionText,
        TMP_Text registerSelectionText,
        TMP_Text functSelectionText,
        TMP_Dropdown opcodeDropdown,
        TMP_Dropdown functDropdown,
        TMP_Dropdown hintDropdown,
        TMP_Text hintText)
    {
        m_OpcodeLessonText = opcodeLessonText;
        m_RegisterLessonText = registerLessonText;
        m_FunctLessonText = functLessonText;
        m_OpcodeGroupRoot = opcodeGroupRoot;
        m_FunctGroupRoot = functGroupRoot;
        m_RegisterBodyText = registerBodyText;
        m_OpcodeSelectionText = opcodeSelectionText;
        m_RegisterSelectionText = registerSelectionText;
        m_FunctSelectionText = functSelectionText;
        m_OpcodeDropdown = opcodeDropdown;
        m_FunctDropdown = functDropdown;
        m_HintDropdown = hintDropdown;
        m_HintText = hintText;
    }

    public void PopulateDropdowns(IReadOnlyList<InstructionDefinition> availableInstructions, InstructionDefinition currentInstruction, ref bool isRefreshing)
    {
        EnsureDropdowns();
        m_Dropdowns.Populate(availableInstructions, currentInstruction, ref isRefreshing);
    }

    public void ResetDropdowns(ref bool isRefreshing)
    {
        EnsureDropdowns();
        m_Dropdowns.Reset(ref isRefreshing);
    }

    public void ResetFunctDropdown(ref bool isRefreshing)
    {
        EnsureDropdowns();
        m_Dropdowns.ResetFunct(ref isRefreshing);
    }

    public void Refresh(
        CpuLessonFlow lessonFlow,
        InstructionFlowStep step,
        IReadOnlyList<InstructionDefinition> availableInstructions,
        DecodeSelectionMode selectionMode)
    {
        var promptKind = GetPromptKind(selectionMode);
        ApplyPromptVisibility(promptKind);
        RefreshSelectionTexts(promptKind, lessonFlow, step);
        EnsureDropdowns();
        m_Dropdowns.RefreshVisibleDropdowns(
            promptKind == PromptKind.Opcode,
            promptKind == PromptKind.Funct);
        RefreshHintText(availableInstructions);
    }

    /// <summary>
    /// Collapses every Learning-only decode widget when Practice mode takes over
    /// the shared decode panel root.
    /// </summary>
    public void HideAll()
    {
        SetTextFieldActive(m_OpcodeLessonText, false);
        SetTextFieldActive(m_FunctLessonText, false);
        SetTextFieldActive(m_RegisterLessonText, false);
        SetObjectActive(m_OpcodeGroupRoot, false);
        SetObjectActive(m_FunctGroupRoot, false);
        SetTextFieldActive(m_RegisterBodyText, false);
        SetTextFieldActive(m_OpcodeSelectionText, false);
        SetTextFieldActive(m_RegisterSelectionText, false);
        SetTextFieldActive(m_FunctSelectionText, false);
        SetObjectActive(m_HintDropdown != null ? m_HintDropdown.gameObject : null, false);
        SetTextFieldActive(m_HintText, false);
    }

    public void RefreshHintText(IReadOnlyList<InstructionDefinition> availableInstructions)
    {
        EnsureDropdowns();
        m_Dropdowns.RefreshHintText(availableInstructions);
    }

    public string GetSelectedOpcode()
    {
        EnsureDropdowns();
        return m_Dropdowns.GetSelectedOpcode();
    }

    public string GetSelectedFunct()
    {
        EnsureDropdowns();
        return m_Dropdowns.GetSelectedFunct();
    }

    static PromptKind GetPromptKind(DecodeSelectionMode selectionMode)
    {
        return selectionMode switch
        {
            DecodeSelectionMode.Opcode => PromptKind.Opcode,
            DecodeSelectionMode.Funct => PromptKind.Funct,
            DecodeSelectionMode.Registers => PromptKind.Registers,
            _ => PromptKind.None,
        };
    }

    void ApplyPromptVisibility(PromptKind promptKind)
    {
        var isOpcodeStep = promptKind == PromptKind.Opcode;
        var isFunctStep = promptKind == PromptKind.Funct;
        var isRegisterStep = promptKind == PromptKind.Registers;

        SetTextFieldActive(m_OpcodeLessonText, isOpcodeStep);
        SetTextFieldActive(m_FunctLessonText, isFunctStep);
        SetTextFieldActive(m_RegisterLessonText, isRegisterStep);
        SetObjectActive(m_OpcodeGroupRoot, isOpcodeStep);
        SetObjectActive(m_FunctGroupRoot, isFunctStep);
        SetTextFieldActive(m_RegisterBodyText, isRegisterStep);
        SetTextFieldActive(m_RegisterSelectionText, isRegisterStep);
    }

    void RefreshSelectionTexts(PromptKind promptKind, CpuLessonFlow lessonFlow, InstructionFlowStep step)
    {
        var assemblySelectionText = m_TextBuilder.BuildAssemblySelectionText(lessonFlow.CurrentInstruction);

        SetTextField(
            m_OpcodeSelectionText,
            promptKind == PromptKind.Opcode ? assemblySelectionText : string.Empty);
        SetTextField(
            m_FunctSelectionText,
            promptKind == PromptKind.Funct ? assemblySelectionText : string.Empty);
        SetTextField(
            m_RegisterSelectionText,
            promptKind == PromptKind.Registers ? m_TextBuilder.BuildRegisterSelectionText(lessonFlow, step) : string.Empty);
    }

    void EnsureDropdowns()
    {
        m_Dropdowns ??= new DecodeDropdownView(
            m_OpcodeDropdown,
            m_FunctDropdown,
            m_HintDropdown,
            m_HintText);
    }

    static void SetTextField(TMP_Text target, string text)
    {
        if (target == null)
            return;

        target.text = text;
        target.gameObject.SetActive(!string.IsNullOrWhiteSpace(text));
    }

    static void SetTextFieldActive(TMP_Text target, bool isActive)
    {
        if (target == null)
            return;

        target.gameObject.SetActive(isActive);
    }

    static void SetObjectActive(GameObject target, bool isActive)
    {
        if (target == null)
            return;

        target.SetActive(isActive);
    }
}

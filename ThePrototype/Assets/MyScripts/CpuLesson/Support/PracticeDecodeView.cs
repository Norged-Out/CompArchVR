using System.Collections.Generic;
using TMPro;
using UnityEngine;

sealed class PracticeDecodeView
{
    static readonly List<string> s_OpcodeOptions = new()
    {
        "Choose Opcode",
        "000000",
        "000100",
        "000101",
        "001000",
        "100011",
        "101011",
    };

    static readonly List<string> s_FunctOptions = new()
    {
        "Choose Funct",
        "100000",
        "100010",
        "100100",
        "100101",
        "101010",
    };

    readonly GameObject m_PracticeRoot;
    readonly TMP_Text m_BinaryText;
    readonly TMP_Text m_StatusText;
    readonly TMP_Text m_HintText;
    readonly PracticeDecodeDropdownField m_OpcodeField;
    readonly PracticeDecodeDropdownField m_RsField;
    readonly PracticeDecodeDropdownField m_RtField;
    readonly PracticeDecodeOptionalField m_RdField;
    readonly PracticeDecodeOptionalField m_ImmediateField;
    readonly PracticeDecodeOptionalField m_FunctField;

    readonly List<string> m_RegisterOptions = new();
    readonly List<string> m_ImmediateOptions = new();

    public PracticeDecodeView(
        GameObject practiceRoot,
        TMP_Text binaryText,
        TMP_Text statusText,
        GameObject opcodeGroupRoot,
        TMP_Dropdown opcodeDropdown,
        GameObject rsGroupRoot,
        TMP_Dropdown rsDropdown,
        GameObject rtGroupRoot,
        TMP_Dropdown rtDropdown,
        GameObject rdGroupRoot,
        UnityEngine.UI.Toggle rdToggle,
        TMP_Dropdown rdDropdown,
        GameObject immediateGroupRoot,
        UnityEngine.UI.Toggle immediateToggle,
        TMP_Dropdown immediateDropdown,
        GameObject functGroupRoot,
        UnityEngine.UI.Toggle functToggle,
        TMP_Dropdown functDropdown,
        TMP_Text hintText)
    {
        m_PracticeRoot = practiceRoot;
        m_BinaryText = binaryText;
        m_StatusText = statusText;
        m_HintText = hintText;
        m_OpcodeField = new PracticeDecodeDropdownField(opcodeGroupRoot, opcodeDropdown);
        m_RsField = new PracticeDecodeDropdownField(rsGroupRoot, rsDropdown);
        m_RtField = new PracticeDecodeDropdownField(rtGroupRoot, rtDropdown);
        m_RdField = new PracticeDecodeOptionalField(rdGroupRoot, rdToggle, rdDropdown);
        m_ImmediateField = new PracticeDecodeOptionalField(immediateGroupRoot, immediateToggle, immediateDropdown);
        m_FunctField = new PracticeDecodeOptionalField(functGroupRoot, functToggle, functDropdown);
    }

    public void Refresh(PracticeInstructionDefinition instruction, bool opcodeConfirmed, bool isFailed, ref bool isRefreshing)
    {
        SetVisible(!isFailed);
        if (isFailed)
            return;

        PopulateOptions(instruction, ref isRefreshing);
        m_OpcodeField.SetVisible(!opcodeConfirmed);
        m_RsField.SetVisible(opcodeConfirmed);
        m_RtField.SetVisible(opcodeConfirmed);
        m_RdField.RefreshVisibility(opcodeConfirmed);
        m_ImmediateField.RefreshVisibility(opcodeConfirmed);
        m_FunctField.RefreshVisibility(opcodeConfirmed);

        SetTextField(
            m_BinaryText,
            instruction != null
                ? $"Instruction Bits: {instruction.GetNormalizedBinaryInstruction()}"
                : string.Empty);
    }

    public void Reset(ref bool isRefreshing)
    {
        m_OpcodeField.Reset();
        m_RsField.Reset();
        m_RtField.Reset();
        m_RdField.Reset();
        m_ImmediateField.Reset();
        m_FunctField.Reset();
        isRefreshing = false;
        SetHintText(string.Empty);
        SetStatusText(string.Empty);
    }

    public void SetVisible(bool isVisible)
    {
        if (m_PracticeRoot != null)
            m_PracticeRoot.SetActive(isVisible);
    }

    public void SetStatusText(string statusText)
    {
        SetTextField(m_StatusText, statusText);
    }

    public void SetHintText(string hintText)
    {
        SetTextField(m_HintText, hintText);
    }

    public PracticeDecodeInputState CaptureInputState()
    {
        return new PracticeDecodeInputState(
            m_OpcodeField.GetSelectedValue(),
            m_RsField.GetSelectedValue(),
            m_RtField.GetSelectedValue(),
            m_RdField.IsEnabled,
            m_RdField.SelectedValue,
            m_ImmediateField.IsEnabled,
            m_ImmediateField.SelectedValue,
            m_FunctField.IsEnabled,
            m_FunctField.SelectedValue);
    }

    void PopulateOptions(PracticeInstructionDefinition instruction, ref bool isRefreshing)
    {
        m_OpcodeField.Populate(s_OpcodeOptions, ref isRefreshing);
        EnsureRegisterOptions();
        m_RsField.Populate(m_RegisterOptions, ref isRefreshing);
        m_RtField.Populate(m_RegisterOptions, ref isRefreshing);
        m_RdField.Populate(m_RegisterOptions, ref isRefreshing);
        PopulateImmediateOptions(instruction, ref isRefreshing);
        m_FunctField.Populate(s_FunctOptions, ref isRefreshing);
    }

    void EnsureRegisterOptions()
    {
        if (m_RegisterOptions.Count > 0)
            return;

        m_RegisterOptions.Add("Choose Register");
        for (var registerIndex = 0; registerIndex < 32; registerIndex++)
            m_RegisterOptions.Add(System.Convert.ToString(registerIndex, 2).PadLeft(5, '0'));
    }

    void PopulateImmediateOptions(PracticeInstructionDefinition instruction, ref bool isRefreshing)
    {
        m_ImmediateOptions.Clear();
        m_ImmediateOptions.Add("Choose Immediate");

        var expectedImmediateBits = instruction != null ? instruction.expectedImmediateBits : string.Empty;
        if (!string.IsNullOrWhiteSpace(expectedImmediateBits))
            m_ImmediateOptions.Add(expectedImmediateBits.Trim());

        m_ImmediateField.Populate(m_ImmediateOptions, ref isRefreshing);
    }

    static void SetTextField(TMP_Text target, string text)
    {
        if (target == null)
            return;

        target.text = text;
        target.gameObject.SetActive(!string.IsNullOrWhiteSpace(text));
    }
}

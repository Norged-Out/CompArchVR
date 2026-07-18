using TMPro;
using UnityEngine;

/// <summary>
/// Scene-side Practice decode presentation wrapper. It keeps the authored
/// input-field groups synchronized with the current staged decode state.
/// </summary>
sealed class PracticeDecodeView
{
    readonly GameObject m_PracticeRoot;
    readonly TMP_Text m_BinaryText;
    readonly TMP_Text m_StatusText;
    readonly TMP_Text m_HintText;
    readonly PracticeDecodeInputField m_OpcodeField;
    readonly PracticeDecodeInputField m_RsField;
    readonly PracticeDecodeInputField m_RtField;
    readonly PracticeDecodeOptionalInputField m_RdField;
    readonly PracticeDecodeOptionalInputField m_ImmediateField;
    readonly PracticeDecodeOptionalInputField m_FunctField;

    public PracticeDecodeView(
        GameObject practiceRoot,
        TMP_Text binaryText,
        TMP_Text statusText,
        GameObject opcodeGroupRoot,
        TMP_InputField opcodeInputField,
        GameObject rsGroupRoot,
        TMP_InputField rsInputField,
        GameObject rtGroupRoot,
        TMP_InputField rtInputField,
        GameObject rdGroupRoot,
        UnityEngine.UI.Toggle rdToggle,
        TMP_InputField rdInputField,
        GameObject immediateGroupRoot,
        UnityEngine.UI.Toggle immediateToggle,
        TMP_InputField immediateInputField,
        GameObject functGroupRoot,
        UnityEngine.UI.Toggle functToggle,
        TMP_InputField functInputField,
        TMP_Text hintText)
    {
        m_PracticeRoot = practiceRoot;
        m_BinaryText = binaryText;
        m_StatusText = statusText;
        m_HintText = hintText;
        m_OpcodeField = new PracticeDecodeInputField(opcodeGroupRoot, opcodeInputField, 6);
        m_RsField = new PracticeDecodeInputField(rsGroupRoot, rsInputField, 5);
        m_RtField = new PracticeDecodeInputField(rtGroupRoot, rtInputField, 5);
        m_RdField = new PracticeDecodeOptionalInputField(rdGroupRoot, rdToggle, rdInputField, 5);
        m_ImmediateField = new PracticeDecodeOptionalInputField(immediateGroupRoot, immediateToggle, immediateInputField, 16);
        m_FunctField = new PracticeDecodeOptionalInputField(functGroupRoot, functToggle, functInputField, 6);
    }

    public void Refresh(PracticeInstructionDefinition instruction, bool opcodeConfirmed, bool isFailed)
    {
        SetVisible(!isFailed);
        if (isFailed)
            return;

        ConfigureFields();
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

    public void Reset()
    {
        m_OpcodeField.Reset();
        m_RsField.Reset();
        m_RtField.Reset();
        m_RdField.Reset();
        m_ImmediateField.Reset();
        m_FunctField.Reset();
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
            m_OpcodeField.GetSubmittedBits(),
            m_RsField.GetSubmittedBits(),
            m_RtField.GetSubmittedBits(),
            m_RdField.IsEnabled,
            m_RdField.SubmittedBits,
            m_ImmediateField.IsEnabled,
            m_ImmediateField.SubmittedBits,
            m_FunctField.IsEnabled,
            m_FunctField.SubmittedBits);
    }

    void ConfigureFields()
    {
        m_OpcodeField.Configure();
        m_RsField.Configure();
        m_RtField.Configure();
        m_RdField.Configure();
        m_ImmediateField.Configure();
        m_FunctField.Configure();
    }

    static void SetTextField(TMP_Text target, string text)
    {
        if (target == null)
            return;

        target.text = text;
        target.gameObject.SetActive(!string.IsNullOrWhiteSpace(text));
    }
}

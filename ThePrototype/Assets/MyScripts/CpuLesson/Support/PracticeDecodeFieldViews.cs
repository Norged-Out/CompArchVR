using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Minimal wrapper around a required Practice decode input field. It owns only
/// visibility, text reset, and the enforced bit-length limit for that field.
/// </summary>
sealed class PracticeDecodeInputField
{
    readonly GameObject m_GroupRoot;
    readonly TMP_InputField m_InputField;
    readonly int m_BitCount;

    public PracticeDecodeInputField(GameObject groupRoot, TMP_InputField inputField, int bitCount)
    {
        m_GroupRoot = groupRoot;
        m_InputField = inputField;
        m_BitCount = bitCount;
    }

    public void SetVisible(bool isVisible)
    {
        if (m_GroupRoot != null)
            m_GroupRoot.SetActive(isVisible);
        else if (m_InputField != null)
            m_InputField.gameObject.SetActive(isVisible);

        if (m_InputField != null)
            m_InputField.interactable = isVisible;
    }

    public void Configure()
    {
        if (m_InputField == null)
            return;

        if (m_InputField.characterLimit != m_BitCount)
            m_InputField.characterLimit = m_BitCount;
    }

    public void Reset()
    {
        if (m_InputField == null)
            return;

        m_InputField.SetTextWithoutNotify(string.Empty);
    }

    public string GetSubmittedBits()
    {
        if (m_InputField == null)
            return string.Empty;

        return PracticeDecodeBitText.Normalize(m_InputField.text);
    }
}

/// <summary>
/// Variant of the Practice decode field wrapper for inputs that are only valid
/// when their matching toggle is enabled, such as rd, immediate, and funct.
/// </summary>
sealed class PracticeDecodeOptionalInputField
{
    readonly GameObject m_GroupRoot;
    readonly Toggle m_Toggle;
    readonly TMP_InputField m_InputField;
    readonly int m_BitCount;

    public PracticeDecodeOptionalInputField(GameObject groupRoot, Toggle toggle, TMP_InputField inputField, int bitCount)
    {
        m_GroupRoot = groupRoot;
        m_Toggle = toggle;
        m_InputField = inputField;
        m_BitCount = bitCount;
    }

    public bool IsEnabled => m_Toggle != null && m_Toggle.isOn;
    public string SubmittedBits => GetSubmittedBits();

    public void RefreshVisibility(bool isVisible)
    {
        if (m_GroupRoot != null)
            m_GroupRoot.SetActive(isVisible);

        if (m_Toggle != null)
            m_Toggle.gameObject.SetActive(isVisible);

        var showInputField = isVisible && IsEnabled;
        if (m_InputField != null)
        {
            m_InputField.gameObject.SetActive(showInputField);
            m_InputField.interactable = showInputField;
        }
    }

    public void Configure()
    {
        if (m_InputField == null)
            return;

        if (m_InputField.characterLimit != m_BitCount)
            m_InputField.characterLimit = m_BitCount;
    }

    public void Reset()
    {
        if (m_InputField != null)
            m_InputField.SetTextWithoutNotify(string.Empty);

        if (m_Toggle != null)
            m_Toggle.SetIsOnWithoutNotify(false);
    }

    string GetSubmittedBits()
    {
        if (m_InputField == null)
            return string.Empty;

        return PracticeDecodeBitText.Normalize(m_InputField.text);
    }
}

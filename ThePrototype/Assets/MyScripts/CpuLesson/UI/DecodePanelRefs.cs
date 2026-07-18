using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
sealed class DecodeLessonPanelRefs
{
    [SerializeField]
    TMP_Text m_OpcodeText;

    [SerializeField]
    TMP_Text m_PracticeDecodingText;

    [SerializeField]
    TMP_Text m_RegisterText;

    [SerializeField]
    TMP_Text m_FunctText;

    public TMP_Text OpcodeText => m_OpcodeText;
    public TMP_Text PracticeDecodingText => m_PracticeDecodingText;
    public TMP_Text RegisterText => m_RegisterText;
    public TMP_Text FunctText => m_FunctText;
}

[System.Serializable]
sealed class DecodeHintPanelRefs
{
    [SerializeField]
    GameObject m_InfoRoot;

    [SerializeField]
    TMP_Dropdown m_InfoDropdown;

    [SerializeField]
    Button m_HintButton;

    [SerializeField]
    TMP_Text m_InfoText;

    [SerializeField]
    TMP_Text m_HintText;

    public GameObject InfoRoot => m_InfoRoot;
    public TMP_Dropdown InfoDropdown => m_InfoDropdown;
    public Button HintButton => m_HintButton;
    public TMP_Text InfoText => m_InfoText;
    public TMP_Text HintText => m_HintText;
}

[System.Serializable]
sealed class DecodeLearnInteractionRefs
{
    [SerializeField]
    GameObject m_OpcodeGroupRoot;

    [SerializeField]
    TMP_Dropdown m_OpcodeDropdown;

    [SerializeField]
    TMP_Text m_OpcodeSelectionText;

    [SerializeField]
    GameObject m_FunctGroupRoot;

    [SerializeField]
    TMP_Dropdown m_FunctDropdown;

    [SerializeField]
    TMP_Text m_FunctSelectionText;

    public GameObject OpcodeGroupRoot => m_OpcodeGroupRoot;
    public TMP_Dropdown OpcodeDropdown => m_OpcodeDropdown;
    public TMP_Text OpcodeSelectionText => m_OpcodeSelectionText;
    public GameObject FunctGroupRoot => m_FunctGroupRoot;
    public TMP_Dropdown FunctDropdown => m_FunctDropdown;
    public TMP_Text FunctSelectionText => m_FunctSelectionText;
}

[System.Serializable]
sealed class DecodePracticeInteractionRefs
{
    [SerializeField]
    GameObject m_Root;

    [SerializeField]
    TMP_Text m_BinaryText;

    [SerializeField]
    TMP_Text m_StatusText;

    [SerializeField]
    GameObject m_OpcodeGroupRoot;

    [SerializeField]
    TMP_InputField m_OpcodeInputField;

    [SerializeField]
    GameObject m_RsGroupRoot;

    [SerializeField]
    TMP_InputField m_RsInputField;

    [SerializeField]
    GameObject m_RtGroupRoot;

    [SerializeField]
    TMP_InputField m_RtInputField;

    [SerializeField]
    GameObject m_RdGroupRoot;

    [SerializeField]
    Toggle m_RdToggle;

    [SerializeField]
    TMP_InputField m_RdInputField;

    [SerializeField]
    GameObject m_ImmediateGroupRoot;

    [SerializeField]
    Toggle m_ImmediateToggle;

    [SerializeField]
    TMP_InputField m_ImmediateInputField;

    [SerializeField]
    GameObject m_FunctGroupRoot;

    [SerializeField]
    Toggle m_FunctToggle;

    [SerializeField]
    TMP_InputField m_FunctInputField;

    public GameObject Root => m_Root;
    public TMP_Text BinaryText => m_BinaryText;
    public TMP_Text StatusText => m_StatusText;
    public GameObject OpcodeGroupRoot => m_OpcodeGroupRoot;
    public TMP_InputField OpcodeInputField => m_OpcodeInputField;
    public GameObject RsGroupRoot => m_RsGroupRoot;
    public TMP_InputField RsInputField => m_RsInputField;
    public GameObject RtGroupRoot => m_RtGroupRoot;
    public TMP_InputField RtInputField => m_RtInputField;
    public GameObject RdGroupRoot => m_RdGroupRoot;
    public Toggle RdToggle => m_RdToggle;
    public TMP_InputField RdInputField => m_RdInputField;
    public GameObject ImmediateGroupRoot => m_ImmediateGroupRoot;
    public Toggle ImmediateToggle => m_ImmediateToggle;
    public TMP_InputField ImmediateInputField => m_ImmediateInputField;
    public GameObject FunctGroupRoot => m_FunctGroupRoot;
    public Toggle FunctToggle => m_FunctToggle;
    public TMP_InputField FunctInputField => m_FunctInputField;
}

[System.Serializable]
sealed class DecodeSharedInteractionRefs
{
    [SerializeField]
    TMP_Text m_RegisterBodyText;

    [SerializeField]
    TMP_Text m_RegisterSelectionText;

    [SerializeField]
    TMP_Text m_Feedback;

    [SerializeField]
    Button m_ActionButton;

    [SerializeField]
    TMP_Text m_ActionLabel;

    public TMP_Text RegisterBodyText => m_RegisterBodyText;
    public TMP_Text RegisterSelectionText => m_RegisterSelectionText;
    public TMP_Text Feedback => m_Feedback;
    public Button ActionButton => m_ActionButton;
    public TMP_Text ActionLabel => m_ActionLabel;
}

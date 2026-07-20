using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public sealed class PcUpdateLessonPanelRefs
{
    [SerializeField]
    GameObject m_Root;

    [SerializeField]
    TMP_Text m_RuntimeText;

    [SerializeField]
    TMP_Text m_BranchText;

    [SerializeField]
    TMP_Text m_ShiftText;

    [SerializeField]
    TMP_Text m_ResultText;

    [SerializeField]
    TMP_Text m_EndText;

    public GameObject Root => m_Root;
    public TMP_Text RuntimeText => m_RuntimeText;
    public TMP_Text BranchText => m_BranchText;
    public TMP_Text ShiftText => m_ShiftText;
    public TMP_Text ResultText => m_ResultText;
    public TMP_Text EndText => m_EndText;
}

[System.Serializable]
public sealed class PcUpdateHintInfoRefs
{
    [SerializeField]
    TMP_Text m_PcText;

    [SerializeField]
    TMP_Text m_PcSrcText;

    [SerializeField]
    TMP_Text m_BranchText;

    [SerializeField]
    TMP_Text m_JumpText;

    [SerializeField]
    TMP_Text m_ShiftLeftTwoText;

    [SerializeField]
    TMP_Text m_ZeroText;

    public TMP_Text PcText => m_PcText;
    public TMP_Text PcSrcText => m_PcSrcText;
    public TMP_Text BranchText => m_BranchText;
    public TMP_Text JumpText => m_JumpText;
    public TMP_Text ShiftLeftTwoText => m_ShiftLeftTwoText;
    public TMP_Text ZeroText => m_ZeroText;
}

[System.Serializable]
public sealed class PcUpdateInteractionPanelRefs
{
    [SerializeField]
    GameObject m_PcUpdateGroupRoot;

    [SerializeField]
    GameObject m_SignalsGroupRoot;

    [SerializeField]
    Slider m_PcIncrementSlider;

    [SerializeField]
    TMP_Text m_BranchStatusText;

    [SerializeField]
    TMP_Text m_JumpStatusText;

    [SerializeField]
    GameObject m_ImmediateGroupRoot;

    [SerializeField]
    TMP_Text m_ImmediateStatusText;

    [SerializeField]
    Button m_ShiftButton;

    [SerializeField]
    GameObject m_BranchConditionGroupRoot;

    [SerializeField]
    TMP_Text m_ZeroStatusText;

    [SerializeField]
    TMP_Dropdown m_BranchConditionDropdown;

    [SerializeField]
    TMP_Text m_PCSrcStatusText;

    public GameObject PcUpdateGroupRoot => m_PcUpdateGroupRoot;
    public GameObject SignalsGroupRoot => m_SignalsGroupRoot;
    public Slider PcIncrementSlider => m_PcIncrementSlider;
    public TMP_Text BranchStatusText => m_BranchStatusText;
    public TMP_Text JumpStatusText => m_JumpStatusText;
    public GameObject ImmediateGroupRoot => m_ImmediateGroupRoot;
    public TMP_Text ImmediateStatusText => m_ImmediateStatusText;
    public Button ShiftButton => m_ShiftButton;
    public GameObject BranchConditionGroupRoot => m_BranchConditionGroupRoot;
    public TMP_Text ZeroStatusText => m_ZeroStatusText;
    public TMP_Dropdown BranchConditionDropdown => m_BranchConditionDropdown;
    public TMP_Text PCSrcStatusText => m_PCSrcStatusText;
}

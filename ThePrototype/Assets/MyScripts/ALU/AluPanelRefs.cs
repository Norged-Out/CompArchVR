using TMPro;
using UnityEngine;

[System.Serializable]
public sealed class AluLessonPanelRefs
{
    [SerializeField]
    TMP_Text m_RuntimeText;

    public TMP_Text RuntimeText => m_RuntimeText;
}

[System.Serializable]
public sealed class AluHintInfoRefs
{
    [SerializeField]
    TMP_Text m_AluOpText;

    [SerializeField]
    TMP_Text m_AluSrcText;

    [SerializeField]
    TMP_Text m_AluControlText;

    public TMP_Text AluOpText => m_AluOpText;
    public TMP_Text AluSrcText => m_AluSrcText;
    public TMP_Text AluControlText => m_AluControlText;
}

[System.Serializable]
public sealed class AluInteractionPanelRefs
{
    [SerializeField]
    TMP_Text m_AluOpStatusText;

    [SerializeField]
    TMP_Text m_AluSrcStatusText;

    [SerializeField]
    TMP_Text m_Input1StatusText;

    [SerializeField]
    TMP_Text m_Input2StatusText;

    [SerializeField]
    TMP_Dropdown m_FunctDropdown;

    public TMP_Text AluOpStatusText => m_AluOpStatusText;
    public TMP_Text AluSrcStatusText => m_AluSrcStatusText;
    public TMP_Text Input1StatusText => m_Input1StatusText;
    public TMP_Text Input2StatusText => m_Input2StatusText;
    public TMP_Dropdown FunctDropdown => m_FunctDropdown;
}

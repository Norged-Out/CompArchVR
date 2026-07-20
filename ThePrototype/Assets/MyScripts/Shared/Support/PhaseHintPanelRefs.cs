using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public sealed class PhaseHintPanelRefs
{
    [SerializeField]
    GameObject m_Root;

    [SerializeField]
    GameObject m_InfoRoot;

    [SerializeField]
    TMP_Dropdown m_InfoDropdown;

    [SerializeField]
    Button m_HintButton;

    [SerializeField]
    TMP_Text m_HintText;

    public GameObject Root => m_Root;
    public GameObject InfoRoot => m_InfoRoot;
    public TMP_Dropdown InfoDropdown => m_InfoDropdown;
    public Button HintButton => m_HintButton;
    public TMP_Text HintText => m_HintText;
}

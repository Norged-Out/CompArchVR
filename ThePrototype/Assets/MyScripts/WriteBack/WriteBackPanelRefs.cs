using TMPro;
using UnityEngine;

[System.Serializable]
public sealed class WriteBackLessonPanelRefs
{
    [SerializeField]
    GameObject m_Root;

    [SerializeField]
    TMP_Text m_RuntimeText;

    public GameObject Root => m_Root;
    public TMP_Text RuntimeText => m_RuntimeText;
}

[System.Serializable]
public sealed class WriteBackHintInfoRefs
{
    [SerializeField]
    TMP_Text m_RegDstText;

    [SerializeField]
    TMP_Text m_RegWriteText;

    [SerializeField]
    TMP_Text m_MemToRegText;

    public TMP_Text RegDstText => m_RegDstText;
    public TMP_Text RegWriteText => m_RegWriteText;
    public TMP_Text MemToRegText => m_MemToRegText;
}

[System.Serializable]
public sealed class WriteBackInteractionPanelRefs
{
    [SerializeField]
    TMP_Text m_RegDstStatusText;

    [SerializeField]
    TMP_Text m_RegWriteStatusText;

    [SerializeField]
    TMP_Text m_MemToRegStatusText;

    [SerializeField]
    TMP_Text m_RegisterStatusText;

    [SerializeField]
    TMP_Text m_DataStatusText;

    public TMP_Text RegDstStatusText => m_RegDstStatusText;
    public TMP_Text RegWriteStatusText => m_RegWriteStatusText;
    public TMP_Text MemToRegStatusText => m_MemToRegStatusText;
    public TMP_Text RegisterStatusText => m_RegisterStatusText;
    public TMP_Text DataStatusText => m_DataStatusText;
}

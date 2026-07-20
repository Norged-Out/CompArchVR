using TMPro;
using UnityEngine;

[System.Serializable]
public sealed class MemoryLessonPanelRefs
{
    [SerializeField]
    GameObject m_Root;

    [SerializeField]
    TMP_Text m_RuntimeText;

    [SerializeField]
    TMP_Text m_LoadText;

    [SerializeField]
    TMP_Text m_StoreText;

    public GameObject Root => m_Root;
    public TMP_Text RuntimeText => m_RuntimeText;
    public TMP_Text LoadText => m_LoadText;
    public TMP_Text StoreText => m_StoreText;
}

[System.Serializable]
public sealed class MemoryHintInfoRefs
{
    [SerializeField]
    TMP_Text m_MemReadText;

    [SerializeField]
    TMP_Text m_MemWriteText;

    public TMP_Text MemReadText => m_MemReadText;
    public TMP_Text MemWriteText => m_MemWriteText;
}

[System.Serializable]
public sealed class MemoryInteractionPanelRefs
{
    [SerializeField]
    TMP_Text m_MemReadStatusText;

    [SerializeField]
    TMP_Text m_MemWriteStatusText;

    [SerializeField]
    TMP_Text m_AddressStatusText;

    [SerializeField]
    TMP_Text m_DataStatusText;

    public TMP_Text MemReadStatusText => m_MemReadStatusText;
    public TMP_Text MemWriteStatusText => m_MemWriteStatusText;
    public TMP_Text AddressStatusText => m_AddressStatusText;
    public TMP_Text DataStatusText => m_DataStatusText;
}

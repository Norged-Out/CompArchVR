using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// Physical value packet emitted by a successful register scanner.
/// This separates the logical register identity from the datapath value that
/// gets carried into later stages such as the ALU and memory.
/// </summary>
[DisallowMultipleComponent]
public class DataPacketToken : MonoBehaviour
{
    [SerializeField]
    DataPacketRole m_PacketRole = DataPacketRole.None;

    [SerializeField]
    string m_SourceRegisterId = string.Empty;

    [SerializeField]
    string m_SourceDisplayLabel = string.Empty;

    [SerializeField]
    int m_Value;

    [Header("Scene References")]

    [SerializeField]
    TMP_Text m_LabelText;

    [SerializeField]
    TMP_Text m_ValueText;

    [SerializeField]
    XRGrabInteractable m_GrabInteractable;

    public DataPacketRole PacketRole => m_PacketRole;
    public string SourceRegisterId => m_SourceRegisterId;
    public string SourceDisplayLabel => m_SourceDisplayLabel;
    public int Value => m_Value;
    public bool IsGrabbed => m_GrabInteractable != null && m_GrabInteractable.isSelected;

    void Awake()
    {
        CacheReferences();
        RefreshText();
    }

    void OnValidate()
    {
        CacheReferences();
        RefreshText();
    }

    public void Configure(
        DataPacketRole packetRole,
        string sourceRegisterId,
        string sourceDisplayLabel,
        int value)
    {
        m_PacketRole = packetRole;
        m_SourceRegisterId = sourceRegisterId;
        m_SourceDisplayLabel = sourceDisplayLabel;
        m_Value = value;
        RefreshText();
    }

    void CacheReferences()
    {
        m_GrabInteractable ??= GetComponent<XRGrabInteractable>();

        if (m_LabelText == null || m_ValueText == null)
        {
            foreach (var textMesh in GetComponentsInChildren<TMP_Text>(true))
            {
                if (textMesh == null)
                    continue;

                if (m_LabelText == null)
                {
                    m_LabelText = textMesh;
                    continue;
                }

                if (m_ValueText == null && textMesh != m_LabelText)
                    m_ValueText = textMesh;
            }
        }
    }

    void RefreshText()
    {
        if (m_LabelText != null)
            m_LabelText.text = BuildLabel();

        if (m_ValueText != null)
            m_ValueText.text = m_Value.ToString();
    }

    string BuildLabel()
    {
        return m_PacketRole switch
        {
            DataPacketRole.ReadData1 => $"RD1 {m_SourceDisplayLabel}",
            DataPacketRole.ReadData2 => $"RD2 {m_SourceDisplayLabel}",
            DataPacketRole.Immediate => "IMM",
            DataPacketRole.AluResult => "ALU",
            DataPacketRole.MemoryData => "MEM",
            _ => m_SourceDisplayLabel,
        };
    }
}

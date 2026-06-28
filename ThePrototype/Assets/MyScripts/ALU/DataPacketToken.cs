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
    XRGrabInteractable m_GrabInteractable;

    [SerializeField]
    Rigidbody m_Rigidbody;

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
        m_Rigidbody ??= GetComponent<Rigidbody>();
        m_LabelText ??= GetComponentInChildren<TMP_Text>(true);
    }

    void RefreshText()
    {
        if (m_LabelText != null)
            m_LabelText.text = BuildLabel();
    }

    string BuildLabel()
    {
        var baseLabel = m_PacketRole switch
        {
            DataPacketRole.ReadData1 => "Read Data 1",
            DataPacketRole.ReadData2 => "Read Data 2",
            DataPacketRole.Immediate => "Immediate",
            DataPacketRole.AluResult => "ALU Result",
            DataPacketRole.MemoryData => "Memory Data",
            _ => m_SourceDisplayLabel,
        };

        // Keep the packet label to a single line for now.
        // return $"{baseLabel}\n{m_Value}";
        return baseLabel;
    }

    public void LatchInPlace(Transform parentTransform)
    {
        CacheReferences();

        if (m_Rigidbody != null)
        {
            m_Rigidbody.linearVelocity = Vector3.zero;
            m_Rigidbody.angularVelocity = Vector3.zero;
            m_Rigidbody.isKinematic = true;
            m_Rigidbody.useGravity = false;
        }

        if (m_GrabInteractable != null)
            m_GrabInteractable.enabled = false;

        if (parentTransform != null)
            transform.SetParent(parentTransform, true);
    }
}

using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using System.Collections.Generic;

/// <summary>
/// Physical value packet emitted by a successful register scanner.
/// This separates the logical register identity from the datapath value that
/// gets carried into later stages such as the ALU and memory.
/// </summary>
[DisallowMultipleComponent]
public class DataPacketToken : MonoBehaviour
{
    static readonly HashSet<DataPacketToken> s_ActiveTokens = new();

    [SerializeField]
    DataPacketRole m_PacketRole = DataPacketRole.None;

    [SerializeField]
    string m_SourceRegisterId = string.Empty;

    [SerializeField]
    string m_SourceDisplayLabel = string.Empty;

    [SerializeField]
    int m_Value;

    [SerializeField]
    bool m_IsSignExtended;

    [Header("Scene References")]

    [SerializeField]
    TMP_Text m_LabelText;

    [SerializeField]
    XRGrabInteractable m_GrabInteractable;

    [SerializeField]
    Rigidbody m_Rigidbody;

    Vector3 m_SpawnPosition;
    Quaternion m_SpawnRotation;
    Transform m_SpawnParent;
    bool m_HasCachedSpawnPose;

    public DataPacketRole PacketRole => m_PacketRole;
    public string SourceRegisterId => m_SourceRegisterId;
    public string SourceDisplayLabel => m_SourceDisplayLabel;
    public int Value => m_Value;
    public bool IsSignExtended => m_IsSignExtended;
    public bool IsGrabbed => m_GrabInteractable != null && m_GrabInteractable.isSelected;
    public bool IsLatched => m_GrabInteractable != null && !m_GrabInteractable.enabled;
    public static IReadOnlyCollection<DataPacketToken> ActiveTokens => s_ActiveTokens;

    void Awake()
    {
        CacheReferences();
        CacheSpawnPose();
        RefreshText();
    }

    void OnEnable()
    {
        CacheReferences();
        CacheSpawnPose();
        s_ActiveTokens.Add(this);
    }

    void OnDisable()
    {
        s_ActiveTokens.Remove(this);
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
        Configure(packetRole, sourceRegisterId, sourceDisplayLabel, value, false);
    }

    /// <summary>
    /// Configures the packet's role, source identity, stored value, and
    /// whether the payload is already sign-extended for datapath use.
    /// </summary>
    public void Configure(
        DataPacketRole packetRole,
        string sourceRegisterId,
        string sourceDisplayLabel,
        int value,
        bool isSignExtended)
    {
        m_PacketRole = packetRole;
        m_SourceRegisterId = sourceRegisterId;
        m_SourceDisplayLabel = sourceDisplayLabel;
        m_Value = value;
        m_IsSignExtended = isSignExtended;
        RefreshText();
    }

    /// <summary>
    /// Marks an existing immediate packet as sign-extended in place.
    /// The value can also be overwritten if a later sign-extension station
    /// wants to explicitly rewrite the payload before ALU/MEM use.
    /// </summary>
    public void MarkSignExtended(int signExtendedValue)
    {
        m_Value = signExtendedValue;
        m_IsSignExtended = true;
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
            DataPacketRole.Zero => "Zero",
            _ => m_SourceDisplayLabel,
        };

        // Keep the packet label to a single line for now.
        // return $"{baseLabel}\n{m_Value}";
        return baseLabel;
    }

    public void LatchInPlace(Transform parentTransform)
    {
        CacheReferences();

        // A latched packet becomes part of a datapath node, so it stops
        // behaving like a loose grabbable object.
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

    /// <summary>
    /// Releases a previously latched packet back into the scene so the learner
    /// can pick it up again after a control-path change invalidates the input.
    /// </summary>
    public void ReleaseFromLatch()
    {
        CacheReferences();

        transform.SetParent(null, true);

        if (m_Rigidbody != null)
        {
            m_Rigidbody.isKinematic = false;
            m_Rigidbody.useGravity = true;
        }

        if (m_GrabInteractable != null)
            m_GrabInteractable.enabled = true;
    }

    /// <summary>
    /// Returns the packet to the exact pose where it originally spawned.
    /// Values and datapath metadata are intentionally preserved.
    /// </summary>
    public void ResetToSpawnPose()
    {
        if (!m_HasCachedSpawnPose || IsLatched)
            return;

        CacheReferences();

        transform.SetParent(m_SpawnParent, true);
        transform.SetPositionAndRotation(m_SpawnPosition, m_SpawnRotation);

        if (m_Rigidbody != null)
        {
            m_Rigidbody.linearVelocity = Vector3.zero;
            m_Rigidbody.angularVelocity = Vector3.zero;
        }
    }

    void CacheSpawnPose()
    {
        if (m_HasCachedSpawnPose)
            return;

        m_SpawnParent = transform.parent;
        m_SpawnPosition = transform.position;
        m_SpawnRotation = transform.rotation;
        m_HasCachedSpawnPose = true;
    }
}

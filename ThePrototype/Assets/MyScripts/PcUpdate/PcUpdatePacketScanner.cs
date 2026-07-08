using UnityEngine;

/// <summary>
/// Packet receiver used by the Program Counter update station.
/// It mirrors the memory-unit pillar behavior, but adds immediate-specific
/// issue reporting so the PC update UI can explain whether an offset still
/// needs sign extension or shift-left-by-2.
/// </summary>
[DisallowMultipleComponent]
public class PcUpdatePacketScanner : MemoryPillarScannerBase
{
    public enum PacketIssue
    {
        None,
        WrongPacketType,
        ImmediateNotSignExtended,
        ImmediateNotShifted,
    }

    [SerializeField]
    Collider m_ScanZone;

    [SerializeField]
    DataPacketRole m_ExpectedPacketRole = DataPacketRole.Immediate;

    [SerializeField]
    bool m_RequireSignExtended = true;

    [SerializeField]
    bool m_RequireShiftedImmediate = false;

    readonly System.Collections.Generic.HashSet<DataPacketToken> m_PacketsInZone = new();
    PacketIssue m_CurrentIssue;

    public DataPacketToken AcceptedPacket { get; private set; }
    public DataPacketRole ExpectedPacketRole => m_ExpectedPacketRole;
    public PacketIssue CurrentIssue => m_CurrentIssue;

    public event System.Action<PcUpdatePacketScanner, DataPacketToken> PacketAccepted;

    protected override void Awake()
    {
        base.Awake();
        BindZoneHelper();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        BindZoneHelper();
    }

    public void SetActive(bool isActive)
    {
        SetScannerActive(isActive);
    }

    public void SetExpectedPacketRole(DataPacketRole packetRole)
    {
        m_ExpectedPacketRole = packetRole;

        if (AcceptedPacket != null && AcceptedPacket.PacketRole != m_ExpectedPacketRole)
            ResetScanner();
    }

    public void SetImmediateRequirements(bool requireSignExtended, bool requireShiftedImmediate)
    {
        m_RequireSignExtended = requireSignExtended;
        m_RequireShiftedImmediate = requireShiftedImmediate;

        if (AcceptedPacket == null)
            return;

        if (m_ExpectedPacketRole != DataPacketRole.Immediate)
            return;

        if ((m_RequireSignExtended && !AcceptedPacket.IsSignExtended) ||
            (m_RequireShiftedImmediate && !AcceptedPacket.IsShiftedLeftTwo))
        {
            ResetScanner();
        }
    }

    public new void ResetScanner()
    {
        m_PacketsInZone.Clear();
        AcceptedPacket = null;
        m_CurrentIssue = PacketIssue.None;
        base.ResetScanner();
    }

    public void ConsumeAcceptedPacket()
    {
        var packetToConsume = AcceptedPacket;
        if (packetToConsume != null)
        {
            if (Application.isPlaying)
                Destroy(packetToConsume.gameObject);
            else
                DestroyImmediate(packetToConsume.gameObject);
        }

        ResetScanner();
    }

    public void NotifyPacketEntered(DataPacketToken dataPacketToken)
    {
        if (dataPacketToken != null)
            m_PacketsInZone.Add(dataPacketToken);
    }

    public void NotifyPacketExited(DataPacketToken dataPacketToken)
    {
        if (dataPacketToken == null)
            return;

        m_PacketsInZone.Remove(dataPacketToken);

        if (!IsLatchedSuccessful && AcceptedPacket == dataPacketToken)
            AcceptedPacket = null;
    }

    protected override void CacheVisualReferences()
    {
        base.CacheVisualReferences();

        if (m_ScanZone == null)
        {
            var scanZoneTransform = transform.Find("Scan Zone");
            if (scanZoneTransform != null)
                m_ScanZone = scanZoneTransform.GetComponent<Collider>();
        }
    }

    protected override Component GetStableCandidate()
    {
        m_PacketsInZone.RemoveWhere(packet => packet == null);

        foreach (var dataPacket in m_PacketsInZone)
        {
            if (dataPacket == null || dataPacket.IsGrabbed)
                continue;

            return dataPacket;
        }

        return null;
    }

    protected override bool IsImmediateMismatch(Component candidate)
    {
        if (candidate is not DataPacketToken dataPacketToken)
        {
            m_CurrentIssue = PacketIssue.WrongPacketType;
            return true;
        }

        if (dataPacketToken.PacketRole != m_ExpectedPacketRole)
        {
            m_CurrentIssue = PacketIssue.WrongPacketType;
            return true;
        }

        if (m_ExpectedPacketRole == DataPacketRole.Immediate)
        {
            if (m_RequireSignExtended && !dataPacketToken.IsSignExtended)
            {
                m_CurrentIssue = PacketIssue.ImmediateNotSignExtended;
                return true;
            }

            if (m_RequireShiftedImmediate && !dataPacketToken.IsShiftedLeftTwo)
            {
                m_CurrentIssue = PacketIssue.ImmediateNotShifted;
                return true;
            }
        }

        m_CurrentIssue = PacketIssue.None;
        return false;
    }

    protected override void OnImmediateMismatch(Component _)
    {
        AcceptedPacket = null;
    }

    protected override void HandleScannerReset()
    {
        AcceptedPacket = null;
        m_CurrentIssue = PacketIssue.None;
    }

    protected override void OnCandidateLost()
    {
        m_CurrentIssue = PacketIssue.None;
    }

    protected override void HandleStableCandidate(Component candidate)
    {
        var stablePacket = candidate as DataPacketToken;
        if (stablePacket == null)
            return;

        AcceptedPacket = stablePacket;
        m_CurrentIssue = PacketIssue.None;
        AcceptedPacket.LatchInPlace(transform);
        PacketAccepted?.Invoke(this, stablePacket);
        MarkSuccess();
    }

    void BindZoneHelper()
    {
        if (m_ScanZone == null)
            return;

        var helper = m_ScanZone.GetComponent<PcUpdatePacketScannerZone>();
        if (helper == null)
            helper = m_ScanZone.gameObject.AddComponent<PcUpdatePacketScannerZone>();

        helper.Bind(this);
    }
}

using UnityEngine;

/// <summary>
/// ALU-side packet receiver.
/// This is intentionally narrower than the register scanner: it only validates
/// packet role and latches the currently resting packet for later ALU logic.
/// </summary>
[DisallowMultipleComponent]
public class AluInputScanner : PedestalScannerBase
{
    [SerializeField]
    DataPacketRole m_ExpectedPacketRole = DataPacketRole.ReadData1;

    [SerializeField]
    Collider m_ScanZone;

    [SerializeField]
    float m_RequiredStableSeconds = 1f;

    [SerializeField]
    float m_LocalPressedOffsetY = -0.02f;

    readonly System.Collections.Generic.HashSet<DataPacketToken> m_PacketsInZone = new();

    public DataPacketRole ExpectedPacketRole => m_ExpectedPacketRole;
    public DataPacketToken AcceptedPacket { get; private set; }
    public int AcceptedValue => AcceptedPacket != null ? AcceptedPacket.Value : 0;

    public event System.Action<AluInputScanner, DataPacketToken> PacketAccepted;

    protected override float RequiredStableSeconds => m_RequiredStableSeconds;
    protected override float PressedOffsetY => m_LocalPressedOffsetY;

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
        SetStepActive(isActive);
    }

    public void SetExpectedPacketRole(DataPacketRole expectedPacketRole)
    {
        if (m_ExpectedPacketRole == expectedPacketRole)
            return;

        m_ExpectedPacketRole = expectedPacketRole;

        // If ALUSrc changes mid-phase, an already accepted packet may become
        // invalid. Drop it so the learner has to provide the new correct type.
        if (AcceptedPacket != null && AcceptedPacket.PacketRole != m_ExpectedPacketRole)
            ResetScanner();
    }

    public void ResetScanner()
    {
        m_PacketsInZone.Clear();
        ClearAcceptedPacket();
        base.ResetScanner();
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
            ClearAcceptedPacket();
    }

    void ClearAcceptedPacket()
    {
        AcceptedPacket = null;
    }

    protected override Component GetStableCandidate()
    {
        m_PacketsInZone.RemoveWhere(packet => packet == null);

        // The first ungrabbed packet in the trigger is treated as the current
        // candidate. We keep this intentionally simple for the two-input ALU MVP.
        foreach (var dataPacket in m_PacketsInZone)
        {
            if (dataPacket == null || dataPacket.IsGrabbed)
                continue;

            return dataPacket;
        }

        return null;
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

    void BindZoneHelper()
    {
        if (m_ScanZone == null)
            return;

        var helper = m_ScanZone.GetComponent<AluInputScannerZone>();
        if (helper == null)
            helper = m_ScanZone.gameObject.AddComponent<AluInputScannerZone>();

        helper.Bind(this);
    }

    protected override bool IsImmediateMismatch(Component candidate)
    {
        return candidate is DataPacketToken dataPacketToken &&
               dataPacketToken.PacketRole != m_ExpectedPacketRole;
    }

    protected override void OnImmediateMismatch(Component candidate)
    {
        ClearAcceptedPacket();
    }

    protected override void HandleScannerReset()
    {
        ClearAcceptedPacket();
    }

    protected override void HandleStableCandidate(Component candidate)
    {
        var stableCandidate = candidate as DataPacketToken;
        if (stableCandidate == null)
            return;

        // Once a packet has remained stable long enough, the scanner owns it
        // for the rest of the phase until the controller explicitly resets.
        AcceptedPacket = stableCandidate;
        AcceptedPacket.LatchInPlace(transform);
        PacketAccepted?.Invoke(this, stableCandidate);
        MarkSuccess();
    }
}

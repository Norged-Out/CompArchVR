using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Memory-unit input that accepts the address packet produced by the ALU.
/// It watches a trigger zone, waits for a stable dropped packet, and latches
/// the accepted address in place for the current memory access.
/// </summary>
[DisallowMultipleComponent]
public class MemoryAddressScanner : MemoryPillarScannerBase
{
    [SerializeField]
    Collider m_ScanZone;

    [SerializeField]
    DataPacketRole m_ExpectedPacketRole = DataPacketRole.AluResult;

    readonly HashSet<DataPacketToken> m_PacketsInZone = new();

    public DataPacketToken AcceptedPacket { get; private set; }
    public int AcceptedAddress => AcceptedPacket != null ? AcceptedPacket.Value : 0;
    public DataPacketRole ExpectedPacketRole => m_ExpectedPacketRole;

    public event Action<MemoryAddressScanner, DataPacketToken> PacketAccepted;

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

    public new void ResetScanner()
    {
        m_PacketsInZone.Clear();
        AcceptedPacket = null;
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

        if (AcceptedPacket == dataPacketToken)
        {
            AcceptedPacket = null;
            base.ResetScanner();
        }
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

    protected override void HandleScannerReset()
    {
        AcceptedPacket = null;
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

    protected override void HandleStableCandidate(Component candidate)
    {
        var stablePacket = candidate as DataPacketToken;
        if (stablePacket == null)
            return;

        if (stablePacket.PacketRole != m_ExpectedPacketRole)
        {
            FlashFailure();
            return;
        }

        AcceptedPacket = stablePacket;
        AcceptedPacket.LatchInPlace(transform);
        PacketAccepted?.Invoke(this, stablePacket);
        MarkSuccess();
    }

    void BindZoneHelper()
    {
        if (m_ScanZone == null)
            return;

        var helper = m_ScanZone.GetComponent<MemoryAddressScannerZone>();
        if (helper == null)
            helper = m_ScanZone.gameObject.AddComponent<MemoryAddressScannerZone>();

        helper.Bind(this);
    }
}

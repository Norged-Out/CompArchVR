using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Physical write-back pedestal that accepts the data packet to be written into
/// the register file. The expected packet role is driven by MemToReg.
/// </summary>
[DisallowMultipleComponent]
public class WriteBackPacketScanner : PedestalScannerBase
{
    [SerializeField]
    Collider m_ScanZone;

    [SerializeField]
    float m_RequiredStableSeconds = 1f;

    [SerializeField]
    float m_LocalPressedOffsetY = -0.02f;

    [SerializeField]
    DataPacketRole m_ExpectedPacketRole = DataPacketRole.AluResult;

    readonly HashSet<DataPacketToken> m_PacketsInZone = new();

    public DataPacketToken AcceptedPacket { get; private set; }
    public DataPacketRole ExpectedPacketRole => m_ExpectedPacketRole;

    public event Action<WriteBackPacketScanner, DataPacketToken> PacketAccepted;

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
        m_ExpectedPacketRole = expectedPacketRole;

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

    /// <summary>
    /// Clears the cached packet reference after the controller has consumed it.
    /// The scanner stays visually successful until the phase changes.
    /// </summary>
    public void ConsumeAcceptedPacket()
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

        var helper = m_ScanZone.GetComponent<WriteBackPacketScannerZone>();
        if (helper == null)
            helper = m_ScanZone.gameObject.AddComponent<WriteBackPacketScannerZone>();

        helper.Bind(this);
    }

    protected override void HandleScannerReset()
    {
        AcceptedPacket = null;
    }

    protected override void HandleStableCandidate(Component candidate)
    {
        var stableCandidate = candidate as DataPacketToken;
        if (stableCandidate == null)
            return;

        if (stableCandidate.PacketRole != m_ExpectedPacketRole)
        {
            FlashFailure();
            return;
        }

        AcceptedPacket = stableCandidate;
        AcceptedPacket.LatchInPlace(transform);
        PacketAccepted?.Invoke(this, stableCandidate);
        MarkSuccess();
    }
}

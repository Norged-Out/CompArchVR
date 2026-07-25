using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

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
    XRSocketInteractor m_ScanSocket;

    [SerializeField]
    float m_RequiredStableSeconds = 1f;

    [SerializeField]
    float m_LocalPressedOffsetY = -0.02f;

    [SerializeField]
    DataPacketRole m_ExpectedPacketRole = DataPacketRole.AluResult;

    readonly HashSet<DataPacketToken> m_PacketsInZone = new();
    DataPacketToken m_SocketedPacket;

    public DataPacketToken AcceptedPacket { get; private set; }
    public DataPacketRole ExpectedPacketRole => m_ExpectedPacketRole;

    public event Action<WriteBackPacketScanner, DataPacketToken> PacketAccepted;
    public event Action<WriteBackPacketScanner, DataPacketToken> PacketRejected;

    protected override float RequiredStableSeconds => m_RequiredStableSeconds;
    protected override float PressedOffsetY => m_LocalPressedOffsetY;

    protected override void Awake()
    {
        base.Awake();
        BindZoneHelper();
        ConfigureSocketState();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        BindZoneHelper();
        ConfigureSocketState();
        HookSocketEvents(true);
    }

    void OnDisable()
    {
        HookSocketEvents(false);
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
        m_SocketedPacket = null;
        if (AcceptedPacket != null)
            AcceptedPacket.ReleaseFromLatch();

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

        if (!IsLatchedSuccessful && AcceptedPacket == dataPacketToken)
            ResetScanner();
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
        if (m_SocketedPacket != null)
            return m_SocketedPacket;

        m_PacketsInZone.RemoveWhere(packet => packet == null);

        foreach (var dataPacket in m_PacketsInZone)
        {
            if (dataPacket == null || dataPacket.IsGrabbed)
                continue;

            return dataPacket;
        }

        return null;
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

    void ConfigureSocketState()
    {
        if (m_ScanSocket == null)
            m_ScanSocket = GetComponent<XRSocketInteractor>();

        if (m_ScanSocket == null)
            return;

        m_ScanSocket.socketActive = true;
    }

    void HookSocketEvents(bool subscribe)
    {
        if (m_ScanSocket == null)
            return;

        m_ScanSocket.selectEntered.RemoveListener(HandleSocketSelectEntered);
        m_ScanSocket.selectExited.RemoveListener(HandleSocketSelectExited);

        if (!subscribe)
            return;

        m_ScanSocket.selectEntered.AddListener(HandleSocketSelectEntered);
        m_ScanSocket.selectExited.AddListener(HandleSocketSelectExited);
    }

    void HandleSocketSelectEntered(SelectEnterEventArgs args)
    {
        m_SocketedPacket = args.interactableObject?.transform.GetComponentInParent<DataPacketToken>();
    }

    void HandleSocketSelectExited(SelectExitEventArgs args)
    {
        var packet = args.interactableObject?.transform.GetComponentInParent<DataPacketToken>();
        if (packet != null && packet == m_SocketedPacket)
            m_SocketedPacket = null;
    }

    protected override void HandleScannerReset()
    {
        if (AcceptedPacket != null)
            AcceptedPacket.ReleaseFromLatch();

        AcceptedPacket = null;
    }

    protected override void HandleStableCandidate(Component candidate)
    {
        var stableCandidate = candidate as DataPacketToken;
        if (stableCandidate == null)
            return;

        if (stableCandidate.PacketRole != m_ExpectedPacketRole)
        {
            PacketRejected?.Invoke(this, stableCandidate);
            FlashFailure();
            return;
        }

        AcceptedPacket = stableCandidate;
        AcceptedPacket.LatchInPlace(transform);
        PacketAccepted?.Invoke(this, stableCandidate);
        MarkSuccess();
    }
}

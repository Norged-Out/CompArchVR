using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

/// <summary>
/// Shared packet-latching logic for the Memory Unit's pillar scanners.
/// Derived scanners only provide their concrete zone helper and the event that
/// should fire once a packet is accepted.
/// </summary>
public abstract class MemoryPacketLatchScannerBase<TZone, TScanner> : MemoryPillarScannerBase
    where TZone : MemoryPacketScannerZoneBase<TScanner>
    where TScanner : MemoryPacketLatchScannerBase<TZone, TScanner>
{
    [SerializeField]
    Collider m_ScanZone;

    [SerializeField]
    XRSocketInteractor m_ScanSocket;

    [SerializeField]
    DataPacketRole m_ExpectedPacketRole;

    readonly HashSet<DataPacketToken> m_PacketsInZone = new();
    DataPacketToken m_SocketedPacket;

    /// <summary>
    /// Packet currently accepted by this memory scanner, if any.
    /// </summary>
    public DataPacketToken AcceptedPacket { get; private set; }

    /// <summary>
    /// Packet role that this scanner currently expects.
    /// </summary>
    public DataPacketRole ExpectedPacketRole => m_ExpectedPacketRole;

    /// <summary>
    /// Raised once a stable-but-wrong packet has been rejected by this scanner.
    /// Practice mode uses this to track scanner-attempt budgets without moving
    /// that policy into the scanner itself.
    /// </summary>
    public event Action<TScanner, DataPacketToken> PacketRejected;

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

    protected virtual void OnDisable()
    {
        HookSocketEvents(false);
    }

    protected override void OnValidate()
    {
        base.OnValidate();

        if (m_ScanZone == null)
        {
            var zoneHelper = GetComponentInChildren<TZone>(true);
            if (zoneHelper != null)
                m_ScanZone = zoneHelper.GetComponent<Collider>();
        }

        if (m_ScanSocket == null)
            m_ScanSocket = GetComponent<XRSocketInteractor>();
    }

    /// <summary>
    /// Activates or deactivates the authored scanner visuals and validation.
    /// </summary>
    public void SetActive(bool isActive)
    {
        SetScannerActive(isActive);
    }

    /// <summary>
    /// Updates the required packet role and drops any now-invalid packet.
    /// </summary>
    public void SetExpectedPacketRole(DataPacketRole packetRole)
    {
        m_ExpectedPacketRole = packetRole;

        if (AcceptedPacket != null && AcceptedPacket.PacketRole != m_ExpectedPacketRole)
            ResetScanner();
    }

    /// <summary>
    /// Forwards a packet enter event from the child trigger zone.
    /// </summary>
    public void NotifyPacketEntered(DataPacketToken dataPacketToken)
    {
        if (dataPacketToken != null)
            m_PacketsInZone.Add(dataPacketToken);
    }

    /// <summary>
    /// Forwards a packet exit event from the child trigger zone.
    /// </summary>
    public void NotifyPacketExited(DataPacketToken dataPacketToken)
    {
        if (dataPacketToken == null)
            return;

        m_PacketsInZone.Remove(dataPacketToken);

        if (!IsLatchedSuccessful && AcceptedPacket == dataPacketToken)
            ResetScanner();
    }

    /// <summary>
    /// Resets the authored scanner and releases any packet that is not yet
    /// consumed by its owning phase.
    /// </summary>
    public new void ResetScanner()
    {
        m_PacketsInZone.Clear();
        m_SocketedPacket = null;

        if (AcceptedPacket != null)
            AcceptedPacket.ReleaseFromLatch();

        AcceptedPacket = null;
        base.ResetScanner();
    }

    /// <summary>
    /// Destroys the accepted packet after the owning phase has consumed it.
    /// </summary>
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

    protected override void HandleScannerReset()
    {
        if (AcceptedPacket != null)
            AcceptedPacket.ReleaseFromLatch();

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

    protected override void HandleStableCandidate(Component candidate)
    {
        var stablePacket = candidate as DataPacketToken;
        if (stablePacket == null)
            return;

        if (stablePacket.PacketRole != m_ExpectedPacketRole)
        {
            PacketRejected?.Invoke((TScanner)this, stablePacket);
            FlashFailure();
            return;
        }

        AcceptedPacket = stablePacket;
        AcceptedPacket.LatchInPlace(transform);
        RaisePacketAccepted(stablePacket);
        MarkSuccess();
    }

    void BindZoneHelper()
    {
        if (m_ScanZone == null)
            return;

        var helper = m_ScanZone.GetComponent<TZone>();
        if (helper == null)
            helper = m_ScanZone.gameObject.AddComponent<TZone>();

        helper.Bind((TScanner)this);
    }

    void ConfigureSocketState()
    {
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

    /// <summary>
    /// Called once a correctly typed packet has latched successfully.
    /// </summary>
    protected abstract void RaisePacketAccepted(DataPacketToken packetToken);
}

using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

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
    }

    [SerializeField]
    Collider m_ScanZone;

    [SerializeField]
    XRSocketInteractor m_ScanSocket;

    [SerializeField]
    DataPacketRole m_ExpectedPacketRole = DataPacketRole.Immediate;

    [SerializeField]
    bool m_RequireSignExtended = true;

    readonly System.Collections.Generic.HashSet<DataPacketToken> m_PacketsInZone = new();
    DataPacketToken m_SocketedPacket;
    PacketIssue m_CurrentIssue;

    public DataPacketToken AcceptedPacket { get; private set; }
    public DataPacketRole ExpectedPacketRole => m_ExpectedPacketRole;
    public PacketIssue CurrentIssue => m_CurrentIssue;

    public event System.Action<PcUpdatePacketScanner, DataPacketToken> PacketAccepted;
    public event System.Action<PcUpdatePacketScanner, DataPacketToken, PacketIssue> PacketRejected;

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

    /// <summary>
    /// Enables or disables the pedestal as a whole for the current lesson
    /// state.
    /// </summary>
    public void SetActive(bool isActive)
    {
        SetScannerActive(isActive);
    }

    /// <summary>
    /// Changes which packet role this pedestal currently accepts. If the
    /// latched packet no longer matches, the pedestal resets immediately.
    /// </summary>
    public void SetExpectedPacketRole(DataPacketRole packetRole)
    {
        m_ExpectedPacketRole = packetRole;

        if (AcceptedPacket != null && AcceptedPacket.PacketRole != m_ExpectedPacketRole)
            ResetScanner();
    }

    /// <summary>
    /// Toggles whether Immediate packets must already be sign-extended before
    /// the pedestal will accept them.
    /// </summary>
    public void SetImmediateRequirements(bool requireSignExtended)
    {
        m_RequireSignExtended = requireSignExtended;

        if (AcceptedPacket == null)
            return;

        if (m_ExpectedPacketRole != DataPacketRole.Immediate)
            return;

        if (m_RequireSignExtended && !AcceptedPacket.IsSignExtended)
        {
            ResetScanner();
        }
    }

    /// <summary>
    /// Clears local candidate tracking and releases any currently latched
    /// packet.
    /// </summary>
    public new void ResetScanner()
    {
        m_PacketsInZone.Clear();
        m_SocketedPacket = null;
        if (AcceptedPacket != null)
            AcceptedPacket.ReleaseFromLatch();

        AcceptedPacket = null;
        m_CurrentIssue = PacketIssue.None;
        base.ResetScanner();
    }

    /// <summary>
    /// Destroys the currently accepted packet and then resets the pedestal.
    /// Used when a packet is fully consumed by downstream logic.
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

    /// <summary>
    /// Registers a packet that has entered the trigger zone so the pedestal can
    /// later evaluate it for stable placement.
    /// </summary>
    public void NotifyPacketEntered(DataPacketToken dataPacketToken)
    {
        if (dataPacketToken != null)
            m_PacketsInZone.Add(dataPacketToken);
    }

    /// <summary>
    /// Unregisters a packet that has left the trigger zone and clears the
    /// current acceptance when the packet was not yet fully latched.
    /// </summary>
    public void NotifyPacketExited(DataPacketToken dataPacketToken)
    {
        if (dataPacketToken == null)
            return;

        m_PacketsInZone.Remove(dataPacketToken);

        if (!IsLatchedSuccessful && AcceptedPacket == dataPacketToken)
            AcceptedPacket = null;
    }

    /// <summary>
    /// Falls back to the conventional child-object name when the trigger
    /// collider was not serialized explicitly.
    /// </summary>
    protected override void CacheVisualReferences()
    {
        base.CacheVisualReferences();

        if (m_ScanZone == null)
        {
            var scanZoneTransform = transform.Find("Scan Zone");
            if (scanZoneTransform != null)
                m_ScanZone = scanZoneTransform.GetComponent<Collider>();
        }

        if (m_ScanSocket == null)
            m_ScanSocket = GetComponent<XRSocketInteractor>();
    }

    /// <summary>
    /// Chooses the first non-grabbed packet still resting inside the trigger as
    /// the current scan candidate.
    /// </summary>
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

    /// <summary>
    /// Performs PC-update-specific validation on the current packet candidate.
    /// </summary>
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
        }

        m_CurrentIssue = PacketIssue.None;
        return false;
    }

    /// <summary>
    /// Releases any previously accepted packet when a new invalid candidate
    /// takes its place.
    /// </summary>
    protected override void OnImmediateMismatch(Component _)
    {
        if (_ is DataPacketToken dataPacketToken)
            PacketRejected?.Invoke(this, dataPacketToken, m_CurrentIssue);

        if (AcceptedPacket != null)
            AcceptedPacket.ReleaseFromLatch();

        AcceptedPacket = null;
    }

    /// <summary>
    /// Shared reset hook from the base scanner.
    /// </summary>
    protected override void HandleScannerReset()
    {
        if (AcceptedPacket != null)
            AcceptedPacket.ReleaseFromLatch();

        AcceptedPacket = null;
        m_CurrentIssue = PacketIssue.None;
    }

    /// <summary>
    /// Clears transient issue state when the current packet candidate is lost
    /// before it can be accepted.
    /// </summary>
    protected override void OnCandidateLost()
    {
        m_CurrentIssue = PacketIssue.None;
    }

    /// <summary>
    /// Latches the validated packet in place and notifies the PC update station
    /// that the pedestal is ready.
    /// </summary>
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

    /// <summary>
    /// Ensures the trigger helper component exists on the authored scan zone
    /// and points back to this scanner instance.
    /// </summary>
    void BindZoneHelper()
    {
        if (m_ScanZone == null)
            return;

        var helper = m_ScanZone.GetComponent<PcUpdatePacketScannerZone>();
        if (helper == null)
            helper = m_ScanZone.gameObject.AddComponent<PcUpdatePacketScannerZone>();

        helper.Bind(this);
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
}

using UnityEngine;

/// <summary>
/// ALU-side packet receiver.
/// This is intentionally narrower than the register scanner: it only validates
/// packet role and latches the currently resting packet for later ALU logic.
/// </summary>
[DisallowMultipleComponent]
public class AluInputScanner : MonoBehaviour
{
    enum VisualState
    {
        Inactive,
        Idle,
        Occupied,
        Success,
        Failure,
    }

    [SerializeField]
    DataPacketRole m_ExpectedPacketRole = DataPacketRole.ReadData1;

    [SerializeField]
    Transform m_BodyTransform;

    [SerializeField]
    Renderer m_BodyRenderer;

    [SerializeField]
    Collider m_ScanZone;

    [SerializeField]
    float m_RequiredStableSeconds = 1f;

    [SerializeField]
    float m_PressedOffsetY = -0.02f;

    [SerializeField]
    Color m_InactiveColor = new(0.23f, 0.28f, 0.34f, 1f);

    [SerializeField]
    Color m_IdleColor = new(0.37f, 0.54f, 0.72f, 1f);

    [SerializeField]
    Color m_OccupiedColor = new(0.93f, 0.72f, 0.25f, 1f);

    [SerializeField]
    Color m_SuccessColor = new(0.3f, 0.76f, 0.43f, 1f);

    [SerializeField]
    Color m_FailureColor = new(0.86f, 0.24f, 0.24f, 1f);

    static readonly int k_BaseColorId = Shader.PropertyToID("_BaseColor");
    static readonly int k_ColorId = Shader.PropertyToID("_Color");

    readonly System.Collections.Generic.HashSet<DataPacketToken> m_PacketsInZone = new();

    Vector3 m_BodyRestLocalPosition;
    DataPacketToken m_CurrentCandidate;
    float m_CurrentStableTime;
    bool m_IsActive;
    bool m_IsLatched;
    VisualState m_VisualState = VisualState.Inactive;

    public DataPacketRole ExpectedPacketRole => m_ExpectedPacketRole;
    public DataPacketToken AcceptedPacket { get; private set; }
    public int AcceptedValue => AcceptedPacket != null ? AcceptedPacket.Value : 0;

    public event System.Action<AluInputScanner, DataPacketToken> PacketAccepted;

    void Awake()
    {
        CacheReferences();
        RememberBodyPose();
        BindZoneHelper();
        ApplyVisualState();
    }

    void OnEnable()
    {
        CacheReferences();
        BindZoneHelper();
    }

    void Update()
    {
        if (!m_IsActive || m_IsLatched)
            return;

        var stableCandidate = GetStableCandidate();
        if (stableCandidate == null)
        {
            m_CurrentCandidate = null;
            m_CurrentStableTime = 0f;
            SetVisualState(VisualState.Idle);
            return;
        }

        if (stableCandidate.PacketRole != m_ExpectedPacketRole)
        {
            m_CurrentCandidate = null;
            m_CurrentStableTime = 0f;
            SetVisualState(VisualState.Failure);
            return;
        }

        if (stableCandidate != m_CurrentCandidate)
        {
            m_CurrentCandidate = stableCandidate;
            m_CurrentStableTime = 0f;
        }

        m_CurrentStableTime += Time.deltaTime;
        SetVisualState(VisualState.Occupied);

        if (m_CurrentStableTime < m_RequiredStableSeconds)
            return;

        // Once a packet has remained stable long enough, the scanner owns it
        // for the rest of the phase until the controller explicitly resets.
        AcceptedPacket = stableCandidate;
        AcceptedPacket.LatchInPlace(transform);
        m_IsLatched = true;
        SetVisualState(VisualState.Success);
        PacketAccepted?.Invoke(this, stableCandidate);
    }

    public void SetActive(bool isActive)
    {
        m_IsActive = isActive;
        SetVisualState(isActive ? VisualState.Idle : VisualState.Inactive);
    }

    public void SetExpectedPacketRole(DataPacketRole expectedPacketRole)
    {
        if (m_ExpectedPacketRole == expectedPacketRole)
            return;

        m_ExpectedPacketRole = expectedPacketRole;

        // If ALUSrc changes mid-phase, an already accepted packet may become
        // invalid. Drop it so the learner has to provide the new correct type.
        if (AcceptedPacket != null && AcceptedPacket.PacketRole != m_ExpectedPacketRole)
            ClearAcceptedPacket();

        ApplyVisualState();
    }

    public void ResetScanner()
    {
        m_PacketsInZone.Clear();
        ClearAcceptedPacket();
        SetVisualState(m_IsActive ? VisualState.Idle : VisualState.Inactive);
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

        if (!m_IsLatched && AcceptedPacket == dataPacketToken)
            ClearAcceptedPacket();
    }

    void ClearAcceptedPacket()
    {
        AcceptedPacket = null;
        m_CurrentCandidate = null;
        m_CurrentStableTime = 0f;
        m_IsLatched = false;
    }

    DataPacketToken GetStableCandidate()
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

    void CacheReferences()
    {
        if (m_BodyRenderer == null)
            m_BodyRenderer = GetComponent<Renderer>();

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

    void RememberBodyPose()
    {
        if (m_BodyTransform != null)
            m_BodyRestLocalPosition = m_BodyTransform.localPosition;
    }

    void SetVisualState(VisualState visualState)
    {
        m_VisualState = visualState;
        ApplyVisualState();
    }

    void ApplyVisualState()
    {
        var bodyColor = m_IdleColor;
        var pressed = false;

        switch (m_VisualState)
        {
            case VisualState.Inactive:
                bodyColor = m_InactiveColor;
                break;
            case VisualState.Occupied:
                bodyColor = m_OccupiedColor;
                pressed = true;
                break;
            case VisualState.Success:
                bodyColor = m_SuccessColor;
                pressed = true;
                break;
            case VisualState.Failure:
                bodyColor = m_FailureColor;
                pressed = true;
                break;
        }

        if (m_BodyRenderer != null)
            ApplyRendererColor(m_BodyRenderer, bodyColor);

        if (m_BodyTransform != null)
        {
            var targetLocalPosition = m_BodyRestLocalPosition;
            if (pressed)
                targetLocalPosition.y += m_PressedOffsetY;

            m_BodyTransform.localPosition = targetLocalPosition;
        }
    }

    static void ApplyRendererColor(Renderer targetRenderer, Color color)
    {
        if (targetRenderer == null)
            return;

        var sharedMaterial = targetRenderer.sharedMaterial;
        if (sharedMaterial == null)
            return;

        var propertyBlock = new MaterialPropertyBlock();
        targetRenderer.GetPropertyBlock(propertyBlock);

        if (sharedMaterial.HasProperty(k_BaseColorId))
            propertyBlock.SetColor(k_BaseColorId, color);
        else if (sharedMaterial.HasProperty(k_ColorId))
            propertyBlock.SetColor(k_ColorId, color);
        else
            return;

        targetRenderer.SetPropertyBlock(propertyBlock);
    }

}

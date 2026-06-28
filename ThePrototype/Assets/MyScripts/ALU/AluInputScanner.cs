using TMPro;
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
    TMP_Text m_LabelText;

    [SerializeField]
    TMP_Text m_ValueText;

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
        ApplyVisualState();
    }

    void OnEnable()
    {
        CacheReferences();
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

        AcceptedPacket = stableCandidate;
        AcceptedPacket.LatchInPlace(transform);
        m_IsLatched = true;
        SetVisualState(VisualState.Success);
        UpdateValueText();
        PacketAccepted?.Invoke(this, stableCandidate);
    }

    public void SetActive(bool isActive)
    {
        m_IsActive = isActive;
        if (!isActive)
            ClearAcceptedPacket();

        SetVisualState(isActive ? VisualState.Idle : VisualState.Inactive);
        UpdateValueText();
    }

    public void SetExpectedPacketRole(DataPacketRole expectedPacketRole)
    {
        if (m_ExpectedPacketRole == expectedPacketRole)
            return;

        m_ExpectedPacketRole = expectedPacketRole;

        if (AcceptedPacket != null && AcceptedPacket.PacketRole != m_ExpectedPacketRole)
            ClearAcceptedPacket();

        ApplyVisualState();
        UpdateValueText();
    }

    public void ResetScanner()
    {
        m_PacketsInZone.Clear();
        ClearAcceptedPacket();
        SetVisualState(m_IsActive ? VisualState.Idle : VisualState.Inactive);
        UpdateValueText();
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

    void OnCollisionEnter(Collision collision)
    {
        NotifyPacketEntered(collision.collider.GetComponentInParent<DataPacketToken>());
    }

    void OnCollisionStay(Collision collision)
    {
        NotifyPacketEntered(collision.collider.GetComponentInParent<DataPacketToken>());
    }

    void OnCollisionExit(Collision collision)
    {
        NotifyPacketExited(collision.collider.GetComponentInParent<DataPacketToken>());
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
        m_BodyTransform ??= transform;

        if (m_BodyRenderer == null)
            m_BodyRenderer = GetComponent<Renderer>();

        if (m_LabelText == null || m_ValueText == null)
        {
            foreach (var textMesh in GetComponentsInChildren<TMP_Text>(true))
            {
                if (textMesh == null)
                    continue;

                if (m_LabelText == null)
                {
                    m_LabelText = textMesh;
                    continue;
                }

                if (m_ValueText == null && textMesh != m_LabelText)
                    m_ValueText = textMesh;
            }
        }

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
        var labelColor = Color.white;
        var pressed = false;

        switch (m_VisualState)
        {
            case VisualState.Inactive:
                bodyColor = m_InactiveColor;
                labelColor = new Color(0.82f, 0.85f, 0.9f, 0.8f);
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

        if (m_LabelText != null)
        {
            m_LabelText.text = GetRoleDisplayName(m_ExpectedPacketRole);
            m_LabelText.color = labelColor;
        }

        if (m_ValueText != null)
            m_ValueText.color = labelColor;

        if (m_BodyTransform != null)
        {
            var targetLocalPosition = m_BodyRestLocalPosition;
            if (pressed)
                targetLocalPosition.y += m_PressedOffsetY;

            m_BodyTransform.localPosition = targetLocalPosition;
        }
    }

    void UpdateValueText()
    {
        if (m_ValueText == null)
            return;

        m_ValueText.text = AcceptedPacket != null ? AcceptedPacket.Value.ToString() : "0";
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

    static string GetRoleDisplayName(DataPacketRole packetRole)
    {
        return packetRole switch
        {
            DataPacketRole.ReadData1 => "Input 1",
            DataPacketRole.ReadData2 => "Input 2",
            DataPacketRole.Immediate => "Immediate",
            DataPacketRole.AluResult => "ALU Result",
            DataPacketRole.MemoryData => "Memory Data",
            _ => "Input",
        };
    }
}

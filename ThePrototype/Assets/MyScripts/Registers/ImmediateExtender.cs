using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Small authored station that accepts an Immediate datapacket and marks it as
/// sign-extended after it remains stable in the zone long enough.
/// The packet stays grabbable so the learner can carry the same packet into
/// ALU or memory afterwards.
/// </summary>
[DisallowMultipleComponent]
public class ImmediateExtender : PedestalScannerBase
{
    [SerializeField]
    bool m_AlwaysActive = true;

    [SerializeField]
    TMP_Text m_LabelText;

    [SerializeField]
    TMP_Text m_ValueText;

    [SerializeField]
    BoxCollider m_ScanZone;

    [SerializeField]
    Vector3 m_ScanZonePadding = new(0.12f, 0.18f, 0.12f);

    [SerializeField]
    float m_ScanZoneSurfaceInset = 0.02f;

    [SerializeField]
    float m_SupportColliderHeightPadding = 0.01f;

    readonly HashSet<DataPacketToken> m_PacketsInZone = new();

    int m_LastResolvedValue;

    public DataPacketToken AcceptedPacket { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        ConfigureSupportCollider();
        ConfigureScanZone();
        BindZoneHelper();
        EnsureStaticPedestalPhysics();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        BindZoneHelper();

        if (m_AlwaysActive)
            SetStepActive(true);
    }

    protected override void OnValidate()
    {
        base.OnValidate();
        RestoreEditorRestPose();
        ConfigureSupportCollider();
        ConfigureScanZone();
        EnsureStaticPedestalPhysics();
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

        if (CurrentCandidateAs<DataPacketToken>() == dataPacketToken)
            OnCandidateLost();

        if (AcceptedPacket == dataPacketToken)
        {
            AcceptedPacket = null;
            m_LastResolvedValue = 0;
            UpdateValueText();

            if (m_AlwaysActive)
                ResetScanner();
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

    protected override void CacheVisualReferences()
    {
        base.CacheVisualReferences();

        var labelTransform = FindChildTransform("Label");
        m_LabelText = labelTransform != null
            ? labelTransform.GetComponent<TMP_Text>() ?? labelTransform.GetComponentInChildren<TMP_Text>(true)
            : GetComponentInChildren<TMP_Text>(true);

        if (m_ValueText == null)
        {
            foreach (var textMesh in GetComponentsInChildren<TMP_Text>(true))
            {
                if (textMesh == null || textMesh == m_LabelText)
                    continue;

                m_ValueText = textMesh;
                break;
            }
        }

        var scanZoneTransform = FindChildTransform("Scan Zone");
        m_ScanZone = scanZoneTransform != null ? scanZoneTransform.GetComponent<BoxCollider>() : null;
    }

    protected override bool IsImmediateMismatch(Component candidate)
    {
        return candidate is not DataPacketToken dataPacketToken ||
               dataPacketToken.PacketRole != DataPacketRole.Immediate;
    }

    protected override void ApplyAuxiliaryVisuals(ScannerVisualState visualState)
    {
        var labelColor = visualState == ScannerVisualState.Inactive
            ? new Color(0.82f, 0.85f, 0.9f, 0.8f)
            : Color.white;

        if (m_LabelText != null)
            m_LabelText.color = labelColor;

        if (m_ValueText != null)
            m_ValueText.color = labelColor;

        UpdateValueText();
    }

    protected override void OnCandidateLost()
    {
        m_LastResolvedValue = 0;
        UpdateValueText();
    }

    protected override void AfterFailureReset()
    {
        m_LastResolvedValue = 0;
        UpdateValueText();
    }

    protected override void OnStepActiveChanged(bool isActive)
    {
        if (!isActive)
            AcceptedPacket = null;

        if (!isActive)
            m_LastResolvedValue = 0;

        UpdateValueText();
    }

    protected override void HandleScannerReset()
    {
        m_PacketsInZone.Clear();
        AcceptedPacket = null;
        m_LastResolvedValue = 0;
        UpdateValueText();
    }

    protected override void HandleStableCandidate(Component candidate)
    {
        var dataPacketToken = candidate as DataPacketToken;
        if (dataPacketToken == null)
            return;

        var signExtendedValue = SignExtend16Bit(dataPacketToken.Value);
        dataPacketToken.MarkSignExtended(signExtendedValue);
        AcceptedPacket = dataPacketToken;
        m_LastResolvedValue = signExtendedValue;
        MarkSuccess();
    }

    void UpdateValueText()
    {
        if (m_ValueText == null)
            return;

        m_ValueText.text = IsLatchedSuccessful
            ? m_LastResolvedValue.ToString()
            : "0";
    }

    static int SignExtend16Bit(int rawImmediateValue)
    {
        return (short)(rawImmediateValue & 0xFFFF);
    }

    void ConfigureSupportCollider()
    {
        var supportCollider = GetComponent<BoxCollider>();
        if (supportCollider == null || BaseRenderer == null || BodyRenderer == null)
            return;

        var baseBounds = GetRendererBoundsInRootSpace(BaseRenderer);
        var bodyBounds = GetRendererBoundsInRootSpace(BodyRenderer);
        var pressedBodyCenterY = bodyBounds.center.y + PressedOffsetY;
        var supportHeight = Mathf.Max(0.02f, bodyBounds.size.y + m_SupportColliderHeightPadding);

        supportCollider.center = new Vector3(baseBounds.center.x, pressedBodyCenterY, baseBounds.center.z);
        supportCollider.size = new Vector3(baseBounds.size.x, supportHeight, baseBounds.size.z);
        supportCollider.isTrigger = false;
    }

    void ConfigureScanZone()
    {
        if (m_ScanZone == null || BodyRenderer == null)
            return;

        var bodyBounds = GetRendererBoundsInRootSpace(BodyRenderer);
        var pressedBodyTopY = bodyBounds.max.y + PressedOffsetY;
        var scanHeight = Mathf.Max(0.12f, bodyBounds.size.y + m_ScanZonePadding.y);
        var scanBottomY = pressedBodyTopY - m_ScanZoneSurfaceInset;
        var scanCenterY = scanBottomY + scanHeight * 0.5f;

        m_ScanZone.isTrigger = true;
        m_ScanZone.center = new Vector3(
            bodyBounds.center.x,
            scanCenterY,
            bodyBounds.center.z);
        m_ScanZone.size = new Vector3(
            bodyBounds.size.x + m_ScanZonePadding.x,
            scanHeight,
            bodyBounds.size.z + m_ScanZonePadding.z);
    }

    void BindZoneHelper()
    {
        if (m_ScanZone == null)
            return;

        var helper = m_ScanZone.GetComponent<ImmediateExtenderZone>();
        if (helper == null)
            helper = m_ScanZone.gameObject.AddComponent<ImmediateExtenderZone>();

        helper.Bind(this);
    }

    void EnsureStaticPedestalPhysics()
    {
        var rigidbody = GetComponent<Rigidbody>();
        if (rigidbody == null)
            return;

        rigidbody.isKinematic = true;
        rigidbody.useGravity = false;
        rigidbody.constraints = RigidbodyConstraints.FreezeAll;
        rigidbody.linearVelocity = Vector3.zero;
        rigidbody.angularVelocity = Vector3.zero;
        rigidbody.Sleep();
    }

    void RestoreEditorRestPose()
    {
        if (Application.isPlaying || BaseRenderer == null || BodyRenderer == null || BodyTransform == null)
            return;

        var baseBounds = GetRendererBoundsInRootSpace(BaseRenderer);
        var bodyBounds = GetRendererBoundsInRootSpace(BodyRenderer);
        var desiredBodyBottom = baseBounds.max.y + 0.01f;
        var correction = desiredBodyBottom - bodyBounds.min.y;

        if (correction > 0.002f)
            BodyTransform.localPosition += new Vector3(0f, correction, 0f);
    }

    Bounds GetRendererBoundsInRootSpace(Renderer targetRenderer)
    {
        var localBounds = targetRenderer.localBounds;
        var worldToLocal = transform.worldToLocalMatrix;

        var min = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
        var max = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);

        for (var x = -1; x <= 1; x += 2)
        {
            for (var y = -1; y <= 1; y += 2)
            {
                for (var z = -1; z <= 1; z += 2)
                {
                    var localCorner = localBounds.center + Vector3.Scale(localBounds.extents, new Vector3(x, y, z));
                    var worldCorner = targetRenderer.transform.TransformPoint(localCorner);
                    var rootLocalCorner = worldToLocal.MultiplyPoint3x4(worldCorner);
                    min = Vector3.Min(min, rootLocalCorner);
                    max = Vector3.Max(max, rootLocalCorner);
                }
            }
        }

        var bounds = new Bounds();
        bounds.SetMinMax(min, max);
        return bounds;
    }
}

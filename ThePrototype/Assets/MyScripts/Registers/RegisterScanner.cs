using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Pedestal-style scanner used during lesson phases that need a specific
/// register role to be physically placed and held in a zone for validation.
///
/// The scanner itself only owns local presentation:
/// - tracking which register token is resting in the scan zone
/// - waiting for the 1-second scan window
/// - pressing the body down while occupied
/// - swapping authored materials for each scanner state
///
/// Lesson correctness still lives in <see cref="CpuLessonFlow"/>.
/// </summary>
[DisallowMultipleComponent]
public class RegisterScanner : PedestalScannerBase
{
    [Header("Role")]

    [SerializeField]
    InstructionRegisterRole m_RegisterRole = InstructionRegisterRole.None;

    [SerializeField]
    RegisterBank m_OwningBank;

    [SerializeField]
    TMP_Text m_LabelText;

    [SerializeField]
    TMP_Text m_ValueText;

    [SerializeField]
    BoxCollider m_ScanZone;

    [Header("Data Packet Output")]

    [SerializeField]
    DataPacketToken m_DataPacketPrefab;

    [SerializeField]
    Transform m_DataPacketSpawnAnchor;

    [SerializeField]
    DataPacketRole m_OutputPacketRole = DataPacketRole.ReadData1;

    [Header("Scan Tuning")]

    [SerializeField]
    Vector3 m_ScanZonePadding = new(0.12f, 0.18f, 0.12f);

    [SerializeField]
    float m_ScanZoneSurfaceInset = 0.02f;

    [SerializeField]
    float m_SupportColliderHeightPadding = 0.01f;

    readonly System.Collections.Generic.HashSet<RegisterToken> m_TokensInZone = new();

    DataPacketToken m_SpawnedPacket;
    int m_LastResolvedValue;

    public InstructionRegisterRole RegisterRole => m_RegisterRole;
    public DataPacketToken SpawnedPacket => m_SpawnedPacket;
    public Transform DataPacketSpawnAnchor => m_DataPacketSpawnAnchor;

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
    }

    protected override void OnValidate()
    {
        base.OnValidate();
        RestoreEditorRestPose();
        ConfigureSupportCollider();
        ConfigureScanZone();
        EnsureStaticPedestalPhysics();
    }

    /// <summary>
    /// Called by the child trigger helper when a token enters the scan zone.
    /// </summary>
    public void NotifyTokenEntered(RegisterToken registerToken)
    {
        if (registerToken != null)
            m_TokensInZone.Add(registerToken);
    }

    /// <summary>
    /// Called by the child trigger helper when a token leaves the scan zone.
    /// </summary>
    public void NotifyTokenExited(RegisterToken registerToken)
    {
        if (registerToken == null)
            return;

        m_TokensInZone.Remove(registerToken);
        if (CurrentCandidateAs<RegisterToken>() == registerToken)
        {
            OnCandidateLost();
        }

    }

    protected override Component GetStableCandidate()
    {
        m_TokensInZone.RemoveWhere(token => token == null);
        foreach (var registerToken in m_TokensInZone)
        {
            if (registerToken == null || registerToken.IsGrabbed)
                continue;

            return registerToken;
        }

        return null;
    }

    protected override void CacheVisualReferences()
    {
        base.CacheVisualReferences();

        // Always rebind authored child references by name. Duplicating these
        // scanners in the scene can leave serialized references pointing at the
        // original object's children, so relying on "only if null" is brittle.
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

        if (!m_OwningBank || m_OwningBank.gameObject.scene != gameObject.scene)
        {
            foreach (var registerBank in Resources.FindObjectsOfTypeAll<RegisterBank>())
            {
                if (registerBank == null)
                    continue;

                if (!registerBank.gameObject.scene.IsValid() || !registerBank.gameObject.scene.isLoaded)
                    continue;

                if (registerBank.gameObject.scene != gameObject.scene)
                    continue;

                m_OwningBank = registerBank;
                break;
            }
        }
    }

    void ConfigureSupportCollider()
    {
        var supportCollider = GetComponent<BoxCollider>();
        if (supportCollider == null || BaseRenderer == null || BodyRenderer == null)
            return;

        // The solid collider matches the scanner footprint, not the trigger.
        // That keeps dropped tokens resting on the pedestal instead of sinking
        // into the moving body mesh.
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

        // The trigger extends a little above the pressed surface so packets can
        // settle naturally before the timer starts.
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

        var helper = m_ScanZone.GetComponent<RegisterScannerZone>();
        if (helper == null)
            helper = m_ScanZone.gameObject.AddComponent<RegisterScannerZone>();

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

    void RememberBodyPose()
    {
        // handled by base
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
    }

    protected override void AfterFailureReset()
    {
        m_LastResolvedValue = 0;
    }

    protected override void OnStepActiveChanged(bool isActive)
    {
        m_LastResolvedValue = 0;
        UpdateValueText();
    }

    protected override void HandleScannerReset()
    {
        m_TokensInZone.Clear();
        ClearSpawnedPacket();
        m_LastResolvedValue = 0;
        UpdateValueText();
    }

    protected override void HandleStableCandidate(Component candidate)
    {
        var registerToken = candidate as RegisterToken;
        if (registerToken == null)
            return;

        // Reaching the scan duration only means "candidate is stable".
        // The lesson flow still decides whether the scanned register is correct.
        m_OwningBank?.NotifyRegisterScanned(m_RegisterRole, registerToken);
    }

    void UpdateValueText()
    {
        if (m_ValueText == null)
            return;

        m_ValueText.text = IsLatchedSuccessful
            ? m_LastResolvedValue.ToString()
            : "0";
    }

    protected override void HandleSuccessLatched()
    {
        var currentCandidate = CurrentCandidateAs<RegisterToken>();
        m_LastResolvedValue = currentCandidate != null ? currentCandidate.RegisterValue : 0;

        if (ShouldSpawnDataPacket())
            SpawnDataPacketFromCurrentCandidate();
    }

    /// <summary>
    /// Retargets the packet role emitted by this scanner without requiring a
    /// separate prefab for each datapath use-case.
    /// </summary>
    public void SetOutputPacketRole(DataPacketRole outputPacketRole)
    {
        m_OutputPacketRole = outputPacketRole;
    }

    /// <summary>
    /// Spawns a packet from this scanner's authored output anchor using the
    /// currently configured packet role.
    /// </summary>
    public void SpawnConfiguredPacket(string sourceRegisterId, string sourceDisplayLabel, int value)
    {
        ClearSpawnedPacket();

        if (m_DataPacketPrefab == null || m_DataPacketSpawnAnchor == null)
            return;

        var spawnedPacket = Instantiate(
            m_DataPacketPrefab,
            m_DataPacketSpawnAnchor.position,
            m_DataPacketSpawnAnchor.rotation);
        spawnedPacket.Configure(
            m_OutputPacketRole,
            sourceRegisterId,
            sourceDisplayLabel,
            value);

        m_SpawnedPacket = spawnedPacket;
    }

    void SpawnDataPacketFromCurrentCandidate()
    {
        var sourceToken = CurrentCandidateAs<RegisterToken>() ?? GetStableCandidate() as RegisterToken;
        if (sourceToken == null)
            return;

        // Packet data is copied at scan success time so later register resets
        // or value changes do not mutate an already emitted datapath packet.
        SpawnConfiguredPacket(
            sourceToken.RegisterId,
            sourceToken.DisplayLabel,
            sourceToken.RegisterValue);
    }

    bool ShouldSpawnDataPacket()
    {
        if (m_RegisterRole == InstructionRegisterRole.Rd)
            return false;

        return m_OutputPacketRole == DataPacketRole.ReadData1 ||
               m_OutputPacketRole == DataPacketRole.ReadData2 ||
               m_OutputPacketRole == DataPacketRole.Immediate;
    }

    void ClearSpawnedPacket()
    {
        if (m_SpawnedPacket == null)
            return;

        if (Application.isPlaying)
            Destroy(m_SpawnedPacket.gameObject);
        else
            DestroyImmediate(m_SpawnedPacket.gameObject);

        m_SpawnedPacket = null;
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

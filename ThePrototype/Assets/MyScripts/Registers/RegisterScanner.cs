using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// Pedestal-style scanner used during lesson phases that need a specific
/// register role to be physically placed and held in a zone for validation.
///
/// The scanner itself only owns local presentation:
/// - tracking which register token is resting in the scan zone
/// - waiting for the 1-second scan window
/// - pressing the body down while occupied
/// - showing idle / success / failure colors
///
/// Lesson correctness still lives in <see cref="CpuLessonFlow"/>.
/// </summary>
[DisallowMultipleComponent]
public class RegisterScanner : MonoBehaviour
{
    enum ScannerVisualState
    {
        Inactive,
        Idle,
        Occupied,
        Success,
        Failure,
    }

    [Header("Role")]

    [SerializeField]
    InstructionRegisterRole m_RegisterRole = InstructionRegisterRole.None;

    [SerializeField]
    RegisterBank m_OwningBank;

    [Header("Scene References")]

    [SerializeField]
    Transform m_BodyTransform;

    [SerializeField]
    Renderer m_BaseRenderer;

    [SerializeField]
    Renderer m_BodyRenderer;

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
    Vector3 m_DataPacketSpawnOffset = new(0f, 0.1f, 0.45f);

    [SerializeField]
    Transform m_DataPacketSpawnAnchor;

    [SerializeField]
    DataPacketRole m_OutputPacketRole = DataPacketRole.ReadData1;

    [Header("Materials")]

    [SerializeField]
    Material m_BaseMaterialTemplate;

    [SerializeField]
    Material m_BodyMaterialTemplate;

    [Header("Scan Tuning")]

    [SerializeField]
    float m_ScanDurationSeconds = 2f;

    [SerializeField]
    float m_PressedOffsetY = -0.03f;

    [SerializeField]
    Vector3 m_ScanZonePadding = new(0.12f, 0.18f, 0.12f);

    [SerializeField]
    float m_ScanZoneSurfaceInset = 0.02f;

    [SerializeField]
    float m_SupportColliderHeightPadding = 0.01f;

    [Header("Colors")]

    [SerializeField]
    Color m_InactiveBodyColor = new(0.23f, 0.28f, 0.34f, 1f);

    [SerializeField]
    Color m_IdleBodyColor = new(0.37f, 0.54f, 0.72f, 1f);

    [SerializeField]
    Color m_OccupiedBodyColor = new(0.93f, 0.72f, 0.25f, 1f);

    [SerializeField]
    Color m_SuccessBodyColor = new(0.3f, 0.76f, 0.43f, 1f);

    [SerializeField]
    Color m_FailureBodyColor = new(0.86f, 0.24f, 0.24f, 1f);

    [SerializeField]
    Color m_BaseColor = new(0.14f, 0.15f, 0.19f, 1f);

    static readonly int k_BaseColorId = Shader.PropertyToID("_BaseColor");
    static readonly int k_ColorId = Shader.PropertyToID("_Color");

    readonly System.Collections.Generic.HashSet<RegisterToken> m_TokensInZone = new();

    Vector3 m_BodyRestLocalPosition;
    RegisterToken m_CurrentCandidate;
    Coroutine m_FailureRoutine;
    float m_CurrentScanTime;
    bool m_IsStepActive;
    bool m_IsAwaitingValidation;
    bool m_IsLatchedSuccessful;
    ScannerVisualState m_VisualState = ScannerVisualState.Inactive;
    DataPacketToken m_SpawnedPacket;

    public InstructionRegisterRole RegisterRole => m_RegisterRole;
    public DataPacketToken SpawnedPacket => m_SpawnedPacket;

    void Awake()
    {
        CacheReferences();
        ApplyTemplates();
        RememberBodyPose();
        ConfigureSupportCollider();
        ConfigureScanZone();
        BindZoneHelper();
        EnsureStaticPedestalPhysics();
        ApplyCurrentVisualState();
    }

    void OnEnable()
    {
        CacheReferences();
        BindZoneHelper();
    }

    void OnValidate()
    {
        CacheReferences();
        ApplyTemplates();
        ResetEditorState();
        RestoreEditorRestPose();
        RememberBodyPose();
        ConfigureSupportCollider();
        ConfigureScanZone();
        EnsureStaticPedestalPhysics();
        ApplyCurrentVisualState();
    }

    void Update()
    {
        UpdateValueText();

        if (!m_IsStepActive || m_IsAwaitingValidation || m_IsLatchedSuccessful)
            return;

        var candidate = GetStableCandidate();
        if (candidate == null)
        {
            m_CurrentCandidate = null;
            m_CurrentScanTime = 0f;
            if (m_VisualState != ScannerVisualState.Failure)
                SetVisualState(ScannerVisualState.Idle);
            return;
        }

        if (candidate != m_CurrentCandidate)
        {
            m_CurrentCandidate = candidate;
            m_CurrentScanTime = 0f;
        }

        m_CurrentScanTime += Time.deltaTime;
        SetVisualState(ScannerVisualState.Occupied);

        if (m_CurrentScanTime < m_ScanDurationSeconds)
            return;

        m_IsAwaitingValidation = true;
        m_CurrentScanTime = 0f;
        m_OwningBank?.NotifyRegisterScanned(m_RegisterRole, candidate);
    }

    /// <summary>
    /// Enables or disables this scanner for the active lesson step.
    /// Inactive scanners ignore tokens and fall back to a muted idle look.
    /// </summary>
    public void SetStepActive(bool isActive)
    {
        m_IsStepActive = isActive;
        m_IsAwaitingValidation = false;
        m_IsLatchedSuccessful = false;
        m_CurrentCandidate = null;
        m_CurrentScanTime = 0f;

        if (m_FailureRoutine != null)
        {
            StopCoroutine(m_FailureRoutine);
            m_FailureRoutine = null;
        }

        if (!isActive)
            ClearSpawnedPacket();

        SetVisualState(isActive ? ScannerVisualState.Idle : ScannerVisualState.Inactive);
        UpdateValueText();
    }

    /// <summary>
    /// Clears local progress and returns the scanner to its default state.
    /// </summary>
    public void ResetScanner()
    {
        m_TokensInZone.Clear();
        ClearSpawnedPacket();
        SetStepActive(false);
        UpdateValueText();
    }

    /// <summary>
    /// Latches the scanner green after the lesson accepts the placed register.
    /// </summary>
    public void MarkSuccess()
    {
        m_IsAwaitingValidation = false;
        m_IsLatchedSuccessful = true;
        SpawnDataPacketFromCurrentCandidate();
        SetVisualState(ScannerVisualState.Success);
    }

    /// <summary>
    /// Briefly shows a failure state, then returns to idle or occupied.
    /// </summary>
    public void FlashFailure()
    {
        if (m_FailureRoutine != null)
            StopCoroutine(m_FailureRoutine);

        m_FailureRoutine = StartCoroutine(FlashFailureRoutine());
    }

    /// <summary>
    /// Called by the child trigger helper when a token enters the scan zone.
    /// </summary>
    public void NotifyTokenEntered(RegisterToken registerToken)
    {
        if (registerToken != null)
            m_TokensInZone.Add(registerToken);

        UpdateValueText();
    }

    /// <summary>
    /// Called by the child trigger helper when a token leaves the scan zone.
    /// </summary>
    public void NotifyTokenExited(RegisterToken registerToken)
    {
        if (registerToken == null)
            return;

        m_TokensInZone.Remove(registerToken);
        if (m_CurrentCandidate == registerToken)
        {
            m_CurrentCandidate = null;
            m_CurrentScanTime = 0f;
            m_IsAwaitingValidation = false;

            if (!m_IsLatchedSuccessful && m_IsStepActive && m_FailureRoutine == null)
                SetVisualState(ScannerVisualState.Idle);
        }

        UpdateValueText();
    }

    IEnumerator FlashFailureRoutine()
    {
        m_IsAwaitingValidation = true;
        SetVisualState(ScannerVisualState.Failure);
        yield return new WaitForSeconds(0.35f);

        m_IsAwaitingValidation = false;
        m_FailureRoutine = null;
        m_CurrentCandidate = null;
        m_CurrentScanTime = 0f;

        if (!m_IsStepActive)
            SetVisualState(ScannerVisualState.Inactive);
        else if (GetStableCandidate() != null)
            SetVisualState(ScannerVisualState.Occupied);
        else
            SetVisualState(ScannerVisualState.Idle);
    }

    RegisterToken GetStableCandidate()
    {
        RegisterToken stableCandidate = null;

        m_TokensInZone.RemoveWhere(token => token == null);
        foreach (var registerToken in m_TokensInZone)
        {
            if (registerToken == null || registerToken.IsGrabbed)
                continue;

            stableCandidate = registerToken;
            break;
        }

        return stableCandidate;
    }

    void CacheReferences()
    {
        // Always rebind authored child references by name. Duplicating these
        // scanners in the scene can leave serialized references pointing at the
        // original object's children, so relying on "only if null" is brittle.
        m_BodyTransform = FindChildTransform("Body");

        var baseTransform = FindChildTransform("Base");
        m_BaseRenderer = baseTransform != null ? baseTransform.GetComponent<Renderer>() : null;
        m_BodyRenderer = m_BodyTransform != null ? m_BodyTransform.GetComponent<Renderer>() : null;

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

    Transform FindChildTransform(string childName)
    {
        foreach (var childTransform in GetComponentsInChildren<Transform>(true))
        {
            if (childTransform == null || childTransform == transform)
                continue;

            if (childTransform.name.Equals(childName, System.StringComparison.Ordinal))
                return childTransform;
        }

        return null;
    }

    void ApplyTemplates()
    {
        CacheReferences();

        if (m_BaseRenderer && m_BaseMaterialTemplate != null)
            m_BaseRenderer.sharedMaterial = m_BaseMaterialTemplate;

        if (m_BodyRenderer && m_BodyMaterialTemplate != null)
            m_BodyRenderer.sharedMaterial = m_BodyMaterialTemplate;
    }

    void ConfigureSupportCollider()
    {
        var supportCollider = GetComponent<BoxCollider>();
        if (supportCollider == null || !m_BaseRenderer || !m_BodyRenderer)
            return;

        var baseBounds = GetRendererBoundsInRootSpace(m_BaseRenderer);
        var bodyBounds = GetRendererBoundsInRootSpace(m_BodyRenderer);
        var pressedBodyCenterY = bodyBounds.center.y + m_PressedOffsetY;
        var supportHeight = Mathf.Max(0.02f, bodyBounds.size.y + m_SupportColliderHeightPadding);

        supportCollider.center = new Vector3(baseBounds.center.x, pressedBodyCenterY, baseBounds.center.z);
        supportCollider.size = new Vector3(baseBounds.size.x, supportHeight, baseBounds.size.z);
        supportCollider.isTrigger = false;
    }

    void ConfigureScanZone()
    {
        if (m_ScanZone == null || !m_BodyRenderer)
            return;

        var bodyBounds = GetRendererBoundsInRootSpace(m_BodyRenderer);
        var pressedBodyTopY = bodyBounds.max.y + m_PressedOffsetY;
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
        if (m_BodyTransform != null)
            m_BodyRestLocalPosition = m_BodyTransform.localPosition;
    }

    void ResetEditorState()
    {
        if (Application.isPlaying)
            return;

        m_TokensInZone.Clear();
        m_CurrentCandidate = null;
        m_CurrentScanTime = 0f;
        m_IsStepActive = false;
        m_IsAwaitingValidation = false;
        m_IsLatchedSuccessful = false;
        m_FailureRoutine = null;
        m_VisualState = ScannerVisualState.Inactive;
    }

    void RestoreEditorRestPose()
    {
        if (Application.isPlaying || !m_BaseRenderer || !m_BodyRenderer || m_BodyTransform == null)
            return;

        var baseBounds = GetRendererBoundsInRootSpace(m_BaseRenderer);
        var bodyBounds = GetRendererBoundsInRootSpace(m_BodyRenderer);
        var desiredBodyBottom = baseBounds.max.y + 0.01f;
        var correction = desiredBodyBottom - bodyBounds.min.y;

        if (correction > 0.002f)
            m_BodyTransform.localPosition += new Vector3(0f, correction, 0f);
    }

    void SetVisualState(ScannerVisualState newState)
    {
        m_VisualState = newState;
        ApplyCurrentVisualState();
    }

    void ApplyCurrentVisualState()
    {
        CacheReferences();

        var bodyColor = m_IdleBodyColor;
        var labelColor = Color.white;
        var pressed = false;

        switch (m_VisualState)
        {
            case ScannerVisualState.Inactive:
                bodyColor = m_InactiveBodyColor;
                labelColor = new Color(0.82f, 0.85f, 0.9f, 0.8f);
                break;

            case ScannerVisualState.Idle:
                bodyColor = m_IdleBodyColor;
                break;

            case ScannerVisualState.Occupied:
                bodyColor = m_OccupiedBodyColor;
                pressed = true;
                break;

            case ScannerVisualState.Success:
                bodyColor = m_SuccessBodyColor;
                pressed = true;
                break;

            case ScannerVisualState.Failure:
                bodyColor = m_FailureBodyColor;
                pressed = true;
                break;
        }

        ApplyRendererColor(m_BaseRenderer, m_BaseColor);
        ApplyRendererColor(m_BodyRenderer, bodyColor);

        if (m_LabelText != null)
            m_LabelText.color = labelColor;

        if (m_ValueText != null)
            m_ValueText.color = labelColor;

        if (m_BodyTransform != null)
        {
            var targetLocalPosition = m_BodyRestLocalPosition;
            if (pressed)
                targetLocalPosition.y += m_PressedOffsetY;

            m_BodyTransform.localPosition = targetLocalPosition;
        }

        UpdateValueText();
    }

    void UpdateValueText()
    {
        if (m_ValueText == null)
            return;

        var stableCandidate = GetStableCandidate();
        m_ValueText.text = stableCandidate != null
            ? stableCandidate.RegisterValue.ToString()
            : "0";
    }

    void SpawnDataPacketFromCurrentCandidate()
    {
        ClearSpawnedPacket();

        if (m_DataPacketPrefab == null)
            return;

        var sourceToken = m_CurrentCandidate != null ? m_CurrentCandidate : GetStableCandidate();
        if (sourceToken == null)
            return;

        var spawnTransform = m_DataPacketSpawnAnchor != null ? m_DataPacketSpawnAnchor : transform;
        var spawnPosition = spawnTransform.TransformPoint(m_DataPacketSpawnOffset);
        var spawnedPacket = Instantiate(m_DataPacketPrefab, spawnPosition, spawnTransform.rotation);
        spawnedPacket.Configure(
            m_OutputPacketRole,
            sourceToken.RegisterId,
            sourceToken.DisplayLabel,
            sourceToken.RegisterValue);

        m_SpawnedPacket = spawnedPacket;
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

    static void ApplyRendererColor(Renderer targetRenderer, Color color)
    {
        if (!targetRenderer)
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

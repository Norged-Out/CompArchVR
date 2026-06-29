using System.Collections;
using UnityEngine;

/// <summary>
/// Shared scanner mechanics for pedestal-style lesson objects.
/// This base owns:
/// - stable-in-zone timing
/// - push-down body motion
/// - inactive / idle / occupied / success / failure material swapping
///
/// Concrete scanners decide what counts as a candidate, how candidates are
/// validated, and what happens when a stable candidate is accepted.
/// </summary>
[DisallowMultipleComponent]
public abstract class PedestalScannerBase : MonoBehaviour
{
    protected enum ScannerVisualState
    {
        Inactive,
        Idle,
        Occupied,
        Success,
        Failure,
    }

    [Header("Scene References")]
    [SerializeField]
    Transform m_BodyTransform;

    [SerializeField]
    Renderer m_BaseRenderer;

    [SerializeField]
    Renderer m_BodyRenderer;

    [Header("Materials")]
    [SerializeField]
    Material m_InactiveMaterial;

    [SerializeField]
    Material m_IdleMaterial;

    [SerializeField]
    Material m_OccupiedMaterial;

    [SerializeField]
    Material m_SuccessMaterial;

    [SerializeField]
    Material m_FailureMaterial;

    [Header("Scan Tuning")]
    [SerializeField]
    float m_ScanDurationSeconds = 2f;

    [SerializeField]
    float m_PressedOffsetY = -0.03f;

    Vector3 m_BodyRestLocalPosition;
    Component m_CurrentCandidate;
    Coroutine m_FailureRoutine;
    float m_CurrentScanTime;
    bool m_IsStepActive;
    bool m_IsAwaitingValidation;
    bool m_IsLatchedSuccessful;
    ScannerVisualState m_VisualState = ScannerVisualState.Inactive;

    protected Transform BodyTransform => m_BodyTransform;
    protected Renderer BaseRenderer => m_BaseRenderer;
    protected Renderer BodyRenderer => m_BodyRenderer;
    protected bool IsStepActive => m_IsStepActive;
    protected bool IsLatchedSuccessful => m_IsLatchedSuccessful;
    protected virtual float RequiredStableSeconds => m_ScanDurationSeconds;
    protected virtual float PressedOffsetY => m_PressedOffsetY;
    protected T CurrentCandidateAs<T>() where T : Component => m_CurrentCandidate as T;

    protected virtual void Awake()
    {
        CacheVisualReferences();
        ApplyDefaultMaterials();
        RememberBodyPose();
        ApplyCurrentVisualState();
    }

    protected virtual void OnEnable()
    {
        CacheVisualReferences();
    }

    protected virtual void OnValidate()
    {
        CacheVisualReferences();
        ApplyDefaultMaterials();
        ResetEditorState();
        RememberBodyPose();
        ApplyCurrentVisualState();
    }

    protected virtual void Update()
    {
        if (!m_IsStepActive || m_IsAwaitingValidation || m_IsLatchedSuccessful)
            return;

        var candidate = GetStableCandidate();
        if (candidate == null)
        {
            m_CurrentCandidate = null;
            m_CurrentScanTime = 0f;
            if (m_VisualState != ScannerVisualState.Failure)
                SetVisualState(ScannerVisualState.Idle);
            OnCandidateLost();
            return;
        }

        if (IsImmediateMismatch(candidate))
        {
            m_CurrentCandidate = null;
            m_CurrentScanTime = 0f;
            SetVisualState(ScannerVisualState.Failure);
            OnImmediateMismatch(candidate);
            return;
        }

        if (candidate != m_CurrentCandidate)
        {
            m_CurrentCandidate = candidate;
            m_CurrentScanTime = 0f;
        }

        m_CurrentScanTime += Time.deltaTime;
        SetVisualState(ScannerVisualState.Occupied);

        if (m_CurrentScanTime < RequiredStableSeconds)
            return;

        m_IsAwaitingValidation = true;
        m_CurrentScanTime = 0f;
        HandleStableCandidate(candidate);
    }

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

        OnStepActiveChanged(isActive);
        SetVisualState(isActive ? ScannerVisualState.Idle : ScannerVisualState.Inactive);
    }

    public void ResetScanner()
    {
        m_CurrentCandidate = null;
        m_CurrentScanTime = 0f;
        m_IsAwaitingValidation = false;
        m_IsLatchedSuccessful = false;

        if (m_FailureRoutine != null)
        {
            StopCoroutine(m_FailureRoutine);
            m_FailureRoutine = null;
        }

        HandleScannerReset();
        SetVisualState(m_IsStepActive ? ScannerVisualState.Idle : ScannerVisualState.Inactive);
    }

    public void MarkSuccess()
    {
        m_IsAwaitingValidation = false;
        m_IsLatchedSuccessful = true;
        HandleSuccessLatched();
        SetVisualState(ScannerVisualState.Success);
    }

    public void FlashFailure()
    {
        if (m_FailureRoutine != null)
            StopCoroutine(m_FailureRoutine);

        m_FailureRoutine = StartCoroutine(FlashFailureRoutine());
    }

    protected virtual void CacheVisualReferences()
    {
        m_BodyTransform ??= FindChildTransform("Body");

        if (m_BaseRenderer == null)
        {
            var baseTransform = FindChildTransform("Base");
            if (baseTransform != null)
                m_BaseRenderer = baseTransform.GetComponent<Renderer>();
        }

        if (m_BodyRenderer == null && m_BodyTransform != null)
            m_BodyRenderer = m_BodyTransform.GetComponent<Renderer>();
    }

    protected Transform FindChildTransform(string childName)
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

    protected void SetVisualState(ScannerVisualState visualState)
    {
        m_VisualState = visualState;
        ApplyCurrentVisualState();
    }

    void ApplyDefaultMaterials()
    {
        if (m_BaseRenderer != null && m_InactiveMaterial != null)
            m_BaseRenderer.sharedMaterial = m_InactiveMaterial;

        if (m_BodyRenderer != null && m_InactiveMaterial != null)
            m_BodyRenderer.sharedMaterial = m_InactiveMaterial;
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

        m_CurrentCandidate = null;
        m_CurrentScanTime = 0f;
        m_IsStepActive = false;
        m_IsAwaitingValidation = false;
        m_IsLatchedSuccessful = false;
        m_FailureRoutine = null;
        m_VisualState = ScannerVisualState.Inactive;
    }

    void ApplyCurrentVisualState()
    {
        var bodyMaterial = m_IdleMaterial != null ? m_IdleMaterial : m_InactiveMaterial;
        var pressed = false;

        switch (m_VisualState)
        {
            case ScannerVisualState.Inactive:
                bodyMaterial = m_InactiveMaterial;
                break;
            case ScannerVisualState.Idle:
                bodyMaterial = m_IdleMaterial != null ? m_IdleMaterial : m_InactiveMaterial;
                break;
            case ScannerVisualState.Occupied:
                bodyMaterial = m_OccupiedMaterial != null ? m_OccupiedMaterial : m_IdleMaterial;
                pressed = true;
                break;
            case ScannerVisualState.Success:
                bodyMaterial = m_SuccessMaterial != null ? m_SuccessMaterial : m_IdleMaterial;
                pressed = true;
                break;
            case ScannerVisualState.Failure:
                bodyMaterial = m_FailureMaterial != null ? m_FailureMaterial : m_IdleMaterial;
                pressed = true;
                break;
        }

        if (m_BaseRenderer != null && m_InactiveMaterial != null)
            m_BaseRenderer.sharedMaterial = m_InactiveMaterial;

        if (m_BodyRenderer != null && bodyMaterial != null)
            m_BodyRenderer.sharedMaterial = bodyMaterial;

        if (m_BodyTransform != null)
        {
            var targetLocalPosition = m_BodyRestLocalPosition;
            if (pressed)
                targetLocalPosition.y += m_PressedOffsetY;

            m_BodyTransform.localPosition = targetLocalPosition;
        }

        ApplyAuxiliaryVisuals(m_VisualState);
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

        AfterFailureReset();

        if (!m_IsStepActive)
            SetVisualState(ScannerVisualState.Inactive);
        else if (GetStableCandidate() != null)
            SetVisualState(ScannerVisualState.Occupied);
        else
            SetVisualState(ScannerVisualState.Idle);
    }

    protected virtual void ApplyAuxiliaryVisuals(ScannerVisualState visualState) { }
    protected virtual void OnCandidateLost() { }
    protected virtual void OnImmediateMismatch(Component candidate) { }
    protected virtual void OnStepActiveChanged(bool isActive) { }
    protected virtual void HandleScannerReset() { }
    protected virtual void HandleSuccessLatched() { }
    protected virtual void AfterFailureReset() { }
    protected virtual bool IsImmediateMismatch(Component candidate) => false;

    protected abstract Component GetStableCandidate();
    protected abstract void HandleStableCandidate(Component candidate);
}

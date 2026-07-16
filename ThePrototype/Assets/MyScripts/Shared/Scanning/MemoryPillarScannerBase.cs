using System.Collections;
using UnityEngine;

/// <summary>
/// Shared scan logic for the Memory Unit's pillar-style inputs.
/// Unlike the pedestal scanners, these pillars do not push down. They only
/// swap the highlight material on one specific renderer slot while a packet is
/// being validated.
/// </summary>
[DisallowMultipleComponent]
public abstract class MemoryPillarScannerBase : MonoBehaviour
{
    const float SuccessClipVolumeScale = 0.5f;

    protected enum ScannerVisualState
    {
        Inactive,
        Idle,
        Occupied,
        Success,
        Failure,
    }

    [Header("Visuals")]
    [SerializeField]
    Renderer m_TargetRenderer;

    [SerializeField]
    int m_TargetMaterialIndex = 2;

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
    float m_RequiredStableSeconds = 1f;

    [SerializeField]
    float m_FailureDisplaySeconds = 1.25f;

    [Header("Audio")]
    [SerializeField]
    AudioSource m_AudioSource;

    [SerializeField]
    AudioClip m_OccupiedClip;

    [SerializeField]
    AudioClip m_SuccessClip;

    [SerializeField]
    AudioClip m_FailureClip;

    Component m_CurrentCandidate;
    Component m_CurrentMismatchCandidate;
    Coroutine m_FailureRoutine;
    float m_CurrentScanTime;
    bool m_IsActive;
    bool m_IsAwaitingValidation;
    bool m_IsEvaluatingMismatch;
    bool m_IsLatchedSuccessful;
    ScannerVisualState m_VisualState = ScannerVisualState.Inactive;

    protected bool IsScannerActive => m_IsActive;
    protected bool IsLatchedSuccessful => m_IsLatchedSuccessful;
    protected float RequiredStableSeconds => m_RequiredStableSeconds;
    protected Material SuccessMaterial => m_SuccessMaterial;
    protected T CurrentCandidateAs<T>() where T : Component => m_CurrentCandidate as T;

    protected virtual void Awake()
    {
        CacheVisualReferences();
        ApplyCurrentVisualState();
    }

    protected virtual void OnEnable()
    {
        CacheVisualReferences();
    }

    protected virtual void OnValidate()
    {
        CacheVisualReferences();

        if (Application.isPlaying)
            return;

        m_CurrentCandidate = null;
        m_CurrentMismatchCandidate = null;
        m_CurrentScanTime = 0f;
        m_IsActive = false;
        m_IsAwaitingValidation = false;
        m_IsEvaluatingMismatch = false;
        m_IsLatchedSuccessful = false;
        m_VisualState = ScannerVisualState.Inactive;
        ApplyCurrentVisualState();
    }

    protected virtual void Update()
    {
        if (!m_IsActive || m_IsAwaitingValidation || m_IsLatchedSuccessful)
            return;

        var candidate = GetStableCandidate();
        if (candidate == null)
        {
            m_CurrentCandidate = null;
            m_CurrentMismatchCandidate = null;
            m_CurrentScanTime = 0f;
            m_IsEvaluatingMismatch = false;
            SetVisualState(ScannerVisualState.Idle);
            OnCandidateLost();
            return;
        }

        if (IsImmediateMismatch(candidate))
        {
            // Mirror the successful scan cadence so invalid packets still read
            // as "being checked" before the pillar settles on failure.
            if (!m_IsEvaluatingMismatch || candidate != m_CurrentMismatchCandidate)
            {
                m_CurrentMismatchCandidate = candidate;
                m_CurrentCandidate = null;
                m_CurrentScanTime = 0f;
                m_IsEvaluatingMismatch = true;
                PlayClip(m_OccupiedClip);
            }

            m_CurrentScanTime += Time.deltaTime;
            SetVisualState(ScannerVisualState.Occupied);

            if (m_CurrentScanTime < m_RequiredStableSeconds)
                return;

            m_IsAwaitingValidation = true;
            m_CurrentScanTime = 0f;
            OnImmediateMismatch(candidate);
            FlashFailure();
            return;
        }

        if (m_IsEvaluatingMismatch)
        {
            m_IsEvaluatingMismatch = false;
            m_CurrentScanTime = 0f;
            m_CurrentCandidate = null;
        }

        m_CurrentMismatchCandidate = null;

        if (candidate != m_CurrentCandidate)
        {
            m_CurrentCandidate = candidate;
            m_CurrentScanTime = 0f;
            PlayClip(m_OccupiedClip);
        }

        m_CurrentScanTime += Time.deltaTime;
        SetVisualState(ScannerVisualState.Occupied);

        if (m_CurrentScanTime < m_RequiredStableSeconds)
            return;

        m_IsAwaitingValidation = true;
        m_CurrentScanTime = 0f;
        HandleStableCandidate(candidate);
    }

    public void SetScannerActive(bool isActive)
    {
        if (m_IsActive == isActive)
            return;

        m_IsActive = isActive;
        m_IsAwaitingValidation = false;
        m_IsLatchedSuccessful = false;
        m_CurrentCandidate = null;
        m_CurrentMismatchCandidate = null;
        m_CurrentScanTime = 0f;
        m_IsEvaluatingMismatch = false;

        if (m_FailureRoutine != null)
        {
            StopCoroutine(m_FailureRoutine);
            m_FailureRoutine = null;
        }

        OnScannerActiveChanged(isActive);
        SetVisualState(isActive ? ScannerVisualState.Idle : ScannerVisualState.Inactive);
    }

    public void ResetScanner()
    {
        m_CurrentCandidate = null;
        m_CurrentMismatchCandidate = null;
        m_CurrentScanTime = 0f;
        m_IsAwaitingValidation = false;
        m_IsEvaluatingMismatch = false;
        m_IsLatchedSuccessful = false;

        if (m_FailureRoutine != null)
        {
            StopCoroutine(m_FailureRoutine);
            m_FailureRoutine = null;
        }

        HandleScannerReset();
        SetVisualState(m_IsActive ? ScannerVisualState.Idle : ScannerVisualState.Inactive);
    }

    public void MarkSuccess()
    {
        m_IsAwaitingValidation = false;
        m_IsLatchedSuccessful = true;
        HandleSuccessLatched();
        SetVisualState(ScannerVisualState.Success);
        PlayClip(m_SuccessClip, SuccessClipVolumeScale);
    }

    public void FlashFailure()
    {
        if (m_FailureRoutine != null)
            StopCoroutine(m_FailureRoutine);

        m_FailureRoutine = StartCoroutine(FlashFailureRoutine());
    }

    protected virtual void CacheVisualReferences()
    {
        if (m_TargetRenderer != null)
            return;

        foreach (var renderer in GetComponentsInChildren<Renderer>(true))
        {
            if (renderer != null)
            {
                m_TargetRenderer = renderer;
                break;
            }
        }
    }

    protected void SetVisualState(ScannerVisualState visualState)
    {
        m_VisualState = visualState;
        ApplyCurrentVisualState();
    }

    void ApplyCurrentVisualState()
    {
        var targetMaterial = m_VisualState switch
        {
            ScannerVisualState.Inactive => m_InactiveMaterial,
            ScannerVisualState.Idle => m_IdleMaterial != null ? m_IdleMaterial : m_InactiveMaterial,
            ScannerVisualState.Occupied => m_OccupiedMaterial != null ? m_OccupiedMaterial : m_IdleMaterial,
            ScannerVisualState.Success => m_SuccessMaterial != null ? m_SuccessMaterial : m_IdleMaterial,
            ScannerVisualState.Failure => m_FailureMaterial != null ? m_FailureMaterial : m_IdleMaterial,
            _ => m_IdleMaterial,
        };

        ApplyMaterial(targetMaterial);
        ApplyAuxiliaryVisuals(m_VisualState);
    }

    void ApplyMaterial(Material targetMaterial)
    {
        if (m_TargetRenderer == null || targetMaterial == null)
            return;

        var materials = m_TargetRenderer.sharedMaterials;
        if (materials == null || materials.Length == 0)
            return;

        if (m_TargetMaterialIndex < 0 || m_TargetMaterialIndex >= materials.Length)
            return;

        materials[m_TargetMaterialIndex] = targetMaterial;
        m_TargetRenderer.sharedMaterials = materials;
    }

    IEnumerator FlashFailureRoutine()
    {
        m_IsAwaitingValidation = true;
        SetVisualState(ScannerVisualState.Failure);
        PlayClip(m_FailureClip);
        yield return new WaitForSeconds(m_FailureDisplaySeconds);

        m_IsAwaitingValidation = false;
        m_FailureRoutine = null;
        m_CurrentCandidate = null;
        m_CurrentMismatchCandidate = null;
        m_CurrentScanTime = 0f;
        m_IsEvaluatingMismatch = false;

        AfterFailureReset();
        SetVisualState(m_IsActive ? ScannerVisualState.Idle : ScannerVisualState.Inactive);
    }

    protected virtual void ApplyAuxiliaryVisuals(ScannerVisualState visualState) { }
    protected virtual void OnCandidateLost() { }
    protected virtual void OnImmediateMismatch(Component candidate) { }
    protected virtual void OnScannerActiveChanged(bool isActive) { }
    protected virtual void HandleScannerReset() { }
    protected virtual void HandleSuccessLatched() { }
    protected virtual void AfterFailureReset() { }
    protected virtual bool IsImmediateMismatch(Component candidate) => false;

    void PlayClip(AudioClip clip, float volumeScale = 1f)
    {
        if (m_AudioSource == null || clip == null)
            return;

        // Keep scanner audio authored per-object while still allowing a small
        // baked-in volume trim for the success cue.
        m_AudioSource.PlayOneShot(clip, volumeScale);
    }

    protected abstract Component GetStableCandidate();
    protected abstract void HandleStableCandidate(Component candidate);
}

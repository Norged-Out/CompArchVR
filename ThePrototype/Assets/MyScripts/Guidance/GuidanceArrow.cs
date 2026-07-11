using UnityEngine;

/// <summary>
/// Drives a single authored guidance arrow.
/// The arrow can pulse its emission and optionally breathe in scale so it reads
/// clearly from a distance without needing any lesson-flow hookup.
/// </summary>
[DisallowMultipleComponent]
public sealed class GuidanceArrow : MonoBehaviour
{
    static readonly int k_EmissionColorId = Shader.PropertyToID("_EmissionColor");

    [Header("Targets")]
    [SerializeField]
    Renderer[] m_Renderers;

    [Header("Activation")]
    [SerializeField]
    bool m_StartActive = true;

    [SerializeField]
    bool m_HideWhenInactive = true;

    [Header("Emission Pulse")]
    [SerializeField]
    Color m_PulseColor = new(0.15f, 0.95f, 1f, 1f);

    [SerializeField]
    float m_MinEmission = 0.1f;

    [SerializeField]
    float m_MaxEmission = 2.2f;

    [SerializeField]
    float m_PulseFrequency = 2f;

    [Header("Optional Scale Breath")]
    [SerializeField]
    bool m_UseScalePulse = true;

    [SerializeField]
    float m_ScalePulseAmount = 0.08f;

    [SerializeField]
    float m_ScalePulseFrequency = 2f;

    Vector3 m_BaseLocalScale;
    MaterialPropertyBlock m_PropertyBlock;
    bool m_IsGuidanceActive;
    float m_TimeOffsetSeconds;

    /// <summary>
    /// Initializes authored targets and restores the requested startup state.
    /// </summary>
    void Awake()
    {
        CacheTargets();
        m_BaseLocalScale = transform.localScale;
        m_PropertyBlock ??= new MaterialPropertyBlock();
        SetGuidanceActive(m_StartActive);
    }

    /// <summary>
    /// Restores authored visuals whenever Unity re-enables the arrow object.
    /// </summary>
    void OnEnable()
    {
        CacheTargets();
        m_BaseLocalScale = transform.localScale;
        m_PropertyBlock ??= new MaterialPropertyBlock();
        ApplyVisualState(0f);
    }

    /// <summary>
    /// Keeps the pulse animation running while this arrow is marked active.
    /// </summary>
    void Update()
    {
        if (!m_IsGuidanceActive)
            return;

        ApplyVisualState(Time.time + m_TimeOffsetSeconds);
    }

    /// <summary>
    /// Lets a lesson-level controller show or hide the arrow without destroying
    /// its authored placement.
    /// </summary>
    public void SetGuidanceActive(bool isActive, float timeOffsetSeconds = 0f)
    {
        m_TimeOffsetSeconds = timeOffsetSeconds;
        m_IsGuidanceActive = isActive;

        if (!isActive)
        {
            ApplyInactiveVisuals();
            if (m_HideWhenInactive)
                gameObject.SetActive(false);

            return;
        }

        if (m_HideWhenInactive && !gameObject.activeSelf)
            gameObject.SetActive(true);

        ApplyVisualState(Time.time + m_TimeOffsetSeconds);
    }

    /// <summary>
    /// Auto-fills the renderer list when none were authored manually.
    /// </summary>
    void CacheTargets()
    {
        if (m_Renderers != null && m_Renderers.Length > 0)
            return;

        m_Renderers = GetComponentsInChildren<Renderer>(true);
    }

    /// <summary>
    /// Applies the animated pulse at the provided sample time.
    /// </summary>
    void ApplyVisualState(float sampleTime)
    {
        var emissionLerp = 0.5f + (0.5f * Mathf.Sin(sampleTime * m_PulseFrequency * Mathf.PI * 2f));
        var emissionStrength = Mathf.Lerp(m_MinEmission, m_MaxEmission, emissionLerp);
        var emissionColor = m_PulseColor * emissionStrength;

        if (m_Renderers != null)
        {
            foreach (var targetRenderer in m_Renderers)
            {
                if (targetRenderer == null)
                    continue;

                targetRenderer.GetPropertyBlock(m_PropertyBlock);
                m_PropertyBlock.SetColor(k_EmissionColorId, emissionColor);
                targetRenderer.SetPropertyBlock(m_PropertyBlock);
            }
        }

        if (!m_UseScalePulse)
            return;

        var scaleLerp = 0.5f + (0.5f * Mathf.Sin(sampleTime * m_ScalePulseFrequency * Mathf.PI * 2f));
        var scaleMultiplier = 1f + (m_ScalePulseAmount * scaleLerp);
        transform.localScale = m_BaseLocalScale * scaleMultiplier;
    }

    /// <summary>
    /// Clears the active pulse and restores the authored resting scale.
    /// </summary>
    void ApplyInactiveVisuals()
    {
        if (m_Renderers != null)
        {
            foreach (var targetRenderer in m_Renderers)
            {
                if (targetRenderer == null)
                    continue;

                targetRenderer.GetPropertyBlock(m_PropertyBlock);
                m_PropertyBlock.SetColor(k_EmissionColorId, m_PulseColor * m_MinEmission);
                targetRenderer.SetPropertyBlock(m_PropertyBlock);
            }
        }

        transform.localScale = m_BaseLocalScale;
    }
}

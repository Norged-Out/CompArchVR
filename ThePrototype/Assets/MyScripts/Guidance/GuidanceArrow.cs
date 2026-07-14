using UnityEngine;

/// <summary>
/// Simple pulsing arrow.
/// Attach this to either a single arrow mesh or a parent that contains several arrow meshes.
/// </summary>
[DisallowMultipleComponent]
public sealed class GuidanceArrow : MonoBehaviour
{
    const string k_EmissionColorProperty = "_EmissionColor";

    [SerializeField]
    Renderer[] m_Renderers;

    [SerializeField]
    Color m_PulseColor = new(0.15f, 0.95f, 1f, 1f);

    [SerializeField]
    float m_MinEmission = 0.04f;

    [SerializeField]
    float m_MaxEmission = 0.9f;

    [SerializeField]
    float m_PulseSeconds = 2.4f;

    [SerializeField]
    bool m_HideWhenInactive = true;

    MaterialPropertyBlock m_PropertyBlock;

    bool m_IsActive;
    float m_TimeOffsetSeconds;

    void Awake()
    {
        m_PropertyBlock = new MaterialPropertyBlock();
        CacheRenderers();
        SetGuidanceActive(false);
    }

    void Update()
    {
        if (!m_IsActive)
            return;

        ApplyPulse(Time.time + m_TimeOffsetSeconds);
    }

    /// <summary>
    /// Enables or disables this arrow group.
    /// The optional offset lets a parent controller stagger several arrows into a route sequence.
    /// </summary>
    public void SetGuidanceActive(bool isActive, float timeOffsetSeconds = 0f)
    {
        m_IsActive = isActive;
        m_TimeOffsetSeconds = timeOffsetSeconds;

        if (!isActive)
        {
            // Keep a faint resting glow so hidden arrows do not flash from black
            // the next time a group is enabled.
            ApplyRestingEmission();
            if (m_HideWhenInactive)
                gameObject.SetActive(false);

            return;
        }

        if (m_HideWhenInactive && !gameObject.activeSelf)
            gameObject.SetActive(true);

        ApplyPulse(Time.time + m_TimeOffsetSeconds);
    }

    void CacheRenderers()
    {
        if (m_Renderers != null && m_Renderers.Length > 0)
            return;

        m_Renderers = GetComponentsInChildren<Renderer>(true);
    }

    void ApplyPulse(float sampleTime)
    {
        var cycleSeconds = Mathf.Max(0.01f, m_PulseSeconds);
        // Use a simple sine wave so staggered arrows read like a moving route
        // without needing animation clips or authored timelines.
        var t = 0.5f + 0.5f * Mathf.Sin((sampleTime / cycleSeconds) * Mathf.PI * 2f);
        var emission = Mathf.Lerp(m_MinEmission, m_MaxEmission, t);
        var color = m_PulseColor * emission;

        ApplyEmission(color);
    }

    void ApplyRestingEmission()
    {
        ApplyEmission(m_PulseColor * m_MinEmission);
    }

    void ApplyEmission(Color color)
    {
        if (m_Renderers == null)
            return;

        foreach (var targetRenderer in m_Renderers)
        {
            if (targetRenderer == null)
                continue;

            targetRenderer.GetPropertyBlock(m_PropertyBlock);
            m_PropertyBlock.SetColor(k_EmissionColorProperty, color);
            targetRenderer.SetPropertyBlock(m_PropertyBlock);
        }
    }
}

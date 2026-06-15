using UnityEngine;

/// <summary>
/// Cycles through a fixed list of renderers and keeps exactly one node highlighted at a time.
/// This is used by the physical push button in Testing Ground to step through the placeholder
/// CPU datapath nodes in order.
/// </summary>
public class CpuNodeSequenceController : MonoBehaviour
{
    // The order here matters: the first renderer is highlighted on the first button press,
    // the second renderer on the next press, and so on.
    [SerializeField]
    Renderer[] m_NodeRenderers;

    // Main color tint applied to the active node.
    [SerializeField]
    Color m_HighlightColor = new(1f, 0.82f, 0.2f, 1f);

    // Extra glow color for shaders/materials that expose an emission channel.
    // If the material does not support emission, this value will not be visible.
    [SerializeField]
    Color m_HighlightEmissionColor = new(0.85f, 0.45f, 0.05f, 1f);

    // Small scale-up so the active node feels more "selected" even without strong material effects.
    [SerializeField]
    float m_HighlightScaleMultiplier = 1.08f;

    // When enabled, the sequence wraps back to the first node after the last one.
    [SerializeField]
    bool m_Loop = true;

    // Common shader property names used by Unity materials.
    static readonly int k_BaseColorId = Shader.PropertyToID("_BaseColor");
    static readonly int k_ColorId = Shader.PropertyToID("_Color");
    static readonly int k_EmissionColorId = Shader.PropertyToID("_EmissionColor");

    MaterialPropertyBlock m_PropertyBlock;
    Vector3[] m_OriginalScales;
    Color[] m_OriginalColors;
    bool[] m_UsesBaseColor;
    bool[] m_HasEmission;
    int m_CurrentIndex = -1;

    /// <summary>
    /// Caches the original renderer state once so we can safely restore it later.
    /// </summary>
    void Awake()
    {
        CacheNodeState();
        ApplyHighlight(-1);
    }

    /// <summary>
    /// Resets every node when the component is disabled so the scene does not keep stale highlight state.
    /// </summary>
    void OnDisable()
    {
        ApplyHighlight(-1);
    }

    /// <summary>
    /// Advances the highlight to the next renderer in the list.
    /// Called by the physical XR push button in the scene.
    /// </summary>
    [ContextMenu("Advance Highlight")]
    public void AdvanceHighlight()
    {
        if (m_NodeRenderers == null || m_NodeRenderers.Length == 0)
            return;

        var nextIndex = m_CurrentIndex + 1;
        if (nextIndex >= m_NodeRenderers.Length)
        {
            if (!m_Loop)
                return;

            nextIndex = 0;
        }

        ApplyHighlight(nextIndex);
    }

    /// <summary>
    /// Clears the active highlight and restores every node to its original appearance.
    /// </summary>
    [ContextMenu("Reset Highlight")]
    public void ResetHighlight()
    {
        ApplyHighlight(-1);
    }

    /// <summary>
    /// Reads each renderer's original scale and material colors.
    /// We use sharedMaterial here only to capture the baseline values without instantiating materials.
    /// </summary>
    void CacheNodeState()
    {
        if (m_NodeRenderers == null)
            return;

        m_PropertyBlock ??= new MaterialPropertyBlock();
        m_OriginalScales = new Vector3[m_NodeRenderers.Length];
        m_OriginalColors = new Color[m_NodeRenderers.Length];
        m_UsesBaseColor = new bool[m_NodeRenderers.Length];
        m_HasEmission = new bool[m_NodeRenderers.Length];

        for (var i = 0; i < m_NodeRenderers.Length; i++)
        {
            var nodeRenderer = m_NodeRenderers[i];
            if (nodeRenderer == null)
                continue;

            // Store the node's original size so we can enlarge only the active one.
            m_OriginalScales[i] = nodeRenderer.transform.localScale;

            var material = nodeRenderer.sharedMaterial;
            if (material == null)
            {
                m_OriginalColors[i] = Color.white;
                continue;
            }

            if (material.HasProperty(k_BaseColorId))
            {
                m_UsesBaseColor[i] = true;
                m_OriginalColors[i] = material.GetColor(k_BaseColorId);
            }
            else if (material.HasProperty(k_ColorId))
            {
                m_OriginalColors[i] = material.GetColor(k_ColorId);
            }
            else
            {
                m_OriginalColors[i] = Color.white;
            }

            // Not every shader supports emission, so we cache that capability per renderer.
            m_HasEmission[i] = material.HasProperty(k_EmissionColorId);
        }
    }

    /// <summary>
    /// Applies highlight styling to the requested index and restores all others.
    /// </summary>
    /// <param name="highlightedIndex">
    /// The node index to highlight, or -1 to clear all highlights.
    /// </param>
    void ApplyHighlight(int highlightedIndex)
    {
        m_CurrentIndex = highlightedIndex;

        if (m_NodeRenderers == null || m_NodeRenderers.Length == 0)
            return;

        m_PropertyBlock ??= new MaterialPropertyBlock();

        for (var i = 0; i < m_NodeRenderers.Length; i++)
        {
            var nodeRenderer = m_NodeRenderers[i];
            if (nodeRenderer == null)
                continue;

            var isHighlighted = i == highlightedIndex;

            // Scale only the active node so it stands out even if the material change is subtle.
            nodeRenderer.transform.localScale = isHighlighted
                ? m_OriginalScales[i] * m_HighlightScaleMultiplier
                : m_OriginalScales[i];

            // MaterialPropertyBlock lets us override visuals per renderer without modifying
            // the shared material asset used elsewhere in the project.
            m_PropertyBlock.Clear();

            if (m_UsesBaseColor[i])
                m_PropertyBlock.SetColor(k_BaseColorId, isHighlighted ? m_HighlightColor : m_OriginalColors[i]);
            else
                m_PropertyBlock.SetColor(k_ColorId, isHighlighted ? m_HighlightColor : m_OriginalColors[i]);

            // If the shader supports emission, add a little extra glow to the active node.
            if (m_HasEmission[i])
                m_PropertyBlock.SetColor(k_EmissionColorId, isHighlighted ? m_HighlightEmissionColor : Color.black);

            nodeRenderer.SetPropertyBlock(m_PropertyBlock);
        }
    }
}

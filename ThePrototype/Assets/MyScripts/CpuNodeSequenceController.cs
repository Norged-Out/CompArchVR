using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles visual highlighting for datapath placeholder nodes.
///
/// The original version only stepped through a fixed renderer array.
/// This version keeps that legacy fallback, but can also highlight exact
/// logical datapath nodes by consulting the scene-side registry.
/// </summary>
public class CpuNodeSequenceController : MonoBehaviour
{
    [SerializeField]
    Renderer[] m_NodeRenderers;

    [SerializeField]
    Color m_HighlightColor = new(1f, 0.82f, 0.2f, 1f);

    [SerializeField]
    Color m_HighlightEmissionColor = new(0.85f, 0.45f, 0.05f, 1f);

    [SerializeField]
    float m_HighlightScaleMultiplier = 1.08f;

    [SerializeField]
    bool m_Loop = true;

    static readonly int k_BaseColorId = Shader.PropertyToID("_BaseColor");
    static readonly int k_ColorId = Shader.PropertyToID("_Color");
    static readonly int k_EmissionColorId = Shader.PropertyToID("_EmissionColor");

    readonly List<Renderer> m_TrackedRenderers = new();
    readonly Dictionary<Renderer, RendererVisualState> m_RendererStates = new();

    MaterialPropertyBlock m_PropertyBlock;
    Renderer m_CurrentHighlightedRenderer;
    int m_CurrentSequenceIndex = -1;

    NodeMap m_NodeMap;
    CpuLessonFlow m_LessonFlow;

    struct RendererVisualState
    {
        public Vector3 originalScale;
        public Color originalColor;
        public bool usesBaseColor;
        public bool hasEmission;
    }

    void Awake()
    {
        m_NodeMap = GetComponent<NodeMap>();
        m_LessonFlow = GetComponent<CpuLessonFlow>();

        if (m_NodeMap != null && m_NodeMap.HasAnyNodes)
            SetTrackedRenderers(m_NodeMap.GetOrderedRenderers());
        else
            SetTrackedRenderers(m_NodeRenderers);

        ResetHighlight();
    }

    void OnDisable()
    {
        ResetHighlight();
    }

    /// <summary>
    /// Replaces the tracked renderer set.
    /// This is used after runtime scene setup adds the Data Memory placeholder.
    /// </summary>
    public void SetTrackedRenderers(IEnumerable<Renderer> renderers)
    {
        ResetHighlight();

        m_TrackedRenderers.Clear();
        m_RendererStates.Clear();
        m_CurrentSequenceIndex = -1;

        if (renderers == null)
            return;

        foreach (var nodeRenderer in renderers)
        {
            if (nodeRenderer == null || m_RendererStates.ContainsKey(nodeRenderer))
                continue;

            m_TrackedRenderers.Add(nodeRenderer);
            CacheRendererState(nodeRenderer);
        }
    }

    /// <summary>
    /// Existing scene push button entrypoint.
    ///
    /// If the instruction lesson controller is active, the button advances the
    /// lesson instead of blindly stepping to the next renderer.
    /// </summary>
    [ContextMenu("Advance Highlight")]
    public void AdvanceHighlight()
    {
        m_LessonFlow ??= GetComponent<CpuLessonFlow>();
        if (m_LessonFlow != null && m_LessonFlow.IsLessonModeActive)
        {
            m_LessonFlow.HandleAdvanceButtonPressed();
            return;
        }

        if (m_TrackedRenderers.Count == 0)
            TryRefreshTrackedRenderers();

        if (m_TrackedRenderers.Count == 0)
            return;

        var nextIndex = m_CurrentSequenceIndex + 1;
        if (nextIndex >= m_TrackedRenderers.Count)
        {
            if (!m_Loop)
                return;

            nextIndex = 0;
        }

        m_CurrentSequenceIndex = nextIndex;
        HighlightRenderer(m_TrackedRenderers[nextIndex]);
    }

    /// <summary>
    /// Highlights an exact logical datapath node if the registry knows it.
    /// </summary>
    public void HighlightNode(DatapathNodeId nodeId)
    {
        if (nodeId == DatapathNodeId.None)
        {
            ResetHighlight();
            return;
        }

        m_NodeMap ??= GetComponent<NodeMap>();
        if (m_NodeMap == null)
        {
            ResetHighlight();
            return;
        }

        if (!m_NodeMap.TryGetRenderer(nodeId, out var targetRenderer) || targetRenderer == null)
        {
            ResetHighlight();
            return;
        }

        if (!m_RendererStates.ContainsKey(targetRenderer))
            SetTrackedRenderers(m_NodeMap.GetOrderedRenderers());

        HighlightRenderer(targetRenderer);
    }

    /// <summary>
    /// Clears the active highlight and restores all tracked renderers.
    /// </summary>
    [ContextMenu("Reset Highlight")]
    public void ResetHighlight()
    {
        HighlightRenderer(null);
        m_CurrentSequenceIndex = -1;
    }

    void TryRefreshTrackedRenderers()
    {
        m_NodeMap ??= GetComponent<NodeMap>();
        if (m_NodeMap != null)
            SetTrackedRenderers(m_NodeMap.GetOrderedRenderers());
    }

    void CacheRendererState(Renderer nodeRenderer)
    {
        var state = new RendererVisualState
        {
            originalScale = nodeRenderer.transform.localScale,
            originalColor = Color.white,
            usesBaseColor = false,
            hasEmission = false,
        };

        var material = nodeRenderer.sharedMaterial;
        if (material != null)
        {
            if (material.HasProperty(k_BaseColorId))
            {
                state.usesBaseColor = true;
                state.originalColor = material.GetColor(k_BaseColorId);
            }
            else if (material.HasProperty(k_ColorId))
            {
                state.originalColor = material.GetColor(k_ColorId);
            }

            state.hasEmission = material.HasProperty(k_EmissionColorId);
        }

        m_RendererStates[nodeRenderer] = state;
    }

    void HighlightRenderer(Renderer targetRenderer)
    {
        m_CurrentHighlightedRenderer = targetRenderer;
        m_PropertyBlock ??= new MaterialPropertyBlock();

        foreach (var nodeRenderer in m_TrackedRenderers)
        {
            if (nodeRenderer == null || !m_RendererStates.TryGetValue(nodeRenderer, out var state))
                continue;

            var isHighlighted = nodeRenderer == targetRenderer;
            nodeRenderer.transform.localScale = isHighlighted
                ? state.originalScale * m_HighlightScaleMultiplier
                : state.originalScale;

            m_PropertyBlock.Clear();

            if (state.usesBaseColor)
                m_PropertyBlock.SetColor(k_BaseColorId, isHighlighted ? m_HighlightColor : state.originalColor);
            else
                m_PropertyBlock.SetColor(k_ColorId, isHighlighted ? m_HighlightColor : state.originalColor);

            if (state.hasEmission)
                m_PropertyBlock.SetColor(k_EmissionColorId, isHighlighted ? m_HighlightEmissionColor : Color.black);

            nodeRenderer.SetPropertyBlock(m_PropertyBlock);
        }
    }
}

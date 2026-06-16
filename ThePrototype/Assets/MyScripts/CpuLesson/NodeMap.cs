using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Scene-side lookup table that maps logical datapath node ids to the actual
/// placeholder objects in Testing Ground.
/// The lesson code talks in terms of logical node ids so later scenes can keep
/// the same lesson behavior even if the visuals change.
/// </summary>
public class NodeMap : MonoBehaviour
{
    static readonly Dictionary<string, DatapathNodeId> k_NodeNameMap =
        new(StringComparer.OrdinalIgnoreCase)
        {
            { "PC", DatapathNodeId.ProgramCounter },
            { "Instruction Memory", DatapathNodeId.InstructionMemory },
            { "Registers", DatapathNodeId.Registers },
            { "ALU", DatapathNodeId.ALU },
            { "Data Memory", DatapathNodeId.DataMemory },
            { "Write Back", DatapathNodeId.WriteBack },
        };

    static readonly DatapathNodeId[] k_OrderedNodeIds =
    {
        DatapathNodeId.ProgramCounter,
        DatapathNodeId.InstructionMemory,
        DatapathNodeId.Registers,
        DatapathNodeId.ALU,
        DatapathNodeId.DataMemory,
        DatapathNodeId.WriteBack,
    };

    readonly Dictionary<DatapathNodeId, Renderer> m_NodeRenderers = new();
    readonly Dictionary<DatapathNodeId, Transform> m_NodeTransforms = new();

    public bool HasAnyNodes => m_NodeRenderers.Count > 0;

    void Awake()
    {
        RebuildRegistry();
    }

    /// <summary>
    /// Re-scans direct children and rebuilds the logical node map.
    /// Call this after placeholder nodes are added or renamed.
    /// </summary>
    [ContextMenu("Rebuild Node Map")]
    public void RebuildRegistry()
    {
        m_NodeRenderers.Clear();
        m_NodeTransforms.Clear();

        foreach (Transform child in transform)
        {
            if (!k_NodeNameMap.TryGetValue(child.name, out var nodeId))
                continue;

            var nodeRenderer = child.GetComponent<Renderer>();
            if (nodeRenderer == null)
                nodeRenderer = child.GetComponentInChildren<Renderer>();

            if (nodeRenderer == null)
                continue;

            m_NodeTransforms[nodeId] = child;
            m_NodeRenderers[nodeId] = nodeRenderer;
        }
    }

    /// <summary>
    /// Tries to fetch the renderer used to highlight a logical node.
    /// </summary>
    public bool TryGetRenderer(DatapathNodeId nodeId, out Renderer renderer)
    {
        return m_NodeRenderers.TryGetValue(nodeId, out renderer);
    }

    /// <summary>
    /// Tries to fetch the transform of a logical node.
    /// </summary>
    public bool TryGetTransform(DatapathNodeId nodeId, out Transform nodeTransform)
    {
        return m_NodeTransforms.TryGetValue(nodeId, out nodeTransform);
    }

    /// <summary>
    /// Returns registered renderers in lesson order.
    /// </summary>
    public Renderer[] GetOrderedRenderers()
    {
        var orderedRenderers = new List<Renderer>(k_OrderedNodeIds.Length);
        foreach (var nodeId in k_OrderedNodeIds)
        {
            if (m_NodeRenderers.TryGetValue(nodeId, out var renderer) && renderer != null)
                orderedRenderers.Add(renderer);
        }

        return orderedRenderers.ToArray();
    }
}

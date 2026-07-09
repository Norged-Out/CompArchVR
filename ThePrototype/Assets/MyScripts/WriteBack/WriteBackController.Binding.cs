using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// Prefab-local binding helpers for write-back controls, scanners, and pipes.
/// </summary>
public partial class WriteBackController
{
    void HandleHintDropdownChanged(int _)
    {
        RefreshPresentation();
    }

    void CacheReferences()
    {
        // Keep this limited to prefab-local components. WB UI bindings are
        // authored scene references and should not be rediscovered at runtime.
        m_RegisterScanner ??= FindChildComponent<WriteBackRegisterScanner>("Reg Input");
        m_PacketScanner ??= FindChildComponent<WriteBackPacketScanner>("Data Input");
        m_TransferParticles ??= GetComponentInChildren<ParticleSystem>(true);
        m_RegDstButtonRoot ??= transform.Find("RegDst Button");
        m_RegWriteButtonRoot ??= transform.Find("RegWrite Button");
        m_MemToRegButtonRoot ??= transform.Find("MemToReg Button");

        if (m_PipeRenderers == null || m_PipeRenderers.Length == 0)
        {
            var renderers = new List<Renderer>();
            AddPipeRendererIfFound(renderers, "DataPipe");
            AddPipeRendererIfFound(renderers, "Pipe 1");
            AddPipeRendererIfFound(renderers, "Pipe 2");
            AddPipeRendererIfFound(renderers, "Pipe 3");
            AddPipeRendererIfFound(renderers, "RegPipe");
            m_PipeRenderers = renderers.ToArray();
        }
    }

    void CachePipeMaterials()
    {
        m_OriginalPipeMaterials.Clear();

        if (m_PipeRenderers == null)
            return;

        foreach (var pipeRenderer in m_PipeRenderers)
        {
            if (pipeRenderer != null && pipeRenderer.sharedMaterial != null)
                m_OriginalPipeMaterials[pipeRenderer] = pipeRenderer.sharedMaterial;
        }
    }

    void ResetPipeMaterials()
    {
        foreach (var pipeMaterialPair in m_OriginalPipeMaterials)
        {
            if (pipeMaterialPair.Key != null)
                pipeMaterialPair.Key.sharedMaterial = pipeMaterialPair.Value;
        }
    }

    void AddPipeRendererIfFound(List<Renderer> renderers, string objectName)
    {
        var pipeTransform = FindChildRecursive(transform, objectName);
        var pipeRenderer = pipeTransform != null ? pipeTransform.GetComponent<Renderer>() : null;
        if (pipeRenderer != null)
            renderers.Add(pipeRenderer);
    }

    void HookButtons()
    {
        HookPhysicalButton(m_RegDstButtonRoot, HandleRegDstPressed, true);
        HookPhysicalButton(m_RegWriteButtonRoot, HandleRegWritePressed, true);
        HookPhysicalButton(m_MemToRegButtonRoot, HandleMemToRegPressed, true);

        if (m_ActionButton != null)
        {
            m_ActionButton.onClick.RemoveListener(HandleActionPressed);
            m_ActionButton.onClick.AddListener(HandleActionPressed);
        }
    }

    void UnhookButtons()
    {
        HookPhysicalButton(m_RegDstButtonRoot, HandleRegDstPressed, false);
        HookPhysicalButton(m_RegWriteButtonRoot, HandleRegWritePressed, false);
        HookPhysicalButton(m_MemToRegButtonRoot, HandleMemToRegPressed, false);

        if (m_ActionButton != null)
            m_ActionButton.onClick.RemoveListener(HandleActionPressed);
    }

    void HookHintDropdown(bool subscribe)
    {
        if (m_HintDropdown == null)
            return;

        if (subscribe)
        {
            m_HintDropdown.onValueChanged.RemoveListener(HandleHintDropdownChanged);
            m_HintDropdown.onValueChanged.AddListener(HandleHintDropdownChanged);
        }
        else
        {
            m_HintDropdown.onValueChanged.RemoveListener(HandleHintDropdownChanged);
        }
    }

    void HookScannerEvents(bool subscribe)
    {
        HookScannerEvent(m_RegisterScanner, subscribe);
        HookPacketEvent(m_PacketScanner, subscribe);
    }

    void HookScannerEvent(WriteBackRegisterScanner scanner, bool subscribe)
    {
        if (scanner == null)
            return;

        scanner.RegisterAccepted -= HandleRegisterAccepted;
        if (subscribe)
            scanner.RegisterAccepted += HandleRegisterAccepted;
    }

    void HookPacketEvent(WriteBackPacketScanner scanner, bool subscribe)
    {
        if (scanner == null)
            return;

        scanner.PacketAccepted -= HandlePacketAccepted;
        if (subscribe)
            scanner.PacketAccepted += HandlePacketAccepted;
    }

    static void HookPhysicalButton(
        Transform buttonRoot,
        UnityEngine.Events.UnityAction<SelectEnterEventArgs> handler,
        bool subscribe)
    {
        var button = buttonRoot != null ? buttonRoot.GetComponent<XRSimpleInteractable>() : null;
        if (button == null)
            return;

        if (subscribe)
        {
            button.firstSelectEntered.RemoveListener(handler);
            button.firstSelectEntered.AddListener(handler);
        }
        else
        {
            button.firstSelectEntered.RemoveListener(handler);
        }
    }

    T FindChildComponent<T>(string childName) where T : Component
    {
        var childTransform = FindChildRecursive(transform, childName);
        return childTransform != null ? childTransform.GetComponent<T>() : null;
    }

    static Transform FindChildRecursive(Transform root, string childName)
    {
        if (root == null)
            return null;

        foreach (var childTransform in root.GetComponentsInChildren<Transform>(true))
        {
            if (childTransform != null && childTransform.name == childName)
                return childTransform;
        }

        return null;
    }
}

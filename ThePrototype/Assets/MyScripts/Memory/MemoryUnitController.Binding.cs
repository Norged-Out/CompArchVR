using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// Prefab-local binding helpers for the Memory Unit controller.
/// Scene-authored UI references stay serialized on the root component.
/// </summary>
public partial class MemoryUnitController
{
    void HandleHintDropdownChanged(int _)
    {
        RefreshPresentation();
    }

    void CacheReferences()
    {
        // Only prefab-local wiring is discovered here. Scene-authored UI and
        // bank references should already be serialized explicitly.
        m_AddressScanner ??= FindChildComponent<MemoryAddressScanner>("Address Input");
        m_DataScanner ??= FindChildComponent<MemoryPacketScanner>("Data Input");
        m_MemReadButtonRoot ??= transform.Find("MemRead Button");
        m_MemWriteButtonRoot ??= transform.Find("MemWrite Button");

        if (m_MemoryDataSpawnTransform == null)
        {
            var pedestalTransform = FindChildRecursive(transform, "Memory Data Pedestal");
            if (pedestalTransform != null)
                m_MemoryDataSpawnTransform = FindChildRecursive(pedestalTransform, "Spawn Point");
        }
    }

    void HookButtons()
    {
        HookPhysicalButton(m_MemReadButtonRoot, HandleMemReadPressed, true);
        HookPhysicalButton(m_MemWriteButtonRoot, HandleMemWritePressed, true);
    }

    void UnhookButtons()
    {
        HookPhysicalButton(m_MemReadButtonRoot, HandleMemReadPressed, false);
        HookPhysicalButton(m_MemWriteButtonRoot, HandleMemWritePressed, false);
    }

    void HookActionButton(bool subscribe)
    {
        if (m_ActionButton == null)
            return;

        if (subscribe)
        {
            m_ActionButton.onClick.RemoveListener(HandleActionPressed);
            m_ActionButton.onClick.AddListener(HandleActionPressed);
        }
        else
        {
            m_ActionButton.onClick.RemoveListener(HandleActionPressed);
        }
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
        HookAddressEvent(subscribe);
        HookDataEvent(subscribe);
    }

    void HookAddressEvent(bool subscribe)
    {
        if (m_AddressScanner == null)
            return;

        m_AddressScanner.PacketAccepted -= HandleAddressAccepted;
        if (subscribe)
            m_AddressScanner.PacketAccepted += HandleAddressAccepted;
    }

    void HookDataEvent(bool subscribe)
    {
        if (m_DataScanner == null)
            return;

        m_DataScanner.PacketAccepted -= HandleDataAccepted;
        if (subscribe)
            m_DataScanner.PacketAccepted += HandleDataAccepted;
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

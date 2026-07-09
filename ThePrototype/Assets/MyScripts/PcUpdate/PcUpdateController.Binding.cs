using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// Prefab-local binding helpers for program-counter update controls.
/// </summary>
public partial class PcUpdateController
{
    void HookButtons(bool subscribe)
    {
        HookPhysicalButton(m_BranchButtonRoot, HandleBranchPressed, subscribe);
        HookPhysicalButton(m_JumpButtonRoot, HandleJumpPressed, subscribe);

        if (m_ShiftButton != null)
        {
            m_ShiftButton.onClick.RemoveListener(HandleShiftPressed);
            if (subscribe)
                m_ShiftButton.onClick.AddListener(HandleShiftPressed);
        }

        if (m_ActionButton != null)
        {
            m_ActionButton.onClick.RemoveListener(HandleActionPressed);
            if (subscribe)
                m_ActionButton.onClick.AddListener(HandleActionPressed);
        }
    }

    void HookDropdown(bool subscribe)
    {
        if (m_BranchConditionDropdown != null)
        {
            m_BranchConditionDropdown.onValueChanged.RemoveListener(HandleDropdownChanged);
            if (subscribe)
                m_BranchConditionDropdown.onValueChanged.AddListener(HandleDropdownChanged);
        }

        if (m_HintDropdown != null)
        {
            m_HintDropdown.onValueChanged.RemoveListener(HandleDropdownChanged);
            if (subscribe)
                m_HintDropdown.onValueChanged.AddListener(HandleDropdownChanged);
        }
    }

    void HookSlider(bool subscribe)
    {
        if (m_PcIncrementSlider == null)
            return;

        m_PcIncrementSlider.onValueChanged.RemoveListener(HandleSliderChanged);
        if (subscribe)
            m_PcIncrementSlider.onValueChanged.AddListener(HandleSliderChanged);
    }

    void HookScannerEvents(bool subscribe)
    {
        HookScannerEvent(m_ImmediateScanner, HandleImmediateAccepted, subscribe);
        HookScannerEvent(m_ZeroScanner, HandleZeroAccepted, subscribe);
    }

    void HookScannerEvent(
        PcUpdatePacketScanner scanner,
        System.Action<PcUpdatePacketScanner, DataPacketToken> handler,
        bool subscribe)
    {
        if (scanner == null)
            return;

        scanner.PacketAccepted -= handler;
        if (subscribe)
            scanner.PacketAccepted += handler;
    }

    void CacheReferences()
    {
        // Program-counter update now relies on explicit scene wiring for its UI.
        // This method only patches up local prefab children when needed.
        m_ImmediateScanner ??= FindChildComponent<PcUpdatePacketScanner>(transform, "Immediate Input");
        m_ZeroScanner ??= FindChildComponent<PcUpdatePacketScanner>(transform, "Zero Input");
        m_BranchButtonRoot ??= FindChildTransform(transform, "Branch Button");
        m_JumpButtonRoot ??= FindChildTransform(transform, "Jump Button");
    }

    static void HookPhysicalButton(Transform buttonRoot, UnityEngine.Events.UnityAction<SelectEnterEventArgs> handler, bool subscribe)
    {
        if (buttonRoot == null)
            return;

        var interactable = buttonRoot.GetComponentInChildren<XRSimpleInteractable>(true);
        if (interactable == null)
            return;

        interactable.selectEntered.RemoveListener(handler);
        if (subscribe)
            interactable.selectEntered.AddListener(handler);
    }

    static T FindChildComponent<T>(Transform root, string childName) where T : Component
    {
        var childTransform = FindChildTransform(root, childName);
        return childTransform != null ? childTransform.GetComponent<T>() : null;
    }

    static Transform FindChildTransform(Transform root, string childName)
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

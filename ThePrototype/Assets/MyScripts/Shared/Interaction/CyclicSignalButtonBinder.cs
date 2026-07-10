using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// Shared helper for authored buttons that cycle through a fixed set of values
/// such as ALUOp or similar multi-state control lines.
/// </summary>
public static class CyclicSignalButtonBinder
{
    /// <summary>
    /// Wires a listener to a physical button that advances a multi-state value.
    /// </summary>
    public static void Bind(Transform buttonRoot, UnityAction<SelectEnterEventArgs> handler)
    {
        var interactable = ResolveInteractable(buttonRoot);
        if (interactable == null || handler == null)
            return;

        interactable.firstSelectEntered.RemoveListener(handler);
        interactable.firstSelectEntered.AddListener(handler);
    }

    /// <summary>
    /// Removes a previously wired listener from a multi-state physical button.
    /// </summary>
    public static void Unbind(Transform buttonRoot, UnityAction<SelectEnterEventArgs> handler)
    {
        var interactable = ResolveInteractable(buttonRoot);
        if (interactable == null || handler == null)
            return;

        interactable.firstSelectEntered.RemoveListener(handler);
    }

    /// <summary>
    /// Utility for advancing an index through a fixed authored state list.
    /// </summary>
    public static int GetNextIndex(int currentIndex, int stateCount)
    {
        if (stateCount <= 0)
            return 0;

        return (currentIndex + 1) % stateCount;
    }

    static XRSimpleInteractable ResolveInteractable(Transform buttonRoot)
    {
        return buttonRoot != null ? buttonRoot.GetComponentInChildren<XRSimpleInteractable>(true) : null;
    }
}

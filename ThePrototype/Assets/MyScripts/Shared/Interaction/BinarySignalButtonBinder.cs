using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// Shared helper for 0/1 physical lesson buttons.
/// This keeps subscribe / unsubscribe boilerplate out of phase controllers.
/// </summary>
public static class BinarySignalButtonBinder
{
    /// <summary>
    /// Wires a listener to an authored physical button root.
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
    /// Removes a previously wired listener from an authored physical button root.
    /// </summary>
    public static void Unbind(Transform buttonRoot, UnityAction<SelectEnterEventArgs> handler)
    {
        var interactable = ResolveInteractable(buttonRoot);
        if (interactable == null || handler == null)
            return;

        interactable.firstSelectEntered.RemoveListener(handler);
    }

    static XRSimpleInteractable ResolveInteractable(Transform buttonRoot)
    {
        return buttonRoot != null ? buttonRoot.GetComponentInChildren<XRSimpleInteractable>(true) : null;
    }
}

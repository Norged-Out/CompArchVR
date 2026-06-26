using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Scene-owned register bank manager.
///
/// The original version built temporary runtime buttons.
/// This version treats authored grabbable register pieces as the source of truth
/// and exposes the same selection-style callbacks the lesson flow already uses.
/// </summary>
[DisallowMultipleComponent]
public class RegisterBank : MonoBehaviour
{
    readonly Dictionary<string, RegisterToken> m_RegisterTokens =
        new(StringComparer.OrdinalIgnoreCase);

    public event Action<string> RegisterPressed;

    void Awake()
    {
        RefreshRegisterCache();
    }

    void OnEnable()
    {
        RefreshRegisterCache();
    }

    /// <summary>
    /// Legacy lesson entrypoint kept for compatibility.
    ///
    /// We no longer destroy and rebuild the bank here. Instead, we refresh the
    /// authored register cache and keep the full 32-register bank available so
    /// the user can work with real scene objects.
    /// </summary>
    public void BuildButtons(string[] _registerChoices)
    {
        RefreshRegisterCache();
    }

    /// <summary>
    /// Re-scans authored register tokens and wires them back to this bank.
    /// Call this after edit-time authoring or if scene children change.
    /// </summary>
    public void RefreshRegisterCache()
    {
        m_RegisterTokens.Clear();

        foreach (var registerToken in GetComponentsInChildren<RegisterToken>(true))
        {
            if (registerToken == null || string.IsNullOrWhiteSpace(registerToken.RegisterId))
                continue;

            registerToken.SetOwningBank(this);
            m_RegisterTokens[registerToken.RegisterId] = registerToken;
        }
    }

    /// <summary>
    /// Restores every register's lesson visual state without moving them.
    /// Useful when the lesson changes stages but the player is still physically
    /// holding or repositioning objects.
    /// </summary>
    public void ResetVisuals()
    {
        foreach (var registerToken in m_RegisterTokens.Values)
            registerToken.ResetVisualState();
    }

    /// <summary>
    /// Snaps every register back to its stored home pose on the authored bank.
    /// This is intended for the dedicated reset button near the bank.
    /// </summary>
    public void ResetAllRegisters()
    {
        RefreshRegisterCache();

        foreach (var registerToken in m_RegisterTokens.Values)
        {
            registerToken.ResetVisualState();
            registerToken.ResetToHome();
        }
    }

    /// <summary>
    /// Keeps successful lesson selections visible.
    /// </summary>
    public void SetSelected(string registerName)
    {
        if (string.IsNullOrWhiteSpace(registerName))
            return;

        if (m_RegisterTokens.TryGetValue(registerName, out var registerToken))
            registerToken.SetSelected(true);
    }

    /// <summary>
    /// Shows a short failure flash on the named register.
    /// </summary>
    public void FlashFailure(string registerName)
    {
        if (string.IsNullOrWhiteSpace(registerName))
            return;

        if (m_RegisterTokens.TryGetValue(registerName, out var registerToken))
            registerToken.FlashFailure();
    }

    /// <summary>
    /// Called by a register token when the player first grabs it.
    /// The lesson system reuses this as the current "selection" signal.
    /// </summary>
    public void NotifyRegisterGrabbed(RegisterToken registerToken)
    {
        if (registerToken == null || string.IsNullOrWhiteSpace(registerToken.RegisterId))
            return;

        RegisterPressed?.Invoke(registerToken.RegisterId);
    }
}

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
    readonly Dictionary<InstructionRegisterRole, RegisterScanner> m_RegisterScanners =
        new();

    public event Action<string> RegisterPressed;
    public event Action<InstructionRegisterRole, string> RegisterScanned;

    public bool HasRegisterScanners =>
        m_RegisterScanners.ContainsKey(InstructionRegisterRole.Rs) &&
        m_RegisterScanners.ContainsKey(InstructionRegisterRole.Rt) &&
        m_RegisterScanners.ContainsKey(InstructionRegisterRole.Rd);

    void Awake()
    {
        RefreshRegisterCache();
        RefreshScannerCache();
    }

    void OnEnable()
    {
        RefreshRegisterCache();
        RefreshScannerCache();
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
        RefreshScannerCache();
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
    /// Re-scans authored register scanners in the current scene.
    /// These scanners are not children of the bank, so we scene-filter them.
    /// </summary>
    public void RefreshScannerCache()
    {
        m_RegisterScanners.Clear();

        foreach (var registerScanner in Resources.FindObjectsOfTypeAll<RegisterScanner>())
        {
            if (registerScanner == null)
                continue;

            if (!registerScanner.gameObject.scene.IsValid() || !registerScanner.gameObject.scene.isLoaded)
                continue;

            if (registerScanner.gameObject.scene != gameObject.scene)
                continue;

            if (registerScanner.RegisterRole == InstructionRegisterRole.None)
                continue;

            m_RegisterScanners[registerScanner.RegisterRole] = registerScanner;
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
        RefreshScannerCache();

        foreach (var registerToken in m_RegisterTokens.Values)
        {
            registerToken.ResetVisualState();
            registerToken.ResetToHome();
        }

        foreach (var registerScanner in m_RegisterScanners.Values)
            registerScanner.ResetScanner();
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
    /// Enables only the scanners that the current lesson step actually uses.
    /// </summary>
    public void ConfigureScannerRoles(InstructionRegisterRole[] activeRoles)
    {
        RefreshScannerCache();

        foreach (var scannerPair in m_RegisterScanners)
        {
            var isActive = false;
            if (activeRoles != null)
            {
                foreach (var activeRole in activeRoles)
                {
                    if (activeRole == scannerPair.Key)
                    {
                        isActive = true;
                        break;
                    }
                }
            }

            scannerPair.Value.SetStepActive(isActive);
        }
    }

    /// <summary>
    /// Keeps the matching scanner green after successful validation.
    /// </summary>
    public void SetScannerSuccess(InstructionRegisterRole role)
    {
        RefreshScannerCache();

        if (m_RegisterScanners.TryGetValue(role, out var registerScanner))
            registerScanner.MarkSuccess();
    }

    /// <summary>
    /// Briefly flashes the matching scanner red for wrong pedestal / wrong token.
    /// </summary>
    public void FlashScannerFailure(InstructionRegisterRole role)
    {
        RefreshScannerCache();

        if (m_RegisterScanners.TryGetValue(role, out var registerScanner))
            registerScanner.FlashFailure();
    }

    /// <summary>
    /// Called by a scanner after a token has sat in its zone long enough.
    /// </summary>
    public void NotifyRegisterScanned(InstructionRegisterRole role, RegisterToken registerToken)
    {
        if (registerToken == null || string.IsNullOrWhiteSpace(registerToken.RegisterId))
            return;

        RegisterScanned?.Invoke(role, registerToken.RegisterId);
    }

    /// <summary>
    /// Called by a register token when the player first grabs it.
    /// The lesson system reuses this as the current "selection" signal.
    /// </summary>
    public void NotifyRegisterGrabbed(RegisterToken registerToken)
    {
        if (registerToken == null || string.IsNullOrWhiteSpace(registerToken.RegisterId))
            return;

        if (HasRegisterScanners)
            return;

        RegisterPressed?.Invoke(registerToken.RegisterId);
    }
}

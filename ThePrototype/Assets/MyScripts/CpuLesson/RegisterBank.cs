using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Owns the curated physical register buttons used by the current lesson.
/// It creates buttons, tracks them by register name, and exposes a single
/// event when one is pressed.
/// </summary>
public class RegisterBank : MonoBehaviour
{
    [SerializeField]
    Vector2 m_ButtonGridSpacing = new(0.4f, 0.32f);

    readonly Dictionary<string, RegisterButton> m_RegisterButtons =
        new(StringComparer.OrdinalIgnoreCase);

    public event Action<string> RegisterPressed;

    /// <summary>
    /// Rebuilds the physical button set for the supplied register choices.
    /// </summary>
    public void BuildButtons(string[] registerChoices)
    {
        foreach (Transform child in transform)
            Destroy(child.gameObject);

        m_RegisterButtons.Clear();

        if (registerChoices == null || registerChoices.Length == 0)
            return;

        for (var i = 0; i < registerChoices.Length; i++)
        {
            var column = i % 3;
            var row = i / 3;
            var localPosition = new Vector3(
                (column - 1) * m_ButtonGridSpacing.x,
                0f,
                row * m_ButtonGridSpacing.y);

            var registerName = registerChoices[i];
            var button = ButtonFactory.CreateButton(
                parent: transform,
                objectName: $"Register Button {registerName}",
                localPosition: localPosition,
                label: registerName,
                buttonId: registerName,
                buttonKind: RegisterButton.ButtonKind.Register);

            button.Pressed += OnRegisterButtonPressed;
            m_RegisterButtons[registerName] = button;
        }
    }

    /// <summary>
    /// Restores every register button to its idle visuals.
    /// </summary>
    public void ResetVisuals()
    {
        foreach (var button in m_RegisterButtons.Values)
            button.ResetVisualState();
    }

    /// <summary>
    /// Marks a register button as selected.
    /// </summary>
    public void SetSelected(string registerName)
    {
        if (string.IsNullOrWhiteSpace(registerName))
            return;

        if (m_RegisterButtons.TryGetValue(registerName, out var button))
            button.SetSelected(true);
    }

    /// <summary>
    /// Shows a short failure flash on the named register button.
    /// </summary>
    public void FlashFailure(string registerName)
    {
        if (string.IsNullOrWhiteSpace(registerName))
            return;

        if (m_RegisterButtons.TryGetValue(registerName, out var button))
            button.FlashFailure();
    }

    void OnRegisterButtonPressed(RegisterButton button)
    {
        if (button == null || button.buttonKind != RegisterButton.ButtonKind.Register)
            return;

        RegisterPressed?.Invoke(button.ButtonId);
    }
}

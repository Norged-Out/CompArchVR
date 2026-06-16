using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// Small XR button used by the lesson runtime.
/// It only manages visuals plus the press event for one button.
/// </summary>
public class RegisterButton : MonoBehaviour
{
    public enum ButtonKind
    {
        Register,
        Reset,
    }

    [SerializeField]
    ButtonKind m_ButtonKind = ButtonKind.Register;

    [SerializeField]
    string m_ButtonId = string.Empty;

    [SerializeField]
    Transform m_ButtonCap;

    [SerializeField]
    Renderer m_BaseRenderer;

    [SerializeField]
    Renderer m_CapRenderer;

    [SerializeField]
    TMP_Text m_LabelText;

    [SerializeField]
    Color m_IdleColor = new(0.72f, 0.28f, 0.28f, 1f);

    [SerializeField]
    Color m_SelectedColor = new(0.25f, 0.75f, 0.35f, 1f);

    [SerializeField]
    Color m_ErrorColor = new(0.85f, 0.2f, 0.2f, 1f);

    [SerializeField]
    float m_PressedOffset = 0.025f;

    XRSimpleInteractable m_Interactable;
    Vector3 m_OriginalCapLocalPosition;
    bool m_IsSelected;
    Coroutine m_FlashRoutine;

    public event Action<RegisterButton> Pressed;

    public string ButtonId => m_ButtonId;
    public ButtonKind buttonKind => m_ButtonKind;

    /// <summary>
    /// Populates the runtime-only button metadata and visuals.
    /// </summary>
    public void Initialize(
        string buttonLabel,
        string buttonId,
        ButtonKind buttonKind,
        Transform buttonCap,
        Renderer baseRenderer,
        Renderer capRenderer,
        TMP_Text labelText)
    {
        m_ButtonKind = buttonKind;
        m_ButtonId = buttonId;
        m_ButtonCap = buttonCap;
        m_BaseRenderer = baseRenderer;
        m_CapRenderer = capRenderer;
        m_LabelText = labelText;

        if (m_LabelText != null)
            m_LabelText.text = buttonLabel;

        if (m_ButtonCap != null)
            m_OriginalCapLocalPosition = m_ButtonCap.localPosition;

        RefreshVisualState();
    }

    void Awake()
    {
        m_Interactable = GetComponent<XRSimpleInteractable>();
        if (m_ButtonCap != null)
            m_OriginalCapLocalPosition = m_ButtonCap.localPosition;
    }

    void OnEnable()
    {
        m_Interactable ??= GetComponent<XRSimpleInteractable>();
        if (m_Interactable == null)
            return;

        m_Interactable.firstSelectEntered.AddListener(OnFirstSelectEntered);
        m_Interactable.lastSelectExited.AddListener(OnLastSelectExited);
    }

    void OnDisable()
    {
        if (m_Interactable == null)
            return;

        m_Interactable.firstSelectEntered.RemoveListener(OnFirstSelectEntered);
        m_Interactable.lastSelectExited.RemoveListener(OnLastSelectExited);
    }

    /// <summary>
    /// Keeps successful selections visible for the current stage.
    /// </summary>
    public void SetSelected(bool isSelected)
    {
        m_IsSelected = isSelected;
        RefreshVisualState();
    }

    /// <summary>
    /// Restores the button to its idle visuals.
    /// </summary>
    public void ResetVisualState()
    {
        m_IsSelected = false;
        if (m_ButtonCap != null)
            m_ButtonCap.localPosition = m_OriginalCapLocalPosition;

        RefreshVisualState();
    }

    /// <summary>
    /// Shows a brief red failure flash.
    /// </summary>
    public void FlashFailure()
    {
        if (m_FlashRoutine != null)
            StopCoroutine(m_FlashRoutine);

        m_FlashRoutine = StartCoroutine(FlashFailureRoutine());
    }

    void OnFirstSelectEntered(SelectEnterEventArgs _)
    {
        if (m_ButtonCap != null)
            m_ButtonCap.localPosition = m_OriginalCapLocalPosition + Vector3.down * m_PressedOffset;

        Pressed?.Invoke(this);
    }

    void OnLastSelectExited(SelectExitEventArgs _)
    {
        if (m_ButtonCap != null)
            m_ButtonCap.localPosition = m_OriginalCapLocalPosition;
    }

    IEnumerator FlashFailureRoutine()
    {
        ApplyCapColor(m_ErrorColor);
        yield return new WaitForSeconds(0.25f);
        RefreshVisualState();
        m_FlashRoutine = null;
    }

    void RefreshVisualState()
    {
        ApplyCapColor(m_IsSelected ? m_SelectedColor : m_IdleColor);
    }

    void ApplyCapColor(Color color)
    {
        if (m_CapRenderer != null)
            m_CapRenderer.material.color = color;

        if (m_BaseRenderer != null)
            m_BaseRenderer.material.color = color * 0.65f;
    }
}

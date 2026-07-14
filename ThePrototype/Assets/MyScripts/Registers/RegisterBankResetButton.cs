using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// Small bridge component for the authored register-bank reset button.
/// The button only snaps the register pieces home; it does not reset the lesson.
/// </summary>
[DisallowMultipleComponent]
public class RegisterBankResetButton : MonoBehaviour
{
    [SerializeField]
    RegisterBank m_RegisterBank;

    [SerializeField]
    XRSimpleInteractable m_Interactable;

    void Awake()
    {
        CacheReferences();
    }

    void OnEnable()
    {
        CacheReferences();

        if (m_Interactable == null)
            return;

        m_Interactable.firstSelectEntered.AddListener(OnFirstSelectEntered);
    }

    void OnDisable()
    {
        if (m_Interactable == null)
            return;

        m_Interactable.firstSelectEntered.RemoveListener(OnFirstSelectEntered);
    }

    /// <summary>
    /// Lets the authoring helper assign the scene-owned register bank reference.
    /// </summary>
    public void Configure(RegisterBank registerBank)
    {
        m_RegisterBank = registerBank;
        CacheReferences();
    }

    /// <summary>
    /// Shared reset entry point used by both the authored XR button and any
    /// future UI button that should snap the register pieces back home.
    /// </summary>
    public void TriggerReset()
    {
        m_RegisterBank?.ResetRegisterPositionsOnly();
    }

    void OnFirstSelectEntered(SelectEnterEventArgs _)
    {
        TriggerReset();
    }

    void CacheReferences()
    {
        m_Interactable ??= GetComponent<XRSimpleInteractable>();
        m_RegisterBank ??= GetComponentInParent<RegisterBank>();
    }
}

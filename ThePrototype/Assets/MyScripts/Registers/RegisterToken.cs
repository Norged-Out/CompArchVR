using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// One physical register piece in the authored register bank.
///
/// The bank owns layout and reset, while this component owns:
/// - register identity (`$t0`, `$s1`, etc.)
/// - grab notifications for the lesson flow
/// - the home pose used by the local reset button
/// </summary>
[DisallowMultipleComponent]
public class RegisterToken : MonoBehaviour
{
    [SerializeField]
    string m_RegisterId = "t0";

    [SerializeField]
    string m_DisplayLabel = "$t0";

    [SerializeField]
    int m_RegisterValue;

    [SerializeField]
    RegisterBank m_OwningBank;

    [Header("Scene References")]

    [SerializeField]
    XRGrabInteractable m_GrabInteractable;

    [SerializeField]
    Rigidbody m_Rigidbody;

    [SerializeField]
    Renderer m_BaseRenderer;

    [SerializeField]
    Renderer m_BodyRenderer;

    [SerializeField]
    TMP_Text m_LabelText;

    [SerializeField]
    TMP_Text m_ValueText;

    [Header("Home Pose")]

    [SerializeField]
    Transform m_HomeParent;

    [SerializeField]
    Vector3 m_HomeLocalPosition;

    [SerializeField]
    Vector3 m_HomeLocalEulerAngles;

    [SerializeField]
    Vector3 m_HomeLocalScale = Vector3.one;

    public string RegisterId => m_RegisterId;
    public string DisplayLabel => m_DisplayLabel;
    public int RegisterValue => m_RegisterValue;
    public bool IsGrabbed => m_GrabInteractable != null && m_GrabInteractable.isSelected;

    void Awake()
    {
        CacheReferences();
        RefreshText();
    }

    void OnEnable()
    {
        CacheReferences();

        if (m_GrabInteractable == null)
            return;

        m_GrabInteractable.firstHoverEntered.AddListener(OnFirstHoverEntered);
        m_GrabInteractable.lastHoverExited.AddListener(OnLastHoverExited);
        m_GrabInteractable.firstSelectEntered.AddListener(OnFirstSelectEntered);
        m_GrabInteractable.lastSelectExited.AddListener(OnLastSelectExited);
    }

    void OnDisable()
    {
        if (m_GrabInteractable == null)
            return;

        m_GrabInteractable.firstHoverEntered.RemoveListener(OnFirstHoverEntered);
        m_GrabInteractable.lastHoverExited.RemoveListener(OnLastHoverExited);
        m_GrabInteractable.firstSelectEntered.RemoveListener(OnFirstSelectEntered);
        m_GrabInteractable.lastSelectExited.RemoveListener(OnLastSelectExited);
    }

    /// <summary>
    /// Assigns the logical register identity and updates the visible label.
    /// </summary>
    public void Configure(string registerId, string displayLabel)
    {
        m_RegisterId = registerId;
        m_DisplayLabel = displayLabel;
        RefreshText();
    }

    /// <summary>
    /// Lets the authoring script explicitly bind the key visual references after
    /// it rebuilds the token presentation in the editor.
    /// </summary>
    public void BindSceneReferences(
        XRGrabInteractable grabInteractable,
        Rigidbody rigidbody,
        Renderer baseRenderer,
        Renderer bodyRenderer,
        TMP_Text labelText,
        TMP_Text valueText = null)
    {
        m_GrabInteractable = grabInteractable;
        m_Rigidbody = rigidbody;
        m_BaseRenderer = baseRenderer;
        m_BodyRenderer = bodyRenderer;
        m_LabelText = labelText;
        m_ValueText = valueText;

        RefreshText();
    }

    /// <summary>
    /// Updates the stored register value without touching pose or lesson state.
    /// </summary>
    public void SetRegisterValue(int registerValue)
    {
        m_RegisterValue = registerValue;
        UpdateValueText();
    }

    /// <summary>
    /// Resets only the logical register value back to zero.
    /// This is separate from the physical bank reset button on purpose.
    /// </summary>
    public void ResetRegisterValue()
    {
        SetRegisterValue(0);
    }

    /// <summary>
    /// Stores the authored home pose used by the bank reset button.
    /// </summary>
    public void CaptureHomePose(Transform homeParent)
    {
        m_HomeParent = homeParent;
        m_HomeLocalPosition = transform.localPosition;
        m_HomeLocalEulerAngles = transform.localEulerAngles;
        m_HomeLocalScale = transform.localScale;
    }

    /// <summary>
    /// Rebinds the scene-owned bank after scene reloads or edit-time rebuilds.
    /// </summary>
    public void SetOwningBank(RegisterBank owningBank)
    {
        m_OwningBank = owningBank;
    }

    /// <summary>
    /// Compatibility hook kept so existing lesson code still compiles.
    /// Register visuals are now authored directly instead of recolored here.
    /// </summary>
    public void SetSelected(bool isSelected)
    {
        // Intentionally left blank.
    }

    /// <summary>
    /// Compatibility hook kept so existing lesson code still compiles.
    /// </summary>
    public void ResetVisualState()
    {
        // Intentionally left blank.
    }

    /// <summary>
    /// Compatibility hook kept so existing lesson code still compiles.
    /// </summary>
    public void FlashFailure()
    {
        // Intentionally left blank.
    }

    /// <summary>
    /// Snaps the token back to its authored home pose unless it is still held.
    /// </summary>
    public void ResetToHome()
    {
        if (m_HomeParent == null)
            return;

        if (m_GrabInteractable != null && m_GrabInteractable.isSelected)
            return;

        transform.SetParent(m_HomeParent, true);
        transform.localPosition = m_HomeLocalPosition;
        transform.localEulerAngles = m_HomeLocalEulerAngles;
        transform.localScale = m_HomeLocalScale;

        if (m_Rigidbody != null)
        {
            m_Rigidbody.linearVelocity = Vector3.zero;
            m_Rigidbody.angularVelocity = Vector3.zero;
            m_Rigidbody.Sleep();
        }
    }

    void OnFirstHoverEntered(HoverEnterEventArgs _)
    {
        // Visual styling is authored directly on the prefab now.
    }

    void OnLastHoverExited(HoverExitEventArgs _)
    {
        // Visual styling is authored directly on the prefab now.
    }

    void OnFirstSelectEntered(SelectEnterEventArgs _)
    {
        m_OwningBank?.NotifyRegisterGrabbed(this);
    }

    void OnLastSelectExited(SelectExitEventArgs _)
    {
        // Visual styling is authored directly on the prefab now.
    }

    void CacheReferences()
    {
        m_GrabInteractable ??= GetComponent<XRGrabInteractable>();
        m_Rigidbody ??= GetComponent<Rigidbody>();

        if (m_BaseRenderer == null)
        {
            var baseTransform = transform.Find("Visuals/Base");
            if (baseTransform != null)
                m_BaseRenderer = baseTransform.GetComponent<Renderer>();
        }

        if (m_BodyRenderer == null)
        {
            var bodyTransform = transform.Find("Visuals/Body");
            if (bodyTransform != null)
                m_BodyRenderer = bodyTransform.GetComponent<Renderer>();
        }

        if (m_LabelText == null || m_ValueText == null)
        {
            foreach (var textMesh in GetComponentsInChildren<TextMeshPro>(true))
            {
                if (textMesh == null)
                    continue;

                if (m_LabelText == null)
                {
                    m_LabelText = textMesh;
                    continue;
                }

                if (m_ValueText == null && textMesh != m_LabelText)
                    m_ValueText = textMesh;
            }
        }

        m_OwningBank ??= GetComponentInParent<RegisterBank>();
    }

    void RefreshText()
    {
        UpdateLabel();
        UpdateValueText();
    }

    void UpdateLabel()
    {
        if (m_LabelText != null)
            m_LabelText.text = m_DisplayLabel;
    }

    void UpdateValueText()
    {
        if (m_ValueText != null)
            m_ValueText.text = m_RegisterValue.ToString();
    }

}

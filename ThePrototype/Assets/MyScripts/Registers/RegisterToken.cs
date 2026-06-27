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
/// - visual state changes for hover, success, and failure
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

    [Header("Home Pose")]

    [SerializeField]
    Transform m_HomeParent;

    [SerializeField]
    Vector3 m_HomeLocalPosition;

    [SerializeField]
    Vector3 m_HomeLocalEulerAngles;

    [SerializeField]
    Vector3 m_HomeLocalScale = Vector3.one;

    [Header("Visual Theme")]

    [SerializeField]
    Color m_IdleBaseColor = new(0.21f, 0.12f, 0.12f, 1f);

    [SerializeField]
    Color m_IdleBodyColor = new(0.72f, 0.28f, 0.28f, 1f);

    [SerializeField]
    Color m_HoverBodyColor = new(0.98f, 0.8f, 0.24f, 1f);

    [SerializeField]
    Color m_SelectedBodyColor = new(0.27f, 0.77f, 0.4f, 1f);

    [SerializeField]
    Color m_ErrorBodyColor = new(0.9f, 0.2f, 0.2f, 1f);

    static readonly int k_BaseColorId = Shader.PropertyToID("_BaseColor");
    static readonly int k_ColorId = Shader.PropertyToID("_Color");

    // Keep per-renderer runtime materials so our lesson colors do not fight the
    // XR affordance system, which writes its own property block for rim glow.
    Material m_BaseRuntimeMaterial;
    Material m_BodyRuntimeMaterial;
    Coroutine m_FlashRoutine;
    bool m_IsHovered;
    bool m_IsGrabbed;
    bool m_IsLessonSelected;

    public string RegisterId => m_RegisterId;
    public string DisplayLabel => m_DisplayLabel;
    public bool IsGrabbed => m_GrabInteractable != null && m_GrabInteractable.isSelected;

    void Awake()
    {
        CacheReferences();
        UpdateLabel();
        ApplyCurrentVisualState();
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
        UpdateLabel();
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
        TMP_Text labelText)
    {
        m_GrabInteractable = grabInteractable;
        m_Rigidbody = rigidbody;
        m_BaseRenderer = baseRenderer;
        m_BodyRenderer = bodyRenderer;
        m_LabelText = labelText;

        UpdateLabel();
        ApplyCurrentVisualState();
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
    /// Keeps successful lesson selections visible after the player lets go.
    /// </summary>
    public void SetSelected(bool isSelected)
    {
        m_IsLessonSelected = isSelected;
        ApplyCurrentVisualState();
    }

    /// <summary>
    /// Clears lesson-driven visual state without moving the token.
    /// </summary>
    public void ResetVisualState()
    {
        m_IsLessonSelected = false;
        ApplyCurrentVisualState();
    }

    /// <summary>
    /// Briefly flashes the token red when the learner chooses the wrong register.
    /// </summary>
    public void FlashFailure()
    {
        if (m_FlashRoutine != null)
            StopCoroutine(m_FlashRoutine);

        m_FlashRoutine = StartCoroutine(FlashFailureRoutine());
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

    IEnumerator FlashFailureRoutine()
    {
        ApplyTheme(m_IdleBaseColor, m_ErrorBodyColor, m_ErrorBodyColor);
        yield return new WaitForSeconds(0.25f);

        m_FlashRoutine = null;
        ApplyCurrentVisualState();
    }

    void OnFirstHoverEntered(HoverEnterEventArgs _)
    {
        m_IsHovered = true;
        ApplyCurrentVisualState();
    }

    void OnLastHoverExited(HoverExitEventArgs _)
    {
        m_IsHovered = false;
        ApplyCurrentVisualState();
    }

    void OnFirstSelectEntered(SelectEnterEventArgs _)
    {
        m_IsGrabbed = true;
        ApplyCurrentVisualState();
        m_OwningBank?.NotifyRegisterGrabbed(this);
    }

    void OnLastSelectExited(SelectExitEventArgs _)
    {
        m_IsGrabbed = false;
        ApplyCurrentVisualState();
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

        m_LabelText ??= GetComponentInChildren<TextMeshPro>(true);
        m_OwningBank ??= GetComponentInParent<RegisterBank>();
    }

    void UpdateLabel()
    {
        if (m_LabelText != null)
            m_LabelText.text = m_DisplayLabel;
    }

    void ApplyCurrentVisualState()
    {
        if (m_FlashRoutine != null)
            return;

        var bodyColor = m_IdleBodyColor;
        if (m_IsLessonSelected)
            bodyColor = m_SelectedBodyColor;
        else if (m_IsGrabbed || m_IsHovered)
            bodyColor = m_HoverBodyColor;

        var labelColor = m_IsLessonSelected ? m_SelectedBodyColor : Color.white;
        ApplyTheme(m_IdleBaseColor, bodyColor, labelColor);
    }

    void ApplyTheme(Color baseColor, Color bodyColor, Color labelColor)
    {
        ApplyRendererColor(m_BaseRenderer, ref m_BaseRuntimeMaterial, baseColor);
        ApplyRendererColor(m_BodyRenderer, ref m_BodyRuntimeMaterial, bodyColor);

        if (m_LabelText != null)
            m_LabelText.color = labelColor;
    }

    static void ApplyRendererColor(Renderer targetRenderer, ref Material runtimeMaterial, Color color)
    {
        if (targetRenderer == null)
            return;

        runtimeMaterial ??= targetRenderer.material;

        if (runtimeMaterial != null && runtimeMaterial.HasProperty(k_BaseColorId))
            runtimeMaterial.SetColor(k_BaseColorId, color);
        else
            runtimeMaterial?.SetColor(k_ColorId, color);
    }
}

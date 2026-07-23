using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Physical lesson object that carries one instruction through the early
/// datapath flow. It owns:
/// - the currently loaded <see cref="InstructionDefinition"/>
/// - one optional display text
/// - simple visual state changes for idle / uploaded / downloaded / grabbed
/// </summary>
[DisallowMultipleComponent]
public class InstructionModule : MonoBehaviour
{
    public enum ModuleVisualState
    {
        Idle,
        Uploaded,
        Downloaded,
        Grabbed,
    }

    [Header("Scene References")]
    [SerializeField]
    TMP_Text m_DisplayText;

    [SerializeField]
    Renderer m_BodyRenderer;

    [SerializeField]
    UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable m_GrabInteractable;

    [SerializeField]
    Rigidbody m_Rigidbody;

    [Header("Display")]
    [SerializeField]
    string m_DefaultLabel = "Instruction";

    [Header("Materials")]
    [SerializeField]
    Material m_IdleMaterial;

    [SerializeField]
    Material m_UploadedMaterial;

    [SerializeField]
    Material m_DownloadedMaterial;

    [SerializeField]
    Material m_GrabbedMaterial;

    InstructionDefinition m_CurrentInstruction;
    string m_DisplayOverrideText;
    Material m_InitialBodyMaterial;
    bool m_IsDownloaded;
    bool m_IsGrabbed;

    public InstructionDefinition CurrentInstruction => m_CurrentInstruction;
    public bool HasInstruction => m_CurrentInstruction != null;
    public bool IsDownloaded => m_IsDownloaded;
    public bool IsGrabbed => m_IsGrabbed;
    public string CurrentDisplayText => HasInstruction && !string.IsNullOrWhiteSpace(m_CurrentInstruction.assemblyInstructionText)
        ? !string.IsNullOrWhiteSpace(m_DisplayOverrideText)
            ? m_DisplayOverrideText
            : m_CurrentInstruction.assemblyInstructionText
        : m_DefaultLabel;

    void Awake()
    {
        CacheReferences();
        RefreshPresentation();
    }

    void OnEnable()
    {
        CacheReferences();
        HookGrabEvents(true);
        RefreshPresentation();
    }

    void OnDisable()
    {
        HookGrabEvents(false);
    }

    void OnValidate()
    {
        CacheReferences();
        RefreshPresentation();
    }

    public void ClearInstruction()
    {
        m_CurrentInstruction = null;
        m_DisplayOverrideText = string.Empty;
        m_IsDownloaded = false;
        RefreshPresentation();
    }

    public void UploadInstruction(InstructionDefinition instruction, string displayOverrideText = null)
    {
        m_CurrentInstruction = instruction;
        m_DisplayOverrideText = displayOverrideText;
        m_IsDownloaded = false;
        RefreshPresentation();
    }

    public void MarkDownloaded()
    {
        m_IsDownloaded = true;
        RefreshPresentation();
    }

    public void SetGrabEnabled(bool isEnabled)
    {
        if (m_GrabInteractable != null)
            m_GrabInteractable.enabled = isEnabled;
    }

    public void SnapToAnchor(Transform anchor, bool lockInPlace)
    {
        if (anchor == null)
            return;

        // Terminals use authored anchors so the module always lands in a tidy,
        // presentation-friendly pose instead of keeping whatever grab offset it
        // had when the learner last released it.
        transform.SetParent(anchor, false);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        if (m_Rigidbody != null)
        {
            m_Rigidbody.linearVelocity = Vector3.zero;
            m_Rigidbody.angularVelocity = Vector3.zero;
            m_Rigidbody.isKinematic = lockInPlace;
            m_Rigidbody.useGravity = !lockInPlace;
        }

        if (lockInPlace)
        {
            m_IsGrabbed = false;
            SetGrabEnabled(false);
        }

        RefreshPresentation();
    }

    public void ReleaseFromAnchor(Transform newParent = null)
    {
        transform.SetParent(newParent, true);

        if (m_Rigidbody != null)
        {
            m_Rigidbody.isKinematic = false;
            m_Rigidbody.useGravity = true;
        }

        SetGrabEnabled(true);
        RefreshPresentation();
    }

    void CacheReferences()
    {
        if (m_BodyRenderer == null)
        {
            var bodyTransform = FindChildTransform("Body");
            if (bodyTransform != null)
                m_BodyRenderer = bodyTransform.GetComponent<Renderer>();
        }

        if (m_BodyRenderer != null && m_InitialBodyMaterial == null)
            m_InitialBodyMaterial = m_BodyRenderer.sharedMaterial;

        m_GrabInteractable ??= GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        m_Rigidbody ??= GetComponent<Rigidbody>();
    }

    void HookGrabEvents(bool subscribe)
    {
        if (m_GrabInteractable == null)
            return;

        if (subscribe)
        {
            m_GrabInteractable.selectEntered.RemoveListener(HandleSelectEntered);
            m_GrabInteractable.selectEntered.AddListener(HandleSelectEntered);
            m_GrabInteractable.selectExited.RemoveListener(HandleSelectExited);
            m_GrabInteractable.selectExited.AddListener(HandleSelectExited);
            return;
        }

        m_GrabInteractable.selectEntered.RemoveListener(HandleSelectEntered);
        m_GrabInteractable.selectExited.RemoveListener(HandleSelectExited);
    }

    void HandleSelectEntered(SelectEnterEventArgs _)
    {
        m_IsGrabbed = true;
        RefreshPresentation();
    }

    void HandleSelectExited(SelectExitEventArgs _)
    {
        m_IsGrabbed = false;
        RefreshPresentation();
    }

    void RefreshPresentation()
    {
        if (m_DisplayText != null)
            m_DisplayText.text = CurrentDisplayText;

        if (m_BodyRenderer != null)
            m_BodyRenderer.sharedMaterial = ResolveVisualMaterial();
    }

    Material ResolveVisualMaterial()
    {
        if (m_IsGrabbed && m_GrabbedMaterial != null)
            return m_GrabbedMaterial;

        if (m_IsDownloaded && m_DownloadedMaterial != null)
            return m_DownloadedMaterial;

        if (HasInstruction && m_UploadedMaterial != null)
            return m_UploadedMaterial;

        if (m_IdleMaterial != null)
            return m_IdleMaterial;

        return m_InitialBodyMaterial;
    }

    Transform FindChildTransform(string childName)
    {
        foreach (var childTransform in GetComponentsInChildren<Transform>(true))
        {
            if (childTransform == null || childTransform == transform)
                continue;

            if (childTransform.name == childName)
                return childTransform;
        }

        return null;
    }
}

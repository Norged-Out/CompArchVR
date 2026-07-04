using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// One addressable word entry inside the Data Memory bank.
/// Each entry stores its address/value pair and can briefly surface that data
/// to the central memory display when hovered.
/// </summary>
[DisallowMultipleComponent]
public class MemoryWord : MonoBehaviour
{
    [SerializeField]
    int m_Address;

    [SerializeField]
    int m_StoredValue;

    [SerializeField]
    string m_DisplayAddress = string.Empty;

    [SerializeField]
    string m_DisplayData = string.Empty;

    [Header("Scene References")]
    [SerializeField]
    Renderer m_Renderer;

    [SerializeField]
    int m_MaterialIndex;

    [SerializeField]
    XRSimpleInteractable m_Interactable;

    [SerializeField]
    TMP_Text m_LabelText;

    [Header("Materials")]
    [SerializeField]
    Material m_IdleMaterial;

    [SerializeField]
    Material m_HoverMaterial;

    [SerializeField]
    Material m_HighlightMaterial;

    DataMemoryBank m_OwningBank;
    bool m_IsHovered;
    bool m_IsHighlighted;

    public int Address => m_Address;
    public int StoredValue => m_StoredValue;
    public string AddressDisplay => string.IsNullOrWhiteSpace(m_DisplayAddress) ? FormatAddress(m_Address) : m_DisplayAddress;
    public string DataDisplay => string.IsNullOrWhiteSpace(m_DisplayData) ? m_StoredValue.ToString() : m_DisplayData;

    void Awake()
    {
        CacheReferences();
        RefreshLocalLabel();
        RefreshMaterial();
    }

    void OnEnable()
    {
        CacheReferences();

        if (m_Interactable == null)
            return;

        m_Interactable.firstHoverEntered.RemoveListener(HandleHoverEntered);
        m_Interactable.firstHoverEntered.AddListener(HandleHoverEntered);
        m_Interactable.lastHoverExited.RemoveListener(HandleHoverExited);
        m_Interactable.lastHoverExited.AddListener(HandleHoverExited);
    }

    void OnDisable()
    {
        if (m_Interactable == null)
            return;

        m_Interactable.firstHoverEntered.RemoveListener(HandleHoverEntered);
        m_Interactable.lastHoverExited.RemoveListener(HandleHoverExited);
    }

    public void SetOwningBank(DataMemoryBank owningBank)
    {
        m_OwningBank = owningBank;
    }

    public void SetStoredValue(int storedValue)
    {
        m_StoredValue = storedValue;
        RefreshLocalLabel();
    }

    public void SetAddress(int address)
    {
        m_Address = address;
        RefreshLocalLabel();
    }

    public void SetAddressHighlighted(bool isHighlighted)
    {
        m_IsHighlighted = isHighlighted;
        RefreshMaterial();
    }

    void HandleHoverEntered(HoverEnterEventArgs _)
    {
        m_IsHovered = true;
        RefreshMaterial();

        if (m_OwningBank == null || m_OwningBank.ShouldAllowHoverPreview())
            m_OwningBank?.ShowWordDetails(this);
    }

    void HandleHoverExited(HoverExitEventArgs _)
    {
        m_IsHovered = false;
        RefreshMaterial();
        m_OwningBank?.HandleWordHoverExited(this);
    }

    void CacheReferences()
    {
        m_Renderer ??= GetComponent<Renderer>();
        m_Interactable ??= GetComponent<XRSimpleInteractable>();
        m_OwningBank ??= GetComponentInParent<DataMemoryBank>();
    }

    void RefreshLocalLabel()
    {
        if (m_LabelText != null)
            m_LabelText.text = AddressDisplay;
    }

    void RefreshMaterial()
    {
        if (m_Renderer == null)
            return;

        var materialToUse = m_IsHighlighted
            ? m_HighlightMaterial
            : m_IsHovered
                ? m_HoverMaterial
                : m_IdleMaterial;

        if (materialToUse == null)
            return;

        var materials = m_Renderer.sharedMaterials;
        if (materials == null || materials.Length == 0)
            return;

        if (m_MaterialIndex < 0 || m_MaterialIndex >= materials.Length)
            return;

        materials[m_MaterialIndex] = materialToUse;
        m_Renderer.sharedMaterials = materials;
    }

    static string FormatAddress(int value)
    {
        return $"0x{value:X8}";
    }
}

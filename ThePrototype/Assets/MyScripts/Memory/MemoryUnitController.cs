using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Owns the authored Memory Unit prefab and the Mem UI.
/// For the current vertical slice it primarily handles:
/// - `lw` memory reads with ALU-result address input
/// - `sw` memory writes with ALU-result address input plus store-data packet
/// - MemRead / MemWrite control validation
/// - highlighting the addressed word in the Data Memory bank
/// - spawning the Memory Data packet used by write-back
/// </summary>
[DisallowMultipleComponent]
public partial class MemoryUnitController : MonoBehaviour
{
    [Header("Memory Unit Prefab")]
    [SerializeField]
    MemoryAddressScanner m_AddressScanner;

    [SerializeField]
    MemoryPacketScanner m_DataScanner;

    [SerializeField]
    Transform m_MemoryDataSpawnTransform;

    [SerializeField]
    DataPacketToken m_MemoryDataPacketPrefab;

    [SerializeField]
    DataMemoryBank m_MemoryBank;

    [Header("Physical Buttons")]
    [SerializeField]
    Transform m_MemReadButtonRoot;

    [SerializeField]
    Transform m_MemWriteButtonRoot;

    [Header("Mem UI")]
    [SerializeField]
    GameObject m_MemUiRoot;

    [SerializeField]
    TMP_Text m_LessonRuntimeText;

    [SerializeField]
    TMP_Text m_LoadLessonText;

    [SerializeField]
    TMP_Text m_StoreLessonText;

    [SerializeField]
    TMP_Text m_MemReadStatusText;

    [SerializeField]
    TMP_Text m_MemWriteStatusText;

    [SerializeField]
    TMP_Text m_AddressStatusText;

    [SerializeField]
    TMP_Text m_DataStatusText;

    [SerializeField]
    TMP_Text m_FeedbackText;

    [SerializeField]
    Button m_ActionButton;

    [SerializeField]
    TMP_Text m_ActionButtonLabel;

    [SerializeField]
    TMP_Dropdown m_HintDropdown;

    [SerializeField]
    TMP_Text m_HintMemReadText;

    [SerializeField]
    TMP_Text m_HintMemWriteText;

    [Header("Timing")]
    [SerializeField]
    float m_DataSpawnDelaySeconds = 0.75f;

    [SerializeField]
    string m_ExecuteButtonText = "Execute Memory";

    [SerializeField]
    string m_ContinueButtonText = "Continue";

    [SerializeField]
    Color m_SuccessFeedbackColor = new(0.78f, 0.96f, 0.82f, 1f);

    [SerializeField]
    Color m_FailureFeedbackColor = new(1f, 0.55f, 0.55f, 1f);

    InstructionDefinition m_CurrentInstruction;
    Coroutine m_ExecutionRoutine;
    DataPacketToken m_SpawnedMemoryPacket;
    bool m_IsPhaseActive;
    bool m_IsAwaitingContinue;
    bool m_HasCompletedMemoryAccess;
    int m_LastAddress;
    int m_LastLoadedValue;
    string m_MemReadValue = "0";
    string m_MemWriteValue = "0";

    public event System.Action ContinueRequested;

    void Awake()
    {
        CacheReferences();
        PopulateHintDropdown();
        HookActionButton(true);
        HookButtons();
        HookHintDropdown(true);
        HookScannerEvents(true);
        RefreshPresentation();
        SetFeedback(string.Empty, false);
    }

    void OnEnable()
    {
        CacheReferences();
        PopulateHintDropdown();
        HookActionButton(true);
        HookButtons();
        HookHintDropdown(true);
        HookScannerEvents(true);
        RefreshPresentation();
    }

    void OnDisable()
    {
        HookActionButton(false);
        HookHintDropdown(false);
        HookScannerEvents(false);
        UnhookButtons();
    }

    public void SetPhaseState(bool isActive, InstructionDefinition instruction)
    {
        CacheReferences();

        var instructionChanged = instruction != null && instruction != m_CurrentInstruction;
        var isEnteringPhase = isActive && !m_IsPhaseActive;

        m_IsPhaseActive = isActive;
        m_CurrentInstruction = instruction != null ? instruction : InstructionDefaults.CreateFallbackAdd();

        if (isEnteringPhase || instructionChanged)
            PrepareForMemoryStep();

        if (m_MemUiRoot != null)
            m_MemUiRoot.SetActive(isActive);

        var usesInteractiveMemory = isActive && UsesInteractiveMemory();
        m_AddressScanner?.SetActive(usesInteractiveMemory);
        m_DataScanner?.SetActive(usesInteractiveMemory && RequiresDataInput());
        m_MemoryBank?.SetPhaseState(isActive, usesInteractiveMemory);
        RefreshPresentation();
    }

    public void ResetMemoryState()
    {
        if (m_ExecutionRoutine != null)
        {
            StopCoroutine(m_ExecutionRoutine);
            m_ExecutionRoutine = null;
        }

        m_CurrentInstruction = null;
        m_IsPhaseActive = false;
        m_IsAwaitingContinue = false;
        m_HasCompletedMemoryAccess = false;
        m_LastAddress = 0;
        m_LastLoadedValue = 0;
        m_MemReadValue = "0";
        m_MemWriteValue = "0";

        m_AddressScanner?.ResetScanner();
        m_DataScanner?.ResetScanner();
        m_MemoryBank?.StopAllAnimations();
        m_MemoryBank?.SetPhaseState(false, false);
        SetFeedback(string.Empty, false);
        RefreshPresentation();

        if (m_MemUiRoot != null)
            m_MemUiRoot.SetActive(false);
    }

    public void HandleActionPressed()
    {
        if (!m_IsPhaseActive || m_ExecutionRoutine != null)
            return;

        if (!UsesInteractiveMemory())
        {
            ContinueRequested?.Invoke();
            return;
        }

        if (m_IsAwaitingContinue)
        {
            m_IsAwaitingContinue = false;
            ContinueRequested?.Invoke();
            return;
        }

        if (!TryValidateSetup(out var validationMessage))
        {
            SetFeedback(validationMessage, true);
            RefreshPresentation();
            return;
        }

        SetFeedback("Memory access confirmed. Performing the transfer...", false);
        RefreshPresentation();
        m_ExecutionRoutine = StartCoroutine(ExecuteMemoryRoutine());
    }
}

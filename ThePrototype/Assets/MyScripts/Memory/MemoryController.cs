using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Owns the authored Memory Unit station and the Mem UI used during the
/// memory-access phase.
/// </summary>
[DisallowMultipleComponent]
public sealed class MemoryController : MonoBehaviour
{
    [Header("Memory Unit")]
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

    [Header("Labels")]
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
    MemoryTransferService m_TransferService;
    bool m_IsPhaseActive;
    bool m_IsAwaitingContinue;
    bool m_HasCompletedMemoryAccess;
    int m_LastAddress;
    int m_LastLoadedValue;
    string m_MemReadValue = "0";
    string m_MemWriteValue = "0";

    public event Action ContinueRequested;

    /// <summary>The instruction currently driving the Mem phase.</summary>
    public InstructionDefinition CurrentInstruction => m_CurrentInstruction;

    /// <summary>True while the memory station is the active lesson phase.</summary>
    public bool IsPhaseActive => m_IsPhaseActive;

    /// <summary>True after a successful memory transfer, when the button becomes Continue.</summary>
    public bool IsAwaitingContinue => m_IsAwaitingContinue;

    /// <summary>True once the Mem phase has already completed its authored transfer.</summary>
    public bool HasCompletedMemoryAccess => m_HasCompletedMemoryAccess;

    /// <summary>Most recent address consumed during memory access.</summary>
    public int LastAddress => m_LastAddress;

    /// <summary>Most recent value loaded from or stored into data memory.</summary>
    public int LastLoadedValue => m_LastLoadedValue;

    /// <summary>Current authored MemRead signal value.</summary>
    public string MemReadValue => m_MemReadValue;

    /// <summary>Current authored MemWrite signal value.</summary>
    public string MemWriteValue => m_MemWriteValue;

    /// <summary>Delay between bank transfer completion and Memory Data packet spawn.</summary>
    public float DataSpawnDelaySeconds => m_DataSpawnDelaySeconds;

    /// <summary>Address pedestal scanner bound in the scene.</summary>
    public MemoryAddressScanner AddressScanner => m_AddressScanner;

    /// <summary>Optional store-data pedestal scanner bound in the scene.</summary>
    public MemoryPacketScanner DataScanner => m_DataScanner;

    /// <summary>Spawn point used for load results.</summary>
    public Transform MemoryDataSpawnTransform => m_MemoryDataSpawnTransform;

    /// <summary>Prefab used to create the Memory Data packet for loads.</summary>
    public DataPacketToken MemoryDataPacketPrefab => m_MemoryDataPacketPrefab;

    /// <summary>Authored data-memory bank referenced by the memory station.</summary>
    public DataMemoryBank MemoryBank => m_MemoryBank;

    /// <summary>Currently running memory-transfer coroutine, if any.</summary>
    public Coroutine ExecutionRoutine => m_ExecutionRoutine;

    /// <summary>Last Memory Data packet spawned by a load instruction.</summary>
    public DataPacketToken SpawnedMemoryPacket => m_SpawnedMemoryPacket;

    /// <summary>Runtime lesson text field on the Mem UI.</summary>
    public TMP_Text LessonRuntimeText => m_LessonRuntimeText;
    public TMP_Text LoadLessonText => m_LoadLessonText;
    public TMP_Text StoreLessonText => m_StoreLessonText;
    public TMP_Text MemReadStatusText => m_MemReadStatusText;
    public TMP_Text MemWriteStatusText => m_MemWriteStatusText;
    public TMP_Text AddressStatusText => m_AddressStatusText;
    public TMP_Text DataStatusText => m_DataStatusText;
    public TMP_Text FeedbackText => m_FeedbackText;
    public Button ActionButton => m_ActionButton;
    public TMP_Text ActionButtonLabel => m_ActionButtonLabel;
    public TMP_Dropdown HintDropdown => m_HintDropdown;
    public TMP_Text HintMemReadText => m_HintMemReadText;
    public TMP_Text HintMemWriteText => m_HintMemWriteText;
    public string ExecuteButtonText => m_ExecuteButtonText;
    public string ContinueButtonText => m_ContinueButtonText;
    public GameObject MemUiRoot => m_MemUiRoot;

    void Awake()
    {
        m_TransferService = new MemoryTransferService();
        HookBindings(true);
        MemoryPresentation.PopulateHintDropdown(m_HintDropdown);
        RefreshPresentation();
        SetFeedback(string.Empty, false);
    }

    void OnEnable()
    {
        m_TransferService ??= new MemoryTransferService();
        HookBindings(true);
        MemoryPresentation.PopulateHintDropdown(m_HintDropdown);
        RefreshPresentation();
    }

    void OnDisable()
    {
        HookBindings(false);
    }

    /// <summary>
    /// Activates or deactivates the memory-access phase for the given
    /// instruction.
    /// </summary>
    public void SetPhaseState(bool isActive, InstructionDefinition instruction)
    {
        var instructionChanged = instruction != null && instruction != m_CurrentInstruction;
        var isEnteringPhase = isActive && !m_IsPhaseActive;

        m_IsPhaseActive = isActive;
        m_CurrentInstruction = instruction != null ? instruction : InstructionDefaults.CreateFallbackAdd();

        if (isEnteringPhase || instructionChanged)
            m_TransferService.PrepareForMemoryStep(this);

        if (m_MemUiRoot != null)
            m_MemUiRoot.SetActive(isActive);

        var usesInteractiveMemory = isActive && m_TransferService.UsesInteractiveMemory(m_CurrentInstruction);
        m_AddressScanner?.SetActive(usesInteractiveMemory);
        m_DataScanner?.SetActive(usesInteractiveMemory && m_TransferService.RequiresDataInput(m_CurrentInstruction));
        m_MemoryBank?.SetPhaseState(isActive, usesInteractiveMemory);
        RefreshPresentation();
    }

    /// <summary>
    /// Clears all runtime memory-phase state between lesson runs.
    /// </summary>
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
        m_TransferService?.ClearSpawnedMemoryPacket(this);
        SetFeedback(string.Empty, false);
        RefreshPresentation();

        if (m_MemUiRoot != null)
            m_MemUiRoot.SetActive(false);
    }

    /// <summary>
    /// Handles the authored execute / continue button for the memory phase.
    /// </summary>
    public void HandleActionPressed()
    {
        if (!m_IsPhaseActive || m_ExecutionRoutine != null)
            return;

        if (!m_TransferService.UsesInteractiveMemory(m_CurrentInstruction))
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

        if (!m_TransferService.TryValidateSetup(this, out var validationMessage))
        {
            SetFeedback(validationMessage, true);
            RefreshPresentation();
            return;
        }

        SetFeedback("Memory access confirmed. Performing the transfer...", false);
        RefreshPresentation();
        m_ExecutionRoutine = StartCoroutine(m_TransferService.RunTransferRoutine(this));
    }

    /// <summary>
    /// Toggles the authored MemRead physical button.
    /// </summary>
    public void HandleMemReadPressed(SelectEnterEventArgs _)
    {
        if (!m_IsPhaseActive || m_HasCompletedMemoryAccess || !m_TransferService.UsesInteractiveMemory(m_CurrentInstruction))
            return;

        m_TransferService.ToggleMemRead(this);
        RefreshPresentation();
    }

    /// <summary>
    /// Toggles the authored MemWrite physical button.
    /// </summary>
    public void HandleMemWritePressed(SelectEnterEventArgs _)
    {
        if (!m_IsPhaseActive || m_HasCompletedMemoryAccess || !m_TransferService.UsesInteractiveMemory(m_CurrentInstruction))
            return;

        m_TransferService.ToggleMemWrite(this);
        RefreshPresentation();
    }

    /// <summary>
    /// Refreshes the previewed bank word when the address pedestal accepts a
    /// packet.
    /// </summary>
    public void HandleAddressAccepted(MemoryAddressScanner _, DataPacketToken packet)
    {
        if (packet == null)
            return;

        m_MemoryBank?.PreviewAddress(packet.Value);
        SetFeedback(string.Empty, false);
        RefreshPresentation();
    }

    /// <summary>
    /// Clears stale feedback whenever the store-data pedestal accepts a packet.
    /// </summary>
    public void HandleDataAccepted(MemoryPacketScanner _, DataPacketToken __)
    {
        SetFeedback(string.Empty, false);
        RefreshPresentation();
    }

    /// <summary>
    /// Rebuilds the authored Mem UI from current state.
    /// </summary>
    public void RefreshPresentation()
    {
        m_TransferService.RefreshExpectedTargets(this);
        MemoryPresentation.Refresh(this, m_TransferService);
    }

    /// <summary>
    /// Updates the shared memory feedback field using the authored success and
    /// failure colors.
    /// </summary>
    public void SetFeedback(string message, bool isFailure)
    {
        MemoryPresentation.SetFeedback(m_FeedbackText, message, isFailure, m_SuccessFeedbackColor, m_FailureFeedbackColor);
    }

    /// <summary>Allows helper services to replace the active instruction context.</summary>
    public void SetCurrentInstruction(InstructionDefinition instruction) => m_CurrentInstruction = instruction;

    /// <summary>Lets the transfer service switch the Mem UI from execute mode into continue mode.</summary>
    public void SetAwaitingContinueState(bool isAwaitingContinue, bool hasCompletedMemoryAccess) 
    { 
        m_IsAwaitingContinue = isAwaitingContinue; 
        m_HasCompletedMemoryAccess = hasCompletedMemoryAccess; 
    }

    /// <summary>Stores the resolved address/value pair for recap text after transfer.</summary>
    public void SetLastTransferState(int address, int loadedValue)
    {
        m_LastAddress = address;
        m_LastLoadedValue = loadedValue;
    }

    /// <summary>Allows the service layer to mutate authored MemRead state.</summary>
    public void SetMemReadValue(string value) => m_MemReadValue = value;

    /// <summary>Allows the service layer to mutate authored MemWrite state.</summary>
    public void SetMemWriteValue(string value) => m_MemWriteValue = value;

    /// <summary>Caches the currently running transfer coroutine so it can be cancelled safely.</summary>
    public void SetExecutionRoutine(Coroutine routine) => m_ExecutionRoutine = routine;

    /// <summary>Tracks the spawned Memory Data packet so later phases can preserve or clear it intentionally.</summary>
    public void SetSpawnedMemoryPacket(DataPacketToken packet) => m_SpawnedMemoryPacket = packet;

    void HookBindings(bool subscribe)
    {
        HookPhysicalButtons(subscribe);
        HookActionButton(subscribe);
        HookHintDropdown(subscribe);
        HookScannerEvents(subscribe);
    }

    void HookPhysicalButtons(bool subscribe)
    {
        if (subscribe)
        {
            BinarySignalButtonBinder.Bind(m_MemReadButtonRoot, HandleMemReadPressed);
            BinarySignalButtonBinder.Bind(m_MemWriteButtonRoot, HandleMemWritePressed);
        }
        else
        {
            BinarySignalButtonBinder.Unbind(m_MemReadButtonRoot, HandleMemReadPressed);
            BinarySignalButtonBinder.Unbind(m_MemWriteButtonRoot, HandleMemWritePressed);
        }
    }

    void HookActionButton(bool subscribe)
    {
        if (m_ActionButton == null)
            return;

        m_ActionButton.onClick.RemoveListener(HandleActionPressed);
        if (subscribe)
            m_ActionButton.onClick.AddListener(HandleActionPressed);
    }

    void HookHintDropdown(bool subscribe)
    {
        if (m_HintDropdown == null)
            return;

        m_HintDropdown.onValueChanged.RemoveListener(HandleHintDropdownChanged);
        if (subscribe)
            m_HintDropdown.onValueChanged.AddListener(HandleHintDropdownChanged);
    }

    void HookScannerEvents(bool subscribe)
    {
        if (m_AddressScanner != null)
        {
            m_AddressScanner.PacketAccepted -= HandleAddressAccepted;
            if (subscribe)
                m_AddressScanner.PacketAccepted += HandleAddressAccepted;
        }

        if (m_DataScanner != null)
        {
            m_DataScanner.PacketAccepted -= HandleDataAccepted;
            if (subscribe)
                m_DataScanner.PacketAccepted += HandleDataAccepted;
        }
    }

    void HandleHintDropdownChanged(int _)
    {
        RefreshPresentation();
    }

}

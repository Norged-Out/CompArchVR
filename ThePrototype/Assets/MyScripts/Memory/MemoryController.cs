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

    [Header("Audio")]
    [SerializeField]
    AudioSource m_TransferAudioSource;

    [SerializeField]
    LessonUiAudioCueSet m_LessonAudioCues = new();

    [Header("Physical Buttons")]
    [SerializeField]
    Transform m_MemReadButtonRoot;

    [SerializeField]
    Transform m_MemWriteButtonRoot;

    [Header("Lesson Panel")]
    [SerializeField]
    MemoryLessonPanelRefs m_LessonPanel;

    [Header("Hint Panel")]
    [SerializeField]
    PhaseHintPanelRefs m_HintPanel;

    [SerializeField]
    MemoryHintInfoRefs m_LearningHints;

    [Header("Interaction Panel")]
    [SerializeField]
    GameObject m_MemUiRoot;

    [SerializeField]
    MemoryInteractionPanelRefs m_InteractionPanel;

    [SerializeField]
    PhaseSharedInteractionRefs m_SharedInteraction;

    [Header("Practice")]
    [SerializeField]
    int m_PracticeValidationAttempts = 2;

    [SerializeField]
    int m_PracticeScannerAttempts = 2;

    [SerializeField]
    int m_PracticeHints = 2;

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
    readonly MemoryPracticeFlow m_PracticeFlow = new();
    bool m_IsPhaseActive;
    bool m_IsAwaitingContinue;
    bool m_HasCompletedMemoryAccess;
    int m_LastAddress;
    int m_LastLoadedValue;
    string m_MemReadValue = "0";
    string m_MemWriteValue = "0";
    LessonMode m_CurrentMode = LessonMode.Learning;

    public event Action ContinueRequested;
    public event Action MemoryTransferCompleted;
    public event Action PracticeResetRequested;

    /// <summary>The instruction currently driving the Mem phase.</summary>
    public InstructionDefinition CurrentInstruction => m_CurrentInstruction;

    /// <summary>True while the memory station is the active lesson phase.</summary>
    public bool IsPhaseActive => m_IsPhaseActive;
    public bool IsPracticeMode => m_CurrentMode == LessonMode.Practice;
    public bool IsPracticeAwaitingReset => m_PracticeFlow.IsAwaitingReset;

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
    public TMP_Text LessonRuntimeText => m_LessonPanel.RuntimeText;
    public TMP_Text LoadLessonText => m_LessonPanel.LoadText;
    public TMP_Text StoreLessonText => m_LessonPanel.StoreText;
    public TMP_Text MemReadStatusText => m_InteractionPanel.MemReadStatusText;
    public TMP_Text MemWriteStatusText => m_InteractionPanel.MemWriteStatusText;
    public TMP_Text AddressStatusText => m_InteractionPanel.AddressStatusText;
    public TMP_Text DataStatusText => m_InteractionPanel.DataStatusText;
    public TMP_Text FeedbackText => m_SharedInteraction.FeedbackText;
    public Button ActionButton => m_SharedInteraction.ActionButton;
    public TMP_Text ActionButtonLabel => m_SharedInteraction.ActionLabel;
    public TMP_Dropdown HintDropdown => m_HintPanel.InfoDropdown;
    public TMP_Text HintMemReadText => m_LearningHints.MemReadText;
    public TMP_Text HintMemWriteText => m_LearningHints.MemWriteText;
    public Button PracticeHintButton => m_HintPanel.HintButton;
    public TMP_Text PracticeHintText => m_HintPanel.HintText;
    public PhaseHintPanelRefs HintPanel => m_HintPanel;
    public string ExecuteButtonText => m_ExecuteButtonText;
    public string ContinueButtonText => m_ContinueButtonText;
    public GameObject MemUiRoot => m_MemUiRoot;

    void Awake()
    {
        m_TransferService = new MemoryTransferService();
        ConfigurePracticeFlow();
        HookRuntimeBindings(true);
        MemoryPresentation.PopulateHintDropdown(HintDropdown);
        RefreshPresentation();
        SetFeedback(string.Empty, false);
    }

    void OnEnable()
    {
        m_TransferService ??= new MemoryTransferService();
        ConfigurePracticeFlow();
        HookRuntimeBindings(true);
        MemoryPresentation.PopulateHintDropdown(HintDropdown);
        RefreshPresentation();
    }

    void OnDisable()
    {
        HookRuntimeBindings(false);
    }

    /// <summary>
    /// Activates or deactivates the memory-access phase for the given
    /// instruction.
    /// </summary>
    public void SetPhaseState(bool isActive, LessonMode lessonMode, InstructionDefinition instruction)
    {
        var instructionChanged = instruction != null && instruction != m_CurrentInstruction;
        var modeChanged = lessonMode != m_CurrentMode;
        var isEnteringPhase = isActive && !m_IsPhaseActive;

        m_IsPhaseActive = isActive;
        m_CurrentMode = lessonMode;
        m_CurrentInstruction = instruction != null ? instruction : InstructionDefaults.CreateFallbackAdd();

        if (isEnteringPhase || instructionChanged || modeChanged)
        {
            m_PracticeFlow.Reset();
            m_TransferService.PrepareForMemoryStep(this);
        }

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
        m_CurrentMode = LessonMode.Learning;
        m_PracticeFlow.Reset();

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

        if (m_PracticeFlow.IsAwaitingReset)
        {
            PracticeResetRequested?.Invoke();
            return;
        }

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
            if (IsPracticeMode)
            {
                var didFail = m_PracticeFlow.HandleValidationFailure(validationMessage, out var practiceFeedback);
                if (didFail)
                    EnterPracticeFailureState(practiceFeedback);
                else
                    SetFeedback(practiceFeedback, true);
            }
            else
            {
                SetFeedback(validationMessage, true);
            }

            RefreshPresentation();
            return;
        }

        SetFeedback("Memory access confirmed. Performing the transfer...", false);
        PlayTransferAudio();
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
    /// Reveals the next Practice-mode hint for Memory Access.
    /// </summary>
    public void HandlePracticeHintPressed()
    {
        if (!IsPracticeMode || PracticeHintText == null)
            return;

        PracticeHintText.text = m_PracticeFlow.BuildHint(this, m_TransferService);
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
    public void SetFeedback(string message, bool isFailure, bool playIncorrectCue = true)
    {
        MemoryPresentation.SetFeedback(FeedbackText, message, isFailure, m_SuccessFeedbackColor, m_FailureFeedbackColor);

        if (playIncorrectCue && isFailure && !string.IsNullOrWhiteSpace(message))
            PlayIncorrectCue();
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

    public void ShowPracticeBudgetSummary()
    {
        if (!IsPracticeMode)
            return;

        SetFeedback(m_PracticeFlow.BuildBudgetSummary("Set the memory path and packet inputs, then validate."), false);
    }

    /// <summary>
    /// Exposes a single completion signal once the memory transfer itself has finished.
    /// </summary>
    public void NotifyMemoryTransferCompleted() => MemoryTransferCompleted?.Invoke();

    /// <summary>
    /// Replays the authored memory-transfer cue from the beginning.
    /// </summary>
    public void PlayTransferAudio()
    {
        if (m_TransferAudioSource == null)
            return;

        m_TransferAudioSource.Stop();
        m_TransferAudioSource.Play();
    }

    void HookRuntimeBindings(bool subscribe)
    {
        HookPhysicalButtons(subscribe);
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

    void HookScannerEvents(bool subscribe)
    {
        if (m_AddressScanner != null)
        {
            m_AddressScanner.PacketAccepted -= HandleAddressAccepted;
            m_AddressScanner.PacketRejected -= HandleAddressRejected;
            if (subscribe)
            {
                m_AddressScanner.PacketAccepted += HandleAddressAccepted;
                m_AddressScanner.PacketRejected += HandleAddressRejected;
            }
        }

        if (m_DataScanner != null)
        {
            m_DataScanner.PacketAccepted -= HandleDataAccepted;
            m_DataScanner.PacketRejected -= HandleDataRejected;
            if (subscribe)
            {
                m_DataScanner.PacketAccepted += HandleDataAccepted;
                m_DataScanner.PacketRejected += HandleDataRejected;
            }
        }
    }

    public void HandleHintDropdownChanged(int _)
    {
        RefreshPresentation();
    }

    void HandleAddressRejected(MemoryAddressScanner _, DataPacketToken packetToken)
    {
        if (!IsPracticeMode || !m_IsPhaseActive || m_HasCompletedMemoryAccess || IsPracticeAwaitingReset)
            return;

        var didFail = m_PracticeFlow.HandleScannerFailure("Address", packetToken, out var feedbackText);
        if (didFail)
            EnterPracticeFailureState(feedbackText);
        else
            SetFeedback(feedbackText, true);
        RefreshPresentation();
    }

    void HandleDataRejected(MemoryPacketScanner _, DataPacketToken packetToken)
    {
        if (!IsPracticeMode || !m_IsPhaseActive || m_HasCompletedMemoryAccess || IsPracticeAwaitingReset)
            return;

        var didFail = m_PracticeFlow.HandleScannerFailure("Data", packetToken, out var feedbackText);
        if (didFail)
            EnterPracticeFailureState(feedbackText);
        else
            SetFeedback(feedbackText, true);
        RefreshPresentation();
    }

    public void PlayPhaseActivatedCue()
    {
        m_LessonAudioCues.PlayPhaseActivatedCue();
    }

    public void PlayPhaseCompletedCue()
    {
        m_LessonAudioCues.PlayPhaseCompletedCue();
    }

    public void PlayIncorrectCue()
    {
        m_LessonAudioCues.PlayIncorrectCue();
    }

    public void PlayLessonCompletedCue()
    {
        m_LessonAudioCues.PlayLessonCompletedCue();
    }

    /// <summary>
    /// Dev-mode helper that finishes a load without requiring scanner setup.
    /// </summary>
    public void DevForceCompleteLoad(int addressValue, int loadedValue)
    {
        if (!m_IsPhaseActive || m_CurrentInstruction == null)
            return;

        m_TransferService.SpawnMemoryDataPacket(this, addressValue, loadedValue);
        SetLastTransferState(addressValue, loadedValue);
        NotifyMemoryTransferCompleted();
        ContinueRequested?.Invoke();
        RefreshPresentation();
    }

    /// <summary>
    /// Dev-mode helper that finishes a store without requiring scanner setup.
    /// </summary>
    public void DevForceCompleteStore(int addressValue, int storedValue)
    {
        if (!m_IsPhaseActive || m_CurrentInstruction == null)
            return;

        m_MemoryBank?.TryWriteWord(addressValue, storedValue, out _);
        SetLastTransferState(addressValue, storedValue);
        NotifyMemoryTransferCompleted();
        ContinueRequested?.Invoke();
        RefreshPresentation();
    }

    void ConfigurePracticeFlow()
    {
        m_PracticeFlow.Configure(m_PracticeValidationAttempts, m_PracticeScannerAttempts, m_PracticeHints);
    }

    /// <summary>
    /// Leaves the Memory station in a failure end-state so the dedicated cue
    /// can play before the restart is confirmed.
    /// </summary>
    void EnterPracticeFailureState(string feedbackText)
    {
        m_IsAwaitingContinue = false;
        m_AddressScanner?.SetActive(false);
        m_DataScanner?.SetActive(false);
        SetFeedback(m_PracticeFlow.BuildFailureResetText(feedbackText), true, false);
        m_LessonAudioCues.PlayFailureCue();
    }

}

using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Owns the authored write-back station and write-back UI.
/// This controller only coordinates scene objects and lesson progression.
/// Validation and transfer behavior live in focused helper classes.
/// </summary>
[DisallowMultipleComponent]
public class WriteBackController : MonoBehaviour
{
    [Header("Write-Back Station")]
    [SerializeField]
    WriteBackRegisterScanner m_RegisterScanner;

    [SerializeField]
    WriteBackPacketScanner m_PacketScanner;

    [SerializeField]
    PipeSequencePlayer m_PipeSequencePlayer;

    [SerializeField]
    ParticleSystem m_TransferParticles;

    [SerializeField]
    Transform m_RegDstButtonRoot;

    [SerializeField]
    Transform m_RegWriteButtonRoot;

    [SerializeField]
    Transform m_MemToRegButtonRoot;

    [Header("Write-Back UI")]
    [SerializeField]
    GameObject m_WbUiRoot;

    [SerializeField]
    TMP_Text m_LessonRuntimeText;

    [SerializeField]
    TMP_Text m_RegDstStatusText;

    [SerializeField]
    TMP_Text m_RegWriteStatusText;

    [SerializeField]
    TMP_Text m_MemToRegStatusText;

    [SerializeField]
    TMP_Text m_RegisterStatusText;

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
    TMP_Text m_HintRegDstText;

    [SerializeField]
    TMP_Text m_HintRegWriteText;

    [SerializeField]
    TMP_Text m_HintMemToRegText;

    [Header("Timing")]
    [SerializeField]
    float m_ParticleLeadTimeSeconds = 0.75f;

    [Header("Labels")]
    [SerializeField]
    string m_ExecuteButtonText = "Execute Write Back";

    [SerializeField]
    string m_ContinueButtonText = "Continue";

    [Header("Feedback Colors")]
    [SerializeField]
    Color m_SuccessFeedbackColor = new(0.78f, 0.96f, 0.82f, 1f);

    [SerializeField]
    Color m_FailureFeedbackColor = new(1f, 0.55f, 0.55f, 1f);

    InstructionDefinition m_CurrentInstruction;
    RegisterBank m_RegisterBank;
    Coroutine m_TransferRoutine;
    WriteBackTransferService m_TransferService;
    bool m_IsPhaseActive;
    bool m_IsAwaitingContinue;
    bool m_HasAppliedWriteBack;
    int m_LastTransferredValue;
    string m_LastTargetRegister = string.Empty;
    DataPacketRole m_LastTransferredPacketRole = DataPacketRole.None;
    string m_RegDstValue = "0";
    string m_RegWriteValue = "0";
    string m_MemToRegValue = "0";

    public event Action<string, int> WriteBackApplied;
    public event Action ContinueRequested;

    /// <summary>
    /// The currently selected instruction for the WB phase.
    /// </summary>
    public InstructionDefinition CurrentInstruction => m_CurrentInstruction;

    /// <summary>
    /// The register bank whose values are updated during successful WB.
    /// </summary>
    public RegisterBank RegisterBank => m_RegisterBank;

    /// <summary>
    /// Exposes the current RegDst signal state for presentation.
    /// </summary>
    public string RegDstValue => m_RegDstValue;

    /// <summary>
    /// Exposes the current RegWrite signal state for presentation.
    /// </summary>
    public string RegWriteValue => m_RegWriteValue;

    /// <summary>
    /// Exposes the current MemToReg signal state for presentation.
    /// </summary>
    public string MemToRegValue => m_MemToRegValue;

    /// <summary>
    /// True once the actual register write has already happened.
    /// </summary>
    public bool HasAppliedWriteBack => m_HasAppliedWriteBack;

    /// <summary>
    /// True after a successful write-back, when the button becomes Continue.
    /// </summary>
    public bool IsAwaitingContinue => m_IsAwaitingContinue;

    /// <summary>
    /// Most recent destination register from a finished WB transfer.
    /// </summary>
    public string LastTargetRegister => m_LastTargetRegister;

    /// <summary>
    /// Most recent value written during a finished WB transfer.
    /// </summary>
    public int LastTransferredValue => m_LastTransferredValue;

    /// <summary>
    /// Most recent packet role consumed during a finished WB transfer.
    /// </summary>
    public DataPacketRole LastTransferredPacketRole => m_LastTransferredPacketRole;

    /// <summary>
    /// The currently accepted destination register, if one is latched.
    /// </summary>
    public RegisterToken AcceptedRegister => m_RegisterScanner != null ? m_RegisterScanner.AcceptedRegister : null;

    /// <summary>
    /// The currently accepted write-back packet, if one is latched.
    /// </summary>
    public DataPacketToken AcceptedPacket => m_PacketScanner != null ? m_PacketScanner.AcceptedPacket : null;

    void Awake()
    {
        // Keep the behavioral service stateless so the controller remains the
        // single runtime owner of authored scene state.
        m_TransferService = new WriteBackTransferService();
        HookButtons();
        HookHintDropdown(true);
        HookScannerEvents(true);
        WriteBackPresentation.PopulateHintDropdown(m_HintDropdown);
        ResetPipeVisuals();
        SetFeedback(string.Empty, false);
        RefreshExpectedTargets();
        RefreshPresentation();
    }

    void OnEnable()
    {
        // Rebind authored events whenever Unity re-enables this station so play
        // mode toggles do not accumulate duplicate listeners.
        HookButtons();
        HookHintDropdown(true);
        HookScannerEvents(true);
        WriteBackPresentation.PopulateHintDropdown(m_HintDropdown);
        RefreshExpectedTargets();
        RefreshPresentation();
    }

    void OnDisable()
    {
        HookScannerEvents(false);
        HookHintDropdown(false);
        UnhookButtons();
    }

    /// <summary>
    /// Shows or hides the WB phase and binds the active instruction context.
    /// </summary>
    public void SetPhaseState(bool isActive, InstructionDefinition instruction, RegisterBank registerBank)
    {
        var instructionChanged = instruction != null && instruction != m_CurrentInstruction;
        var enteringPhase = isActive && !m_IsPhaseActive;

        m_IsPhaseActive = isActive;
        m_CurrentInstruction = instruction != null ? instruction : InstructionDefaults.CreateFallbackAdd();
        m_RegisterBank = registerBank;

        if (enteringPhase || instructionChanged)
            PrepareForWriteBackStep();

        if (m_WbUiRoot != null)
            m_WbUiRoot.SetActive(isActive);

        m_RegisterScanner?.SetActive(isActive);
        m_PacketScanner?.SetActive(isActive);
        RefreshExpectedTargets();
        RefreshPresentation();
    }

    /// <summary>
    /// Fully clears temporary WB state and hides the authored UI.
    /// </summary>
    public void ResetWriteBackState()
    {
        if (m_TransferRoutine != null)
        {
            StopCoroutine(m_TransferRoutine);
            m_TransferRoutine = null;
        }

        if (m_TransferParticles != null)
            m_TransferParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        m_CurrentInstruction = null;
        m_IsPhaseActive = false;
        m_IsAwaitingContinue = false;
        m_HasAppliedWriteBack = false;
        m_LastTransferredValue = 0;
        m_LastTargetRegister = string.Empty;
        m_LastTransferredPacketRole = DataPacketRole.None;
        m_RegDstValue = "0";
        m_RegWriteValue = "0";
        m_MemToRegValue = "0";

        m_RegisterScanner?.ResetScanner();
        m_PacketScanner?.ResetScanner();
        ResetPipeVisuals();
        SetFeedback(string.Empty, false);
        RefreshPresentation();

        if (m_WbUiRoot != null)
            m_WbUiRoot.SetActive(false);
    }

    /// <summary>
    /// Handles the authored WB UI action button.
    /// Before success it validates and executes transfer.
    /// After success it advances the lesson flow.
    /// </summary>
    public void HandleActionPressed()
    {
        if (!m_IsPhaseActive || m_TransferRoutine != null)
            return;

        if (m_IsAwaitingContinue)
        {
            m_IsAwaitingContinue = false;
            ContinueRequested?.Invoke();
            return;
        }

        if (!m_TransferService.TryValidate(
                m_CurrentInstruction,
                m_RegDstValue,
                m_RegWriteValue,
                m_MemToRegValue,
                m_RegisterScanner,
                m_PacketScanner,
                out var validationMessage))
        {
            SetFeedback(validationMessage, true);
            RefreshPresentation();
            return;
        }

        SetFeedback("Write-back confirmed. Transferring value into the destination register...", false);
        RefreshPresentation();
        m_TransferRoutine = StartCoroutine(ApplyWriteBackRoutine());
    }

    /// <summary>
    /// Resolves the register that should be accepted under the current RegDst state.
    /// </summary>
    public string GetExpectedRegisterIdFromControlState()
    {
        return m_TransferService.GetExpectedRegisterId(m_CurrentInstruction, m_RegDstValue);
    }

    /// <summary>
    /// Resolves the packet role that should be accepted under the current MemToReg state.
    /// </summary>
    public DataPacketRole GetExpectedPacketRoleFromControlState()
    {
        return m_TransferService.GetExpectedPacketRole(m_CurrentInstruction, m_MemToRegValue);
    }

    /// <summary>
    /// Updates all authored WB text and hint blocks.
    /// </summary>
    public void RefreshPresentation()
    {
        WriteBackPresentation.Refresh(this);
    }

    /// <summary>
    /// Updates the authored WB feedback field.
    /// </summary>
    public void SetFeedback(string message, bool isFailure)
    {
        WriteBackPresentation.SetFeedback(m_FeedbackText, message, isFailure, m_SuccessFeedbackColor, m_FailureFeedbackColor);
    }

    void PrepareForWriteBackStep()
    {
        // Entering WB should always feel like a fresh authored station state:
        // default signals, empty scanners, idle pipes, and no pending continue.
        if (m_TransferRoutine != null)
        {
            StopCoroutine(m_TransferRoutine);
            m_TransferRoutine = null;
        }

        if (m_TransferParticles != null)
            m_TransferParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        m_RegDstValue = "0";
        m_RegWriteValue = "0";
        m_MemToRegValue = "0";
        m_IsAwaitingContinue = false;
        m_HasAppliedWriteBack = false;
        m_LastTransferredValue = 0;
        m_LastTargetRegister = string.Empty;
        m_LastTransferredPacketRole = DataPacketRole.None;

        m_RegisterScanner?.ResetScanner();
        m_PacketScanner?.ResetScanner();
        ResetPipeVisuals();
        SetFeedback(string.Empty, false);
        RefreshExpectedTargets();
        RefreshPresentation();
    }

    IEnumerator ApplyWriteBackRoutine()
    {
        var packet = AcceptedPacket;
        var targetRegister = AcceptedRegister != null ? AcceptedRegister.RegisterId : string.Empty;

        // The transfer service owns the actual sequencing so the controller can
        // stay focused on lesson state and UI transitions.
        yield return m_TransferService.RunTransferRoutine(
            m_RegisterBank,
            m_PipeSequencePlayer,
            m_TransferParticles,
            m_ParticleLeadTimeSeconds,
            targetRegister,
            packet,
            OnWriteBackTransferApplied);

        m_TransferRoutine = null;
        RefreshPresentation();
    }

    void OnWriteBackTransferApplied(string destinationRegister, int transferredValue, DataPacketRole packetRole)
    {
        // Cache the applied result so the UI can swap from "waiting" language
        // to recap language before the learner presses Continue.
        m_LastTargetRegister = destinationRegister;
        m_LastTransferredValue = transferredValue;
        m_LastTransferredPacketRole = packetRole;
        m_HasAppliedWriteBack = true;
        m_IsAwaitingContinue = true;

        m_PacketScanner?.ConsumeAcceptedPacket();
        SetFeedback(
            $"Write-back complete. {destinationRegister} now stores {transferredValue}. Click Continue to proceed to Program Counter Update.",
            false);
        RefreshPresentation();
        WriteBackApplied?.Invoke(destinationRegister, transferredValue);
    }

    void HandleRegDstPressed(SelectEnterEventArgs _)
    {
        if (!m_IsPhaseActive || m_HasAppliedWriteBack)
            return;

        // WB uses simple binary authored buttons, so the interaction is just a
        // flip between the two legal control values.
        m_RegDstValue = m_RegDstValue == "1" ? "0" : "1";
        RefreshExpectedTargets();
        ResetScannersIfSignalStateIsInvalid();
        SetFeedback(string.Empty, false);
        RefreshPresentation();
    }

    void HandleRegWritePressed(SelectEnterEventArgs _)
    {
        if (!m_IsPhaseActive || m_HasAppliedWriteBack)
            return;

        // If the learner changes the control path mid-step, the authored
        // scanner latches must be revalidated against the new expectation.
        m_RegWriteValue = m_RegWriteValue == "1" ? "0" : "1";
        ResetScannersIfSignalStateIsInvalid();
        SetFeedback(string.Empty, false);
        RefreshPresentation();
    }

    void HandleMemToRegPressed(SelectEnterEventArgs _)
    {
        if (!m_IsPhaseActive || m_HasAppliedWriteBack)
            return;

        // MemToReg changes which packet type is legal on the data pedestal, so
        // expected packet role and any existing latches must be refreshed.
        m_MemToRegValue = m_MemToRegValue == "1" ? "0" : "1";
        RefreshExpectedTargets();
        ResetScannersIfSignalStateIsInvalid();
        SetFeedback(string.Empty, false);
        RefreshPresentation();
    }

    void HandleRegisterAccepted(WriteBackRegisterScanner _, RegisterToken __)
    {
        if (m_HasAppliedWriteBack)
            return;

        SetFeedback(string.Empty, false);
        RefreshPresentation();
    }

    void HandlePacketAccepted(WriteBackPacketScanner _, DataPacketToken __)
    {
        if (m_HasAppliedWriteBack)
            return;

        SetFeedback(string.Empty, false);
        RefreshPresentation();
    }

    void HookButtons()
    {
        BinarySignalButtonBinder.Bind(m_RegDstButtonRoot, HandleRegDstPressed);
        BinarySignalButtonBinder.Bind(m_RegWriteButtonRoot, HandleRegWritePressed);
        BinarySignalButtonBinder.Bind(m_MemToRegButtonRoot, HandleMemToRegPressed);

        if (m_ActionButton != null)
        {
            m_ActionButton.onClick.RemoveListener(HandleActionPressed);
            m_ActionButton.onClick.AddListener(HandleActionPressed);
        }
    }

    void UnhookButtons()
    {
        BinarySignalButtonBinder.Unbind(m_RegDstButtonRoot, HandleRegDstPressed);
        BinarySignalButtonBinder.Unbind(m_RegWriteButtonRoot, HandleRegWritePressed);
        BinarySignalButtonBinder.Unbind(m_MemToRegButtonRoot, HandleMemToRegPressed);

        if (m_ActionButton != null)
            m_ActionButton.onClick.RemoveListener(HandleActionPressed);
    }

    void HookHintDropdown(bool subscribe)
    {
        if (m_HintDropdown == null)
            return;

        if (subscribe)
        {
            m_HintDropdown.onValueChanged.RemoveListener(HandleHintDropdownChanged);
            m_HintDropdown.onValueChanged.AddListener(HandleHintDropdownChanged);
        }
        else
        {
            m_HintDropdown.onValueChanged.RemoveListener(HandleHintDropdownChanged);
        }
    }

    void HookScannerEvents(bool subscribe)
    {
        // Scanners raise semantic events after their own validation finishes;
        // the controller only reacts by refreshing authored UI state.
        if (m_RegisterScanner != null)
        {
            m_RegisterScanner.RegisterAccepted -= HandleRegisterAccepted;
            if (subscribe)
                m_RegisterScanner.RegisterAccepted += HandleRegisterAccepted;
        }

        if (m_PacketScanner != null)
        {
            m_PacketScanner.PacketAccepted -= HandlePacketAccepted;
            if (subscribe)
                m_PacketScanner.PacketAccepted += HandlePacketAccepted;
        }
    }

    void HandleHintDropdownChanged(int _)
    {
        RefreshPresentation();
    }

    void RefreshExpectedTargets()
    {
        // The pedestals themselves should always reflect the current WB control
        // path so learners can see the consequences of signal changes.
        m_RegisterScanner?.SetExpectedRegisterId(GetExpectedRegisterIdFromControlState());
        m_PacketScanner?.SetExpectedPacketRole(GetExpectedPacketRoleFromControlState());
    }

    void ResetScannersIfSignalStateIsInvalid()
    {
        if (m_CurrentInstruction == null)
            return;

        // Once signals deviate from the authored expectation, both latches are
        // cleared so the learner must rebuild the correct write-back setup.
        var expectedRegDst = m_CurrentInstruction.GetExpectedRegDstControlValue();
        var expectedRegWrite = m_CurrentInstruction.GetExpectedRegWriteControlValue();
        var expectedMemToReg = m_CurrentInstruction.GetExpectedMemToRegControlValue();

        if (m_RegDstValue == expectedRegDst &&
            m_RegWriteValue == expectedRegWrite &&
            m_MemToRegValue == expectedMemToReg)
        {
            return;
        }

        m_RegisterScanner?.ResetScanner();
        m_PacketScanner?.ResetScanner();
    }

    void ResetPipeVisuals()
    {
        m_PipeSequencePlayer?.ResetToIdle();
    }

    public TMP_Text LessonRuntimeText => m_LessonRuntimeText;
    public TMP_Text RegDstStatusText => m_RegDstStatusText;
    public TMP_Text RegWriteStatusText => m_RegWriteStatusText;
    public TMP_Text MemToRegStatusText => m_MemToRegStatusText;
    public TMP_Text RegisterStatusText => m_RegisterStatusText;
    public TMP_Text DataStatusText => m_DataStatusText;
    public TMP_Text ActionButtonLabel => m_ActionButtonLabel;
    public Button ActionButton => m_ActionButton;
    public TMP_Dropdown HintDropdown => m_HintDropdown;
    public TMP_Text HintRegDstText => m_HintRegDstText;
    public TMP_Text HintRegWriteText => m_HintRegWriteText;
    public TMP_Text HintMemToRegText => m_HintMemToRegText;
    public bool IsPhaseActive => m_IsPhaseActive;
    public bool IsTransferRunning => m_TransferRoutine != null;
    public string ExecuteButtonText => m_ExecuteButtonText;
    public string ContinueButtonText => m_ContinueButtonText;
}

using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Owns the authored Program Counter update station and its three-panel UI.
/// The controller coordinates physical buttons, packet scanners, and lesson
/// progression, while focused helper classes handle validation and UI building.
/// </summary>
[DisallowMultipleComponent]
public class PcUpdateController : MonoBehaviour
{
    [Header("Station")]
    [SerializeField]
    PcUpdatePacketScanner m_ImmediateScanner;

    [SerializeField]
    PcUpdatePacketScanner m_ZeroScanner;

    [SerializeField]
    Transform m_BranchButtonRoot;

    [SerializeField]
    Transform m_JumpButtonRoot;

    [Header("Audio")]
    [SerializeField]
    LessonUiAudioCueSet m_LessonAudioCues = new();

    [Header("PC Update UI")]
    [SerializeField]
    GameObject m_PcUpdateUiRoot;

    [SerializeField]
    TMP_Text m_LessonRuntimeText;

    [SerializeField]
    TMP_Text m_LessonBranchText;

    [SerializeField]
    TMP_Text m_LessonShiftText;

    [SerializeField]
    TMP_Text m_LessonResultText;

    [SerializeField]
    TMP_Text m_LessonEndText;

    [SerializeField]
    GameObject m_PcUpdateGroupRoot;

    [SerializeField]
    GameObject m_SignalsGroupRoot;

    [SerializeField]
    Slider m_PcIncrementSlider;

    [SerializeField]
    TMP_Text m_BranchStatusText;

    [SerializeField]
    TMP_Text m_JumpStatusText;

    [SerializeField]
    GameObject m_ImmediateGroupRoot;

    [SerializeField]
    TMP_Text m_ImmediateStatusText;

    [SerializeField]
    Button m_ShiftButton;

    [SerializeField]
    GameObject m_BranchConditionGroupRoot;

    [SerializeField]
    TMP_Text m_ZeroStatusText;

    [SerializeField]
    TMP_Dropdown m_BranchConditionDropdown;

    [SerializeField]
    TMP_Text m_PCSrcStatusText;

    [SerializeField]
    TMP_Text m_FeedbackText;

    [SerializeField]
    Button m_ActionButton;

    [SerializeField]
    TMP_Text m_ActionButtonLabel;

    [Header("Hint UI")]
    [SerializeField]
    TMP_Dropdown m_HintDropdown;

    [SerializeField]
    TMP_Text m_HintPcText;

    [SerializeField]
    TMP_Text m_HintPcSrcText;

    [SerializeField]
    TMP_Text m_HintBranchText;

    [SerializeField]
    TMP_Text m_HintJumpText;

    [SerializeField]
    TMP_Text m_HintShiftLeftTwoText;

    [SerializeField]
    TMP_Text m_HintZeroText;

    [Header("Labels")]
    [SerializeField]
    string m_ConfirmButtonText = "Confirm PC Update";

    [SerializeField]
    string m_ContinueButtonText = "Continue";

    [SerializeField]
    Color m_SuccessFeedbackColor = new(0.78f, 0.96f, 0.82f, 1f);

    [SerializeField]
    Color m_FailureFeedbackColor = new(1f, 0.55f, 0.55f, 1f);

    InstructionDefinition m_CurrentInstruction;
    bool m_IsPhaseActive;
    bool m_IsAwaitingContinue;
    string m_BranchValue = "0";
    string m_JumpValue = "0";
    DataPacketToken m_ShiftPreparedImmediatePacket;
    PcBranchService m_BranchService;

    public event System.Action ContinueRequested;
    public event System.Action PcUpdateConfirmed;

    /// <summary>
    /// Builds the authored services and wires every authored UI / scanner event
    /// when the station first comes alive in the scene.
    /// </summary>
    void Awake()
    {
        m_BranchService = new PcBranchService();
        PcUpdatePresentation.PopulateBranchConditionDropdown(m_BranchConditionDropdown);
        PcUpdatePresentation.PopulateHintDropdown(m_HintDropdown);
        HookButtons(true);
        HookDropdown(true);
        HookSlider(true);
        HookScannerEvents(true);
        RefreshPresentation();
    }

    /// <summary>
    /// Rebinds listeners after Unity re-enables the station object so the
    /// controller continues working after scene reloads and resets.
    /// </summary>
    void OnEnable()
    {
        m_BranchService ??= new PcBranchService();
        PcUpdatePresentation.PopulateBranchConditionDropdown(m_BranchConditionDropdown);
        PcUpdatePresentation.PopulateHintDropdown(m_HintDropdown);
        HookButtons(true);
        HookDropdown(true);
        HookSlider(true);
        HookScannerEvents(true);
        RefreshPresentation();
    }

    /// <summary>
    /// Removes listener hookups while the object is disabled to avoid duplicate
    /// callbacks when Unity toggles the station on and off.
    /// </summary>
    void OnDisable()
    {
        HookButtons(false);
        HookDropdown(false);
        HookSlider(false);
        HookScannerEvents(false);
    }

    /// <summary>
    /// Called by the lesson flow whenever the PC update phase becomes active or
    /// inactive for the current instruction.
    /// </summary>
    public void SetPhaseState(bool isActive, InstructionDefinition instruction)
    {
        var instructionChanged = instruction != null && instruction != m_CurrentInstruction;
        var isEnteringPhase = isActive && !m_IsPhaseActive;

        m_IsPhaseActive = isActive;
        m_CurrentInstruction = instruction != null ? instruction : InstructionDefaults.CreateFallbackAdd();

        if (isEnteringPhase || instructionChanged)
            PrepareForPcUpdate();

        if (m_PcUpdateUiRoot != null)
            m_PcUpdateUiRoot.SetActive(isActive);

        var showBranchSpecificGroups = isActive && m_BranchValue == "1" && !m_IsAwaitingContinue;
        m_ImmediateScanner?.SetActive(showBranchSpecificGroups);
        m_ZeroScanner?.SetActive(showBranchSpecificGroups);
        RefreshPresentation();
    }

    /// <summary>
    /// Restores the station to its authored baseline so a new lesson run always
    /// starts from a clean Program Counter update step.
    /// </summary>
    public void ResetPcUpdateState()
    {
        m_CurrentInstruction = null;
        m_IsPhaseActive = false;
        m_IsAwaitingContinue = false;
        m_BranchValue = "0";
        m_JumpValue = "0";
        m_ShiftPreparedImmediatePacket = null;

        if (m_PcIncrementSlider != null)
            m_PcIncrementSlider.SetValueWithoutNotify(0f);

        if (m_BranchConditionDropdown != null)
            m_BranchConditionDropdown.SetValueWithoutNotify(0);

        if (m_HintDropdown != null)
            m_HintDropdown.SetValueWithoutNotify(0);

        m_ImmediateScanner?.ResetScanner();
        m_ZeroScanner?.ResetScanner();
        SetFeedback(string.Empty, false);
        RefreshPresentation();

        if (m_PcUpdateUiRoot != null)
            m_PcUpdateUiRoot.SetActive(false);
    }

    /// <summary>
    /// Handles the main interaction button. During solve-state it validates the
    /// final PC decision; during end-state it requests lesson continuation.
    /// </summary>
    public void HandleActionPressed()
    {
        if (!m_IsPhaseActive)
            return;

        if (m_IsAwaitingContinue)
        {
            m_IsAwaitingContinue = false;
            ContinueRequested?.Invoke();
            return;
        }

        if (!m_BranchService.TryValidate(
                m_CurrentInstruction,
                m_BranchValue,
                m_JumpValue,
                GetPcIncrementValue(),
                m_ImmediateScanner,
                m_ShiftPreparedImmediatePacket,
                m_ZeroScanner,
                GetSelectedBranchCondition(),
                out var validationMessage))
        {
            SetFeedback(validationMessage, true);
            RefreshPresentation();
            return;
        }

        m_IsAwaitingContinue = true;
        PcUpdateConfirmed?.Invoke();
        SetFeedback("Program Counter update confirmed. Press Continue to reset the lesson.", false);
        RefreshPresentation();
    }

    /// <summary>
    /// Applies the default phase-state expected before the learner starts
    /// solving the Program Counter update logic.
    /// </summary>
    void PrepareForPcUpdate()
    {
        // Every PC update phase begins from the same authored baseline so the
        // learner always rebuilds the final control decision from scratch.
        m_IsAwaitingContinue = false;
        m_BranchValue = "0";
        m_JumpValue = "0";
        m_ShiftPreparedImmediatePacket = null;

        if (m_PcIncrementSlider != null)
        {
            m_PcIncrementSlider.minValue = 0f;
            m_PcIncrementSlider.maxValue = 4f;
            m_PcIncrementSlider.wholeNumbers = true;
            m_PcIncrementSlider.SetValueWithoutNotify(0f);
        }

        if (m_BranchConditionDropdown != null)
            m_BranchConditionDropdown.SetValueWithoutNotify(0);

        m_ImmediateScanner?.ResetScanner();
        m_ZeroScanner?.ResetScanner();
        SetFeedback("Move the PC update control from 0 to 4, then confirm the next PC path.", false);
        RefreshPresentation();
    }

    /// <summary>
    /// Marks the currently accepted branch immediate as shift-left-by-2 ready.
    /// The packet itself is not mutated; the controller simply records that the
    /// learner performed the required transformation step.
    /// </summary>
    void HandleShiftPressed()
    {
        if (!m_IsPhaseActive || m_BranchValue != "1" || m_ImmediateScanner == null)
            return;

        var immediatePacket = m_ImmediateScanner.AcceptedPacket;
        if (immediatePacket == null)
        {
            SetFeedback("Place the sign-extended immediate first.", true);
            RefreshPresentation();
            return;
        }

        if (!immediatePacket.IsSignExtended)
        {
            SetFeedback("The immediate must be sign-extended before shifting.", true);
            RefreshPresentation();
            return;
        }

        m_ShiftPreparedImmediatePacket = immediatePacket;
        SetFeedback("Branch offset shifted left by 2.", false);
        RefreshPresentation();
    }

    /// <summary>
    /// Toggles the authored Branch control button and clears branch-only state
    /// whenever the learner turns branching back off.
    /// </summary>
    void HandleBranchPressed(SelectEnterEventArgs _)
    {
        if (!m_IsPhaseActive || m_IsAwaitingContinue)
            return;

        m_BranchValue = m_BranchValue == "1" ? "0" : "1";
        if (m_BranchValue != "1")
        {
            m_ShiftPreparedImmediatePacket = null;
            m_ImmediateScanner?.ResetScanner();
            m_ZeroScanner?.ResetScanner();
            if (m_BranchConditionDropdown != null)
                m_BranchConditionDropdown.SetValueWithoutNotify(0);
        }

        SetFeedback(string.Empty, false);
        RefreshPresentation();
    }

    /// <summary>
    /// Toggles the authored Jump control button.
    /// </summary>
    void HandleJumpPressed(SelectEnterEventArgs _)
    {
        if (!m_IsPhaseActive || m_IsAwaitingContinue)
            return;

        m_JumpValue = m_JumpValue == "1" ? "0" : "1";
        SetFeedback(string.Empty, false);
        RefreshPresentation();
    }

    /// <summary>
    /// Any dropdown change invalidates stale feedback and rebuilds the panel so
    /// the learner sees the latest authored hint / condition state.
    /// </summary>
    void HandleDropdownChanged(int _)
    {
        SetFeedback(string.Empty, false);
        RefreshPresentation();
    }

    /// <summary>
    /// Keeps the PC status text in sync while the learner drags the PC+4
    /// slider, rather than waiting for a later confirm press.
    /// </summary>
    void HandleSliderChanged(float _)
    {
        if (!m_IsPhaseActive)
            return;

        RefreshPresentation();
    }

    /// <summary>
    /// Re-evaluates immediate-specific status any time the branch offset
    /// pedestal accepts a new packet.
    /// </summary>
    void HandleImmediateAccepted(PcUpdatePacketScanner _, DataPacketToken __)
    {
        if (m_ImmediateScanner == null || m_ImmediateScanner.AcceptedPacket != m_ShiftPreparedImmediatePacket)
            m_ShiftPreparedImmediatePacket = null;

        SetFeedback(string.Empty, false);
        RefreshPresentation();
    }

    /// <summary>
    /// Re-evaluates branch-condition status any time the zero-result pedestal
    /// accepts a new packet.
    /// </summary>
    void HandleZeroAccepted(PcUpdatePacketScanner _, DataPacketToken __)
    {
        SetFeedback(string.Empty, false);
        RefreshPresentation();
    }

    /// <summary>
    /// Centralizes binding for the authored physical controls and UI buttons so
    /// every enable / disable path uses the same hookup rules.
    /// </summary>
    void HookButtons(bool subscribe)
    {
        if (subscribe)
        {
            BinarySignalButtonBinder.Bind(m_BranchButtonRoot, HandleBranchPressed);
            BinarySignalButtonBinder.Bind(m_JumpButtonRoot, HandleJumpPressed);
        }
        else
        {
            BinarySignalButtonBinder.Unbind(m_BranchButtonRoot, HandleBranchPressed);
            BinarySignalButtonBinder.Unbind(m_JumpButtonRoot, HandleJumpPressed);
        }

        if (m_ShiftButton != null)
        {
            m_ShiftButton.onClick.RemoveListener(HandleShiftPressed);
            if (subscribe)
                m_ShiftButton.onClick.AddListener(HandleShiftPressed);
        }

        if (m_ActionButton != null)
        {
            m_ActionButton.onClick.RemoveListener(HandleActionPressed);
            if (subscribe)
                m_ActionButton.onClick.AddListener(HandleActionPressed);
        }
    }

    /// <summary>
    /// Wires both authored dropdowns that affect live presentation state.
    /// </summary>
    void HookDropdown(bool subscribe)
    {
        if (m_BranchConditionDropdown != null)
        {
            m_BranchConditionDropdown.onValueChanged.RemoveListener(HandleDropdownChanged);
            if (subscribe)
                m_BranchConditionDropdown.onValueChanged.AddListener(HandleDropdownChanged);
        }

        if (m_HintDropdown != null)
        {
            m_HintDropdown.onValueChanged.RemoveListener(HandleDropdownChanged);
            if (subscribe)
                m_HintDropdown.onValueChanged.AddListener(HandleDropdownChanged);
        }
    }

    /// <summary>
    /// Wires the authored PC slider used for the PC + 4 update.
    /// </summary>
    void HookSlider(bool subscribe)
    {
        if (m_PcIncrementSlider == null)
            return;

        m_PcIncrementSlider.onValueChanged.RemoveListener(HandleSliderChanged);
        if (subscribe)
            m_PcIncrementSlider.onValueChanged.AddListener(HandleSliderChanged);
    }

    /// <summary>
    /// Wires the two packet pedestals so packet acceptance can immediately
    /// refresh the lesson UI.
    /// </summary>
    void HookScannerEvents(bool subscribe)
    {
        HookScannerEvent(m_ImmediateScanner, HandleImmediateAccepted, subscribe);
        HookScannerEvent(m_ZeroScanner, HandleZeroAccepted, subscribe);
    }

    /// <summary>
    /// Shared helper for attaching or detaching a packet-scanner callback.
    /// </summary>
    void HookScannerEvent(
        PcUpdatePacketScanner scanner,
        System.Action<PcUpdatePacketScanner, DataPacketToken> handler,
        bool subscribe)
    {
        if (scanner == null)
            return;

        scanner.PacketAccepted -= handler;
        if (subscribe)
            scanner.PacketAccepted += handler;
    }

    /// <summary>
    /// Rebuilds the full Program Counter update presentation from current
    /// runtime state.
    /// </summary>
    void RefreshPresentation()
    {
        PcUpdatePresentation.Refresh(this, m_BranchService);
    }

    /// <summary>
    /// Updates the authored feedback text using shared success / failure
    /// colors.
    /// </summary>
    void SetFeedback(string message, bool isFailure)
    {
        PcUpdatePresentation.SetFeedback(m_FeedbackText, message, isFailure, m_SuccessFeedbackColor, m_FailureFeedbackColor);

        if (isFailure && !string.IsNullOrWhiteSpace(message))
            PlayIncorrectCue();
    }

    /// <summary>
    /// Returns the authored PC increment slider as an integer step.
    /// </summary>
    public int GetPcIncrementValue()
    {
        return m_PcIncrementSlider != null ? Mathf.RoundToInt(m_PcIncrementSlider.value) : 0;
    }

    /// <summary>
    /// Converts the currently selected branch-condition dropdown item into the
    /// enum consumed by validation logic.
    /// </summary>
    public BranchConditionKind GetSelectedBranchCondition()
    {
        return m_BranchService != null && m_BranchConditionDropdown != null
            ? m_BranchService.GetSelectedBranchCondition(m_BranchConditionDropdown.value)
            : BranchConditionKind.None;
    }

    public InstructionDefinition CurrentInstruction => m_CurrentInstruction;
    public bool IsPhaseActive => m_IsPhaseActive;
    public bool IsAwaitingContinue => m_IsAwaitingContinue;
    public string BranchValue => m_BranchValue;
    public string JumpValue => m_JumpValue;
    public DataPacketToken ShiftPreparedImmediatePacket => m_ShiftPreparedImmediatePacket;
    public PcUpdatePacketScanner ImmediateScanner => m_ImmediateScanner;
    public PcUpdatePacketScanner ZeroScanner => m_ZeroScanner;
    public GameObject PcUpdateGroupRoot => m_PcUpdateGroupRoot;
    public GameObject SignalsGroupRoot => m_SignalsGroupRoot;
    public GameObject ImmediateGroupRoot => m_ImmediateGroupRoot;
    public GameObject BranchConditionGroupRoot => m_BranchConditionGroupRoot;
    public TMP_Text LessonRuntimeText => m_LessonRuntimeText;
    public TMP_Text LessonBranchText => m_LessonBranchText;
    public TMP_Text LessonShiftText => m_LessonShiftText;
    public TMP_Text LessonResultText => m_LessonResultText;
    public TMP_Text LessonEndText => m_LessonEndText;
    public TMP_Text BranchStatusText => m_BranchStatusText;
    public TMP_Text JumpStatusText => m_JumpStatusText;
    public TMP_Text ImmediateStatusText => m_ImmediateStatusText;
    public TMP_Text ZeroStatusText => m_ZeroStatusText;
    public TMP_Text PCSrcStatusText => m_PCSrcStatusText;
    public Button ActionButton => m_ActionButton;
    public TMP_Text ActionButtonLabel => m_ActionButtonLabel;
    public TMP_Dropdown HintDropdown => m_HintDropdown;
    public TMP_Text HintPcText => m_HintPcText;
    public TMP_Text HintPcSrcText => m_HintPcSrcText;
    public TMP_Text HintBranchText => m_HintBranchText;
    public TMP_Text HintJumpText => m_HintJumpText;
    public TMP_Text HintShiftLeftTwoText => m_HintShiftLeftTwoText;
    public TMP_Text HintZeroText => m_HintZeroText;
    public string ConfirmButtonText => m_ConfirmButtonText;
    public string ContinueButtonText => m_ContinueButtonText;

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
}

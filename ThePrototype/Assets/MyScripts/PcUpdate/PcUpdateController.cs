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

    [Header("Lesson Panel")]
    [SerializeField]
    PcUpdateLessonPanelRefs m_LessonPanel;

    [Header("Hint Panel")]
    [SerializeField]
    PhaseHintPanelRefs m_HintPanel;

    [SerializeField]
    PcUpdateHintInfoRefs m_LearningHints;

    [Header("Interaction Panel")]
    [SerializeField]
    GameObject m_PcUpdateUiRoot;

    [SerializeField]
    PcUpdateInteractionPanelRefs m_InteractionPanel;

    [SerializeField]
    PhaseSharedInteractionRefs m_SharedInteraction;

    [Header("Practice")]
    [SerializeField]
    int m_PracticeValidationAttempts = 3;

    [SerializeField]
    int m_PracticeScannerAttempts = 2;

    [SerializeField]
    int m_PracticeHints = 2;

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
    readonly PcUpdatePracticeFlow m_PracticeFlow = new();
    LessonMode m_CurrentMode = LessonMode.Learning;

    public event System.Action ContinueRequested;
    public event System.Action PcUpdateConfirmed;
    public event System.Action PracticeResetRequested;

    /// <summary>
    /// Builds the authored services and wires every authored UI / scanner event
    /// when the station first comes alive in the scene.
    /// </summary>
    void Awake()
    {
        m_BranchService = new PcBranchService();
        ConfigurePracticeFlow();
        PcUpdatePresentation.PopulateBranchConditionDropdown(BranchConditionDropdown);
        PcUpdatePresentation.PopulateHintDropdown(HintDropdown);
        HookPhysicalBindings(true);
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
        ConfigurePracticeFlow();
        PcUpdatePresentation.PopulateBranchConditionDropdown(BranchConditionDropdown);
        PcUpdatePresentation.PopulateHintDropdown(HintDropdown);
        HookPhysicalBindings(true);
        HookScannerEvents(true);
        RefreshPresentation();
    }

    /// <summary>
    /// Removes listener hookups while the object is disabled to avoid duplicate
    /// callbacks when Unity toggles the station on and off.
    /// </summary>
    void OnDisable()
    {
        HookPhysicalBindings(false);
        HookScannerEvents(false);
    }

    /// <summary>
    /// Called by the lesson flow whenever the PC update phase becomes active or
    /// inactive for the current instruction.
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
            PrepareForPcUpdate();
            m_PracticeFlow.Reset();
        }

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
        m_CurrentMode = LessonMode.Learning;
        m_PracticeFlow.Reset();

        if (PcIncrementSlider != null)
            PcIncrementSlider.SetValueWithoutNotify(0f);

        if (BranchConditionDropdown != null)
            BranchConditionDropdown.SetValueWithoutNotify(0);

        if (HintDropdown != null)
            HintDropdown.SetValueWithoutNotify(0);

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

        if (m_PracticeFlow.IsAwaitingReset)
        {
            PracticeResetRequested?.Invoke();
            return;
        }

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

        if (PcIncrementSlider != null)
        {
            PcIncrementSlider.minValue = 0f;
            PcIncrementSlider.maxValue = 4f;
            PcIncrementSlider.wholeNumbers = true;
            PcIncrementSlider.SetValueWithoutNotify(0f);
        }

        if (BranchConditionDropdown != null)
            BranchConditionDropdown.SetValueWithoutNotify(0);

        m_ImmediateScanner?.ResetScanner();
        m_ZeroScanner?.ResetScanner();
        SetFeedback(
            IsPracticeMode
                ? m_PracticeFlow.BuildBudgetSummary("Build the next PC path, then validate.")
                : "Move the PC update control from 0 to 4, then confirm the next PC path.",
            false);
        RefreshPresentation();
    }

    /// <summary>
    /// Marks the currently accepted branch immediate as shift-left-by-2 ready.
    /// The packet itself is not mutated; the controller simply records that the
    /// learner performed the required transformation step.
    /// </summary>
    public void HandleShiftPressed()
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
            if (BranchConditionDropdown != null)
                BranchConditionDropdown.SetValueWithoutNotify(0);
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
    public void HandleDropdownChanged(int _)
    {
        SetFeedback(string.Empty, false);
        RefreshPresentation();
    }

    /// <summary>
    /// Keeps the PC status text in sync while the learner drags the PC+4
    /// slider, rather than waiting for a later confirm press.
    /// </summary>
    public void HandleSliderChanged(float _)
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
    /// Reveals the next Practice-mode hint for Program Counter Update.
    /// </summary>
    public void HandlePracticeHintPressed()
    {
        if (!IsPracticeMode || PracticeHintText == null)
            return;

        PracticeHintText.text = m_PracticeFlow.BuildHint(this, m_BranchService);
        RefreshPresentation();
    }

    /// <summary>
    /// Keeps only the XR-side branch and jump controls code-bound. Standard UI
    /// widgets now use Inspector event hookups instead.
    /// </summary>
    void HookPhysicalBindings(bool subscribe)
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
    }

    /// <summary>
    /// Wires the two packet pedestals so packet acceptance can immediately
    /// refresh the lesson UI.
    /// </summary>
    void HookScannerEvents(bool subscribe)
    {
        HookScannerEvent(m_ImmediateScanner, HandleImmediateAccepted, HandleImmediateRejected, subscribe);
        HookScannerEvent(m_ZeroScanner, HandleZeroAccepted, HandleZeroRejected, subscribe);
    }

    /// <summary>
    /// Shared helper for attaching or detaching a packet-scanner callback.
    /// </summary>
    void HookScannerEvent(
        PcUpdatePacketScanner scanner,
        System.Action<PcUpdatePacketScanner, DataPacketToken> handler,
        System.Action<PcUpdatePacketScanner, DataPacketToken, PcUpdatePacketScanner.PacketIssue> rejectedHandler,
        bool subscribe)
    {
        if (scanner == null)
            return;

        scanner.PacketAccepted -= handler;
        scanner.PacketRejected -= rejectedHandler;
        if (subscribe)
        {
            scanner.PacketAccepted += handler;
            scanner.PacketRejected += rejectedHandler;
        }
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
    void SetFeedback(string message, bool isFailure, bool playIncorrectCue = true)
    {
        PcUpdatePresentation.SetFeedback(m_SharedInteraction.FeedbackText, message, isFailure, m_SuccessFeedbackColor, m_FailureFeedbackColor);

        if (playIncorrectCue && isFailure && !string.IsNullOrWhiteSpace(message))
            PlayIncorrectCue();
    }

    /// <summary>
    /// Returns the authored PC increment slider as an integer step.
    /// </summary>
    public int GetPcIncrementValue()
    {
        return PcIncrementSlider != null ? Mathf.RoundToInt(PcIncrementSlider.value) : 0;
    }

    /// <summary>
    /// Converts the currently selected branch-condition dropdown item into the
    /// enum consumed by validation logic.
    /// </summary>
    public BranchConditionKind GetSelectedBranchCondition()
    {
        return m_BranchService != null && BranchConditionDropdown != null
            ? m_BranchService.GetSelectedBranchCondition(BranchConditionDropdown.value)
            : BranchConditionKind.None;
    }

    void HandleImmediateRejected(PcUpdatePacketScanner _, DataPacketToken packetToken, PcUpdatePacketScanner.PacketIssue issue)
    {
        if (!IsPracticeMode || !m_IsPhaseActive || m_IsAwaitingContinue || IsPracticeAwaitingReset)
            return;

        var didFail = m_PracticeFlow.HandleImmediateScannerFailure(packetToken, issue, out var feedbackText);
        if (didFail)
            EnterPracticeFailureState(feedbackText);
        else
            SetFeedback(feedbackText, true);
        RefreshPresentation();
    }

    void HandleZeroRejected(PcUpdatePacketScanner _, DataPacketToken packetToken, PcUpdatePacketScanner.PacketIssue issue)
    {
        if (!IsPracticeMode || !m_IsPhaseActive || m_IsAwaitingContinue || IsPracticeAwaitingReset)
            return;

        var didFail = m_PracticeFlow.HandleZeroScannerFailure(packetToken, out var feedbackText);
        if (didFail)
            EnterPracticeFailureState(feedbackText);
        else
            SetFeedback(feedbackText, true);
        RefreshPresentation();
    }

    public InstructionDefinition CurrentInstruction => m_CurrentInstruction;
    public bool IsPhaseActive => m_IsPhaseActive;
    public bool IsPracticeMode => m_CurrentMode == LessonMode.Practice;
    public bool IsPracticeAwaitingReset => m_PracticeFlow.IsAwaitingReset;
    public bool IsAwaitingContinue => m_IsAwaitingContinue;
    public string BranchValue => m_BranchValue;
    public string JumpValue => m_JumpValue;
    public DataPacketToken ShiftPreparedImmediatePacket => m_ShiftPreparedImmediatePacket;
    public PcUpdatePacketScanner ImmediateScanner => m_ImmediateScanner;
    public PcUpdatePacketScanner ZeroScanner => m_ZeroScanner;
    public GameObject PcUpdateGroupRoot => m_InteractionPanel.PcUpdateGroupRoot;
    public GameObject SignalsGroupRoot => m_InteractionPanel.SignalsGroupRoot;
    public GameObject ImmediateGroupRoot => m_InteractionPanel.ImmediateGroupRoot;
    public GameObject BranchConditionGroupRoot => m_InteractionPanel.BranchConditionGroupRoot;
    public TMP_Text LessonRuntimeText => m_LessonPanel.RuntimeText;
    public TMP_Text LessonBranchText => m_LessonPanel.BranchText;
    public TMP_Text LessonShiftText => m_LessonPanel.ShiftText;
    public TMP_Text LessonResultText => m_LessonPanel.ResultText;
    public TMP_Text LessonEndText => m_LessonPanel.EndText;
    public TMP_Text BranchStatusText => m_InteractionPanel.BranchStatusText;
    public TMP_Text JumpStatusText => m_InteractionPanel.JumpStatusText;
    public TMP_Text ImmediateStatusText => m_InteractionPanel.ImmediateStatusText;
    public TMP_Text ZeroStatusText => m_InteractionPanel.ZeroStatusText;
    public TMP_Text PCSrcStatusText => m_InteractionPanel.PCSrcStatusText;
    public Button ActionButton => m_SharedInteraction.ActionButton;
    public TMP_Text ActionButtonLabel => m_SharedInteraction.ActionLabel;
    public TMP_Dropdown HintDropdown => m_HintPanel.InfoDropdown;
    public TMP_Text HintPcText => m_LearningHints.PcText;
    public TMP_Text HintPcSrcText => m_LearningHints.PcSrcText;
    public TMP_Text HintBranchText => m_LearningHints.BranchText;
    public TMP_Text HintJumpText => m_LearningHints.JumpText;
    public TMP_Text HintShiftLeftTwoText => m_LearningHints.ShiftLeftTwoText;
    public TMP_Text HintZeroText => m_LearningHints.ZeroText;
    public Slider PcIncrementSlider => m_InteractionPanel.PcIncrementSlider;
    public TMP_Dropdown BranchConditionDropdown => m_InteractionPanel.BranchConditionDropdown;
    public Button ShiftButton => m_InteractionPanel.ShiftButton;
    public Button PracticeHintButton => m_HintPanel.HintButton;
    public TMP_Text PracticeHintText => m_HintPanel.HintText;
    public PhaseHintPanelRefs HintPanel => m_HintPanel;
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

    public void PlayFailureCue()
    {
        m_LessonAudioCues.PlayFailureCue();
    }

    /// <summary>
    /// Dev-mode helper that treats the current PC update as complete without
    /// requiring the learner to configure the station.
    /// </summary>
    public void DevForceCompletePhase()
    {
        if (!m_IsPhaseActive)
            return;

        PcUpdateConfirmed?.Invoke();
        ContinueRequested?.Invoke();
    }

    void ConfigurePracticeFlow()
    {
        m_PracticeFlow.Configure(m_PracticeValidationAttempts, m_PracticeScannerAttempts, m_PracticeHints);
    }

    /// <summary>
    /// Matches the normal two-step phase ending flow, but for Practice failure.
    /// The cue plays immediately and Restart confirms the reset.
    /// </summary>
    void EnterPracticeFailureState(string feedbackText)
    {
        m_IsAwaitingContinue = false;
        m_ImmediateScanner?.SetActive(false);
        m_ZeroScanner?.SetActive(false);
        SetFeedback(m_PracticeFlow.BuildFailureResetText(feedbackText), true, false);
        PlayFailureCue();
    }
}

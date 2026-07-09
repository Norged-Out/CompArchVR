using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Owns the authored Program Counter update station and UI.
/// For the current slice it finalizes the lesson by verifying the normal
/// sequential PC update (PC + 4). Branch / jump support is scaffolded here so
/// later instructions can reuse the same station instead of inventing a new one.
/// </summary>
[DisallowMultipleComponent]
public partial class PcUpdateController : MonoBehaviour
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

    public event System.Action ContinueRequested;

    void Awake()
    {
        CacheReferences();
        PopulateDropdown();
        PopulateHintDropdown();
        HookButtons(true);
        HookDropdown(true);
        HookSlider(true);
        HookScannerEvents(true);
        RefreshPresentation();
    }

    void OnEnable()
    {
        CacheReferences();
        PopulateDropdown();
        PopulateHintDropdown();
        HookButtons(true);
        HookDropdown(true);
        HookSlider(true);
        HookScannerEvents(true);
        RefreshPresentation();
    }

    void OnDisable()
    {
        HookButtons(false);
        HookDropdown(false);
        HookSlider(false);
        HookScannerEvents(false);
    }

    public void SetPhaseState(bool isActive, InstructionDefinition instruction)
    {
        CacheReferences();

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

        if (!TryValidateSetup(out var validationMessage))
        {
            SetFeedback(validationMessage, true);
            RefreshPresentation();
            return;
        }

        m_IsAwaitingContinue = true;
        SetFeedback("Program Counter update confirmed. Press Continue to reset the lesson.", false);
        RefreshPresentation();
    }
}

using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// Owns the authored Program Counter update station and UI.
/// For the current slice it finalizes the lesson by verifying the normal
/// sequential PC update (PC + 4). Branch / jump support is scaffolded here so
/// later instructions can reuse the same station instead of inventing a new one.
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

    public event System.Action ContinueRequested;

    void Awake()
    {
        CacheReferences();
        PopulateDropdown();
        PopulateHintDropdown();
        HookButtons(true);
        HookDropdown(true);
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
        HookScannerEvents(true);
        RefreshPresentation();
    }

    void OnDisable()
    {
        HookButtons(false);
        HookDropdown(false);
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

        var showBranchSpecificGroups = isActive && m_BranchValue == "1";
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
        SetFeedback("PC update confirmed. Click Continue to finish the lesson.", false);
        RefreshPresentation();
    }

    void PrepareForPcUpdate()
    {
        m_IsAwaitingContinue = false;
        m_BranchValue = "0";
        m_JumpValue = "0";

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
        m_ImmediateScanner?.SetExpectedPacketRole(DataPacketRole.Immediate);
        m_ImmediateScanner?.SetImmediateRequirements(true, false);
        m_ZeroScanner?.SetExpectedPacketRole(DataPacketRole.Zero);
        SetFeedback("Move the PC update control from 0 to 4, then confirm the next PC path.", false);
        RefreshPresentation();
    }

    bool TryValidateSetup(out string validationMessage)
    {
        validationMessage = string.Empty;

        if (Mathf.RoundToInt(GetPcIncrementValue()) != 4)
        {
            validationMessage = "PC + 4 is not set yet.";
            return false;
        }

        if (m_CurrentInstruction == null)
            return true;

        var expectedBranch = m_CurrentInstruction.GetExpectedBranchControlValue();
        if (m_BranchValue != expectedBranch)
        {
            validationMessage = "Branch does not match this instruction.";
            return false;
        }

        var expectedJump = m_CurrentInstruction.GetExpectedJumpControlValue();
        if (m_JumpValue != expectedJump)
        {
            validationMessage = "Jump does not match this instruction.";
            return false;
        }

        if (m_CurrentInstruction.UsesBranchDecision())
        {
            if (m_ImmediateScanner == null || m_ImmediateScanner.AcceptedPacket == null)
            {
                validationMessage = "The branch offset packet is still missing.";
                return false;
            }

            if (!m_ImmediateScanner.AcceptedPacket.IsShiftedLeftTwo)
            {
                validationMessage = "Shift the branch immediate left by 2 before confirming.";
                return false;
            }

            if (m_ZeroScanner == null || m_ZeroScanner.AcceptedPacket == null)
            {
                validationMessage = "The zero-result packet is still missing.";
                return false;
            }

            if (GetSelectedBranchCondition() != m_CurrentInstruction.GetExpectedBranchCondition())
            {
                validationMessage = "Branch condition does not match this instruction.";
                return false;
            }
        }

        return true;
    }

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

        if (!immediatePacket.IsShiftedLeftTwo)
            immediatePacket.MarkShiftedLeftTwo(immediatePacket.Value << 2);

        m_ImmediateScanner.SetImmediateRequirements(true, true);
        m_ImmediateScanner.ResetScanner();
        SetFeedback("Branch offset shifted left by 2. Place it back on the PC update station.", false);
        RefreshPresentation();
    }

    void HandleBranchPressed(SelectEnterEventArgs _)
    {
        if (!m_IsPhaseActive || m_IsAwaitingContinue)
            return;

        m_BranchValue = m_BranchValue == "1" ? "0" : "1";
        if (m_BranchValue != "1")
        {
            m_ImmediateScanner?.ResetScanner();
            m_ZeroScanner?.ResetScanner();
            if (m_BranchConditionDropdown != null)
                m_BranchConditionDropdown.SetValueWithoutNotify(0);
        }

        SetFeedback(string.Empty, false);
        RefreshPresentation();
    }

    void HandleJumpPressed(SelectEnterEventArgs _)
    {
        if (!m_IsPhaseActive || m_IsAwaitingContinue)
            return;

        m_JumpValue = m_JumpValue == "1" ? "0" : "1";
        SetFeedback(string.Empty, false);
        RefreshPresentation();
    }

    void HandleDropdownChanged(int _)
    {
        SetFeedback(string.Empty, false);
        RefreshPresentation();
    }

    void HandleImmediateAccepted(PcUpdatePacketScanner _, DataPacketToken __)
    {
        SetFeedback(string.Empty, false);
        RefreshPresentation();
    }

    void HandleZeroAccepted(PcUpdatePacketScanner _, DataPacketToken __)
    {
        SetFeedback(string.Empty, false);
        RefreshPresentation();
    }

    void RefreshPresentation()
    {
        CacheReferences();

        var showBranchSpecificGroups = m_IsPhaseActive && m_BranchValue == "1";
        if (m_ImmediateGroupRoot != null)
            m_ImmediateGroupRoot.SetActive(showBranchSpecificGroups);

        if (m_BranchConditionGroupRoot != null)
            m_BranchConditionGroupRoot.SetActive(showBranchSpecificGroups);

        m_ImmediateScanner?.SetActive(showBranchSpecificGroups);
        m_ZeroScanner?.SetActive(showBranchSpecificGroups);
        m_ImmediateScanner?.SetImmediateRequirements(true, false);
        m_ZeroScanner?.SetExpectedPacketRole(DataPacketRole.Zero);

        if (m_BranchStatusText != null)
            m_BranchStatusText.text = $"Branch: {m_BranchValue}";

        if (m_JumpStatusText != null)
            m_JumpStatusText.text = $"Jump: {m_JumpValue}";

        RefreshLessonBlocks();

        if (m_ImmediateStatusText != null)
            m_ImmediateStatusText.text = BuildImmediateStatusText();

        if (m_ZeroStatusText != null)
            m_ZeroStatusText.text = BuildZeroStatusText();

        if (m_PCSrcStatusText != null)
            m_PCSrcStatusText.text = BuildPcSrcStatusText();

        if (m_ActionButton != null)
            m_ActionButton.interactable = m_IsPhaseActive;

        if (m_ActionButtonLabel != null)
            m_ActionButtonLabel.text = m_IsAwaitingContinue ? m_ContinueButtonText : m_ConfirmButtonText;

        RefreshHintBlocks();
    }

    string BuildImmediateStatusText()
    {
        if (m_BranchValue != "1")
            return "Waiting";

        if (m_ImmediateScanner == null || m_ImmediateScanner.AcceptedPacket == null)
        {
            return m_ImmediateScanner != null
                ? m_ImmediateScanner.CurrentIssue switch
            {
                PcUpdatePacketScanner.PacketIssue.ImmediateNotSignExtended => "Not extended",
                PcUpdatePacketScanner.PacketIssue.ImmediateNotShifted => "Not shifted",
                _ => "Waiting",
            }
                : "Waiting";
        }

        var packet = m_ImmediateScanner.AcceptedPacket;
        if (!packet.IsSignExtended)
            return "Not extended";

        if (!packet.IsShiftedLeftTwo)
            return "Not shifted";

        return "Ready";
    }

    string BuildZeroStatusText()
    {
        if (m_BranchValue != "1")
            return "Zero: n/a";

        if (m_ZeroScanner == null || m_ZeroScanner.AcceptedPacket == null)
            return "Zero: waiting";

        return $"Zero: {m_ZeroScanner.AcceptedPacket.Value}";
    }

    string BuildPcSrcStatusText()
    {
        var pcIncrement = Mathf.RoundToInt(GetPcIncrementValue());

        if (m_CurrentInstruction == null || !m_CurrentInstruction.UsesBranchDecision())
            return $"PCSrc = 0\nNext PC: PC + {pcIncrement}";

        var zeroValue = m_ZeroScanner != null && m_ZeroScanner.AcceptedPacket != null
            ? m_ZeroScanner.AcceptedPacket.Value
            : 0;

        var selectedCondition = GetSelectedBranchCondition();
        var conditionMet = selectedCondition switch
        {
            BranchConditionKind.Equal => zeroValue == 1,
            BranchConditionKind.NotEqual => zeroValue == 0,
            _ => false,
        };

        var pcSrc = m_BranchValue == "1" && conditionMet ? 1 : 0;
        var nextPc = pcSrc == 1 ? "branch target" : $"PC + {pcIncrement}";
        return $"PCSrc = Branch({m_BranchValue}) AND ConditionMet({(conditionMet ? 1 : 0)}) = {pcSrc}\nNext PC: {nextPc}";
    }

    float GetPcIncrementValue()
    {
        return m_PcIncrementSlider != null ? m_PcIncrementSlider.value : 0f;
    }

    BranchConditionKind GetSelectedBranchCondition()
    {
        if (m_BranchConditionDropdown == null)
            return BranchConditionKind.None;

        return m_BranchConditionDropdown.value switch
        {
            1 => BranchConditionKind.Equal,
            2 => BranchConditionKind.NotEqual,
            _ => BranchConditionKind.None,
        };
    }

    void PopulateDropdown()
    {
        if (m_BranchConditionDropdown == null)
            return;

        var selectedValue = Mathf.Clamp(m_BranchConditionDropdown.value, 0, 2);
        m_BranchConditionDropdown.ClearOptions();
        m_BranchConditionDropdown.AddOptions(new System.Collections.Generic.List<string>
        {
            "Choose Condition",
            "Equal",
            "Not Equal",
        });
        m_BranchConditionDropdown.SetValueWithoutNotify(selectedValue);
    }

    void PopulateHintDropdown()
    {
        if (m_HintDropdown == null)
            return;

        var selectedValue = Mathf.Clamp(m_HintDropdown.value, 0, 6);
        m_HintDropdown.ClearOptions();
        m_HintDropdown.AddOptions(new System.Collections.Generic.List<string>
        {
            "Choose Option",
            "PC",
            "PCSrc",
            "Branch",
            "Jump",
            "Shift Left 2",
            "Zero",
        });
        m_HintDropdown.SetValueWithoutNotify(selectedValue);
    }

    void SetFeedback(string message, bool isFailure)
    {
        if (m_FeedbackText == null)
            return;

        m_FeedbackText.text = message;
        m_FeedbackText.color = isFailure ? m_FailureFeedbackColor : m_SuccessFeedbackColor;
        m_FeedbackText.gameObject.SetActive(!string.IsNullOrWhiteSpace(message));
    }

    void HookButtons(bool subscribe)
    {
        HookPhysicalButton(m_BranchButtonRoot, HandleBranchPressed, subscribe);
        HookPhysicalButton(m_JumpButtonRoot, HandleJumpPressed, subscribe);

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

    void HookScannerEvents(bool subscribe)
    {
        HookScannerEvent(m_ImmediateScanner, HandleImmediateAccepted, subscribe);
        HookScannerEvent(m_ZeroScanner, HandleZeroAccepted, subscribe);
    }

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

    void CacheReferences()
    {
        m_ImmediateScanner ??= FindChildComponent<PcUpdatePacketScanner>(transform, "Immediate Input");
        m_ZeroScanner ??= FindChildComponent<PcUpdatePacketScanner>(transform, "Zero Input");
        m_BranchButtonRoot ??= FindChildTransform(transform, "Branch Button");
        m_JumpButtonRoot ??= FindChildTransform(transform, "Jump Button");

        if (m_PcUpdateUiRoot == null)
        {
            var pcUiTransform = FindSceneTransformByName("PC Update UI");
            m_PcUpdateUiRoot = pcUiTransform != null ? pcUiTransform.gameObject : null;
        }

        if (m_PcUpdateUiRoot != null)
        {
            m_LessonRuntimeText ??= FindNamedText(m_PcUpdateUiRoot.transform, "Runtime text");
            m_LessonBranchText ??= FindNamedText(m_PcUpdateUiRoot.transform, "Branch");
            m_LessonShiftText ??= FindNamedText(m_PcUpdateUiRoot.transform, "Shift");
            m_LessonResultText ??= FindNamedText(m_PcUpdateUiRoot.transform, "Result");
            m_LessonEndText ??= FindNamedText(m_PcUpdateUiRoot.transform, "End text");
        }
    }

    static void HookPhysicalButton(Transform buttonRoot, UnityEngine.Events.UnityAction<SelectEnterEventArgs> handler, bool subscribe)
    {
        if (buttonRoot == null)
            return;

        var interactable = buttonRoot.GetComponentInChildren<XRSimpleInteractable>(true);
        if (interactable == null)
            return;

        interactable.selectEntered.RemoveListener(handler);
        if (subscribe)
            interactable.selectEntered.AddListener(handler);
    }

    static T FindChildComponent<T>(Transform root, string childName) where T : Component
    {
        var childTransform = FindChildTransform(root, childName);
        return childTransform != null ? childTransform.GetComponent<T>() : null;
    }

    static Transform FindChildTransform(Transform root, string childName)
    {
        if (root == null)
            return null;

        foreach (var childTransform in root.GetComponentsInChildren<Transform>(true))
        {
            if (childTransform != null && childTransform.name == childName)
                return childTransform;
        }

        return null;
    }

    static Transform FindSceneTransformByName(string objectName)
    {
        foreach (var sceneTransform in Resources.FindObjectsOfTypeAll<Transform>())
        {
            if (sceneTransform == null || sceneTransform.name != objectName)
                continue;

            if (!sceneTransform.gameObject.scene.IsValid() || !sceneTransform.gameObject.scene.isLoaded)
                continue;

            return sceneTransform;
        }

        return null;
    }

    void RefreshLessonBlocks()
    {
        var showEndState = m_IsAwaitingContinue;

        SetTextActive(m_LessonRuntimeText, !showEndState);
        SetTextActive(m_LessonBranchText, !showEndState && ShouldShowBranchLesson());
        SetTextActive(m_LessonShiftText, !showEndState && ShouldShowShiftLesson());
        SetTextActive(m_LessonResultText, !showEndState && ShouldShowResultLesson());
        SetTextActive(m_LessonEndText, showEndState);

        if (m_LessonRuntimeText != null)
            m_LessonRuntimeText.text = BuildLessonRuntimeText();

        if (m_LessonEndText != null)
            m_LessonEndText.text = BuildLessonEndText();
    }

    bool ShouldShowBranchLesson()
    {
        return m_CurrentInstruction != null && m_CurrentInstruction.UsesBranchDecision();
    }

    bool ShouldShowShiftLesson()
    {
        return m_CurrentInstruction != null && m_CurrentInstruction.UsesBranchDecision();
    }

    bool ShouldShowResultLesson()
    {
        return m_CurrentInstruction != null && m_CurrentInstruction.UsesBranchDecision();
    }

    string BuildLessonRuntimeText()
    {
        if (m_CurrentInstruction == null)
            return string.Empty;

        if (m_CurrentInstruction.UsesBranchDecision())
        {
            return "Use the control outputs and datapath results from earlier stages to decide whether the Program Counter keeps the sequential path or takes the branch target.";
        }

        if (m_CurrentInstruction.UsesJumpDecision())
        {
            return "Use the final control signals to decide whether the Program Counter follows the normal sequential path or jumps elsewhere.";
        }

        return "Close the datapath cycle by confirming the normal sequential Program Counter update for this instruction.";
    }

    string BuildLessonEndText()
    {
        return "Program Counter update confirmed. Continue to finish the lesson.";
    }

    void RefreshHintBlocks()
    {
        var selectedHint = m_HintDropdown != null ? m_HintDropdown.value : 0;
        SetTextActive(m_HintPcText, selectedHint == 1);
        SetTextActive(m_HintPcSrcText, selectedHint == 2);
        SetTextActive(m_HintBranchText, selectedHint == 3);
        SetTextActive(m_HintJumpText, selectedHint == 4);
        SetTextActive(m_HintShiftLeftTwoText, selectedHint == 5);
        SetTextActive(m_HintZeroText, selectedHint == 6);
    }

    static TMP_Text FindNamedText(Transform root, string objectName)
    {
        if (root == null)
            return null;

        foreach (var textMesh in root.GetComponentsInChildren<TMP_Text>(true))
        {
            if (textMesh != null && textMesh.name == objectName)
                return textMesh;
        }

        return null;
    }

    static void SetTextActive(TMP_Text textBlock, bool isActive)
    {
        if (textBlock == null)
            return;

        textBlock.gameObject.SetActive(isActive);
    }
}

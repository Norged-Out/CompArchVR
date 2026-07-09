using TMPro;
using UnityEngine;

/// <summary>
/// Presentation helpers for lesson text, status text, and hint visibility in PC update.
/// </summary>
public partial class PcUpdateController
{
    void RefreshPresentation()
    {
        CacheReferences();

        var showEndState = m_IsAwaitingContinue;
        var showBranchSpecificGroups = m_IsPhaseActive && m_BranchValue == "1" && !showEndState;

        if (m_PcUpdateGroupRoot != null)
            m_PcUpdateGroupRoot.SetActive(!showEndState);

        if (m_SignalsGroupRoot != null)
            m_SignalsGroupRoot.SetActive(!showEndState);

        if (m_ImmediateGroupRoot != null)
            m_ImmediateGroupRoot.SetActive(showBranchSpecificGroups);

        if (m_BranchConditionGroupRoot != null)
            m_BranchConditionGroupRoot.SetActive(showBranchSpecificGroups);

        m_ImmediateScanner?.SetActive(showBranchSpecificGroups);
        m_ZeroScanner?.SetActive(showBranchSpecificGroups);
        m_ImmediateScanner?.SetImmediateRequirements(true);
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
                    _ => "Waiting",
                }
                : "Waiting";
        }

        var packet = m_ImmediateScanner.AcceptedPacket;
        if (!packet.IsSignExtended)
            return "Not extended";

        if (packet != m_ShiftPreparedImmediatePacket)
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
        var nextPc = pcSrc == 1 ? $"PC + {pcIncrement} + Branch Offset" : $"PC + {pcIncrement}";
        return $"PCSrc = Branch({m_BranchValue}) AND ConditionMet({(conditionMet ? 1 : 0)}) = {pcSrc}\nNext PC: {nextPc}";
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

    static void SetTextActive(TMP_Text textBlock, bool isActive)
    {
        if (textBlock == null)
            return;

        textBlock.gameObject.SetActive(isActive);
    }
}

using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Program-counter update validation and branch/jump decision logic.
/// </summary>
public partial class PcUpdateController
{
    void PrepareForPcUpdate()
    {
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

            if (m_ImmediateScanner.AcceptedPacket != m_ShiftPreparedImmediatePacket)
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

        m_ShiftPreparedImmediatePacket = immediatePacket;
        SetFeedback("Branch offset shifted left by 2.", false);
        RefreshPresentation();
    }

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

    void HandleSliderChanged(float _)
    {
        if (!m_IsPhaseActive)
            return;

        RefreshPresentation();
    }

    void HandleImmediateAccepted(PcUpdatePacketScanner _, DataPacketToken __)
    {
        if (m_ImmediateScanner == null || m_ImmediateScanner.AcceptedPacket != m_ShiftPreparedImmediatePacket)
            m_ShiftPreparedImmediatePacket = null;

        SetFeedback(string.Empty, false);
        RefreshPresentation();
    }

    void HandleZeroAccepted(PcUpdatePacketScanner _, DataPacketToken __)
    {
        SetFeedback(string.Empty, false);
        RefreshPresentation();
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
            "Choose Option",
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
}

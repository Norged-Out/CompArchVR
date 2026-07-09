using TMPro;
using UnityEngine;

/// <summary>
/// Presentation helpers for write-back status text and hint blocks.
/// </summary>
public partial class WriteBackController
{
    void RefreshPresentation()
    {
        CacheReferences();
        RefreshExpectedTargets();

        if (m_LessonRuntimeText != null)
            m_LessonRuntimeText.text = BuildLessonRuntimeText();

        if (m_RegWriteStatusText != null)
            m_RegWriteStatusText.text = $"RegWrite: {m_RegWriteValue}";

        if (m_RegDstStatusText != null)
            m_RegDstStatusText.text = $"RegDst: {m_RegDstValue}";

        if (m_MemToRegStatusText != null)
            m_MemToRegStatusText.text = $"MemToReg: {m_MemToRegValue}";

        if (m_RegisterStatusText != null)
        {
            if (m_HasAppliedWriteBack)
            {
                m_RegisterStatusText.text = $"Register Target: {m_LastTargetRegister}";
            }
            else if (m_RegisterScanner == null || m_RegisterScanner.AcceptedRegister == null)
            {
                m_RegisterStatusText.text = $"Register Target: waiting for {GetExpectedRegisterIdFromControlState()}";
            }
            else
            {
                m_RegisterStatusText.text = $"Register Target: {m_RegisterScanner.AcceptedRegister.RegisterId}";
            }
        }

        if (m_DataStatusText != null)
        {
            if (m_HasAppliedWriteBack)
            {
                m_DataStatusText.text = $"Write Data: {GetPacketRoleDisplayName(m_LastTransferredPacketRole)} = {m_LastTransferredValue}";
            }
            else if (m_PacketScanner == null || m_PacketScanner.AcceptedPacket == null)
            {
                m_DataStatusText.text = $"Write Data: waiting for {GetPacketRoleDisplayName(GetExpectedPacketRoleFromControlState())}";
            }
            else
            {
                m_DataStatusText.text =
                    $"Write Data: {GetPacketRoleDisplayName(m_PacketScanner.AcceptedPacket.PacketRole)} = {m_PacketScanner.AcceptedPacket.Value}";
            }
        }

        if (m_ActionButtonLabel != null)
            m_ActionButtonLabel.text = m_IsAwaitingContinue ? m_ContinueButtonText : m_ExecuteButtonText;

        if (m_ActionButton != null)
            m_ActionButton.interactable = m_IsPhaseActive && m_TransferRoutine == null;

        RefreshHintBlocks();
    }

    string BuildLessonRuntimeText()
    {
        var instructionName = m_CurrentInstruction != null ? m_CurrentInstruction.displayName : "instruction";
        var assembly = m_CurrentInstruction != null ? m_CurrentInstruction.assemblyInstructionText : "add t2, t0, t1";

        return $"Instruction: {instructionName}\nAssembly: {assembly}";
    }

    void SetFeedback(string message, bool isFailure)
    {
        if (m_FeedbackText == null)
            return;

        m_FeedbackText.text = message;
        m_FeedbackText.color = isFailure ? m_FailureFeedbackColor : m_SuccessFeedbackColor;
        m_FeedbackText.gameObject.SetActive(!string.IsNullOrWhiteSpace(message));
    }

    void RefreshHintBlocks()
    {
        var selectedHint = m_HintDropdown != null ? m_HintDropdown.value : 0;

        SetHintBlockActive(m_HintRegDstText, selectedHint == 1);
        SetHintBlockActive(m_HintRegWriteText, selectedHint == 2);
        SetHintBlockActive(m_HintMemToRegText, selectedHint == 3);
    }

    void PopulateHintDropdown()
    {
        if (m_HintDropdown == null)
            return;

        var selectedValue = Mathf.Clamp(m_HintDropdown.value, 0, 3);
        m_HintDropdown.ClearOptions();
        m_HintDropdown.AddOptions(new System.Collections.Generic.List<string>
        {
            "Choose Option",
            "RegDst",
            "RegWrite",
            "MemToReg",
        });
        m_HintDropdown.SetValueWithoutNotify(selectedValue);
    }

    static string GetPacketRoleDisplayName(DataPacketRole packetRole)
    {
        return packetRole switch
        {
            DataPacketRole.ReadData1 => "Read Data 1",
            DataPacketRole.ReadData2 => "Read Data 2",
            DataPacketRole.Immediate => "Immediate",
            DataPacketRole.AluResult => "ALU Result",
            DataPacketRole.MemoryData => "Memory Data",
            _ => "Packet",
        };
    }

    static void SetHintBlockActive(TMP_Text textBlock, bool isActive)
    {
        if (textBlock == null)
            return;

        textBlock.gameObject.SetActive(isActive);
    }
}

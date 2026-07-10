using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Presentation-only helper for the authored WB UI.
/// Keeping this separate lets the controller focus on phase state instead of
/// hand-building text and hint visibility.
/// </summary>
public static class WriteBackPresentation
{
    /// <summary>
    /// Rebuilds the entire WB UI from the controller's current phase state.
    /// </summary>
    public static void Refresh(WriteBackController controller)
    {
        if (controller == null)
            return;

        if (controller.LessonRuntimeText != null)
            controller.LessonRuntimeText.text = BuildLessonRuntimeText(controller.CurrentInstruction);

        if (controller.RegWriteStatusText != null)
            controller.RegWriteStatusText.text = $"RegWrite: {controller.RegWriteValue}";

        if (controller.RegDstStatusText != null)
            controller.RegDstStatusText.text = $"RegDst: {controller.RegDstValue}";

        if (controller.MemToRegStatusText != null)
            controller.MemToRegStatusText.text = $"MemToReg: {controller.MemToRegValue}";

        if (controller.RegisterStatusText != null)
        {
            if (controller.HasAppliedWriteBack)
            {
                controller.RegisterStatusText.text = $"Register Target: {controller.LastTargetRegister}";
            }
            else if (controller.AcceptedRegister == null)
            {
                controller.RegisterStatusText.text =
                    $"Register Target: waiting for {controller.GetExpectedRegisterIdFromControlState()}";
            }
            else
            {
                controller.RegisterStatusText.text =
                    $"Register Target: {controller.AcceptedRegister.RegisterId}";
            }
        }

        if (controller.DataStatusText != null)
        {
            if (controller.HasAppliedWriteBack)
            {
                controller.DataStatusText.text =
                    $"Write Data: {GetPacketRoleDisplayName(controller.LastTransferredPacketRole)} = {controller.LastTransferredValue}";
            }
            else if (controller.AcceptedPacket == null)
            {
                controller.DataStatusText.text =
                    $"Write Data: waiting for {GetPacketRoleDisplayName(controller.GetExpectedPacketRoleFromControlState())}";
            }
            else
            {
                controller.DataStatusText.text =
                    $"Write Data: {GetPacketRoleDisplayName(controller.AcceptedPacket.PacketRole)} = {controller.AcceptedPacket.Value}";
            }
        }

        if (controller.ActionButtonLabel != null)
            controller.ActionButtonLabel.text = controller.IsAwaitingContinue
                ? controller.ContinueButtonText
                : controller.ExecuteButtonText;

        if (controller.ActionButton != null)
            controller.ActionButton.interactable = controller.IsPhaseActive && !controller.IsTransferRunning;

        RefreshHintBlocks(controller.HintDropdown, controller.HintRegDstText, controller.HintRegWriteText, controller.HintMemToRegText);
    }

    /// <summary>
    /// Updates the authored feedback field color and active state.
    /// </summary>
    public static void SetFeedback(TMP_Text feedbackText, string message, bool isFailure, Color successColor, Color failureColor)
    {
        if (feedbackText == null)
            return;

        feedbackText.text = message;
        feedbackText.color = isFailure ? failureColor : successColor;
        feedbackText.gameObject.SetActive(!string.IsNullOrWhiteSpace(message));
    }

    /// <summary>
    /// Rebuilds the fixed WB hint dropdown options if needed.
    /// </summary>
    public static void PopulateHintDropdown(TMP_Dropdown hintDropdown)
    {
        if (hintDropdown == null)
            return;

        var selectedValue = Mathf.Clamp(hintDropdown.value, 0, 3);
        hintDropdown.ClearOptions();
        hintDropdown.AddOptions(new List<string>
        {
            "Choose Option",
            "RegDst",
            "RegWrite",
            "MemToReg",
        });
        hintDropdown.SetValueWithoutNotify(selectedValue);
    }

    static string BuildLessonRuntimeText(InstructionDefinition instruction)
    {
        var instructionName = instruction != null ? instruction.displayName : "instruction";
        var assembly = instruction != null ? instruction.assemblyInstructionText : "add t2, t0, t1";
        return $"Instruction: {instructionName}\nAssembly: {assembly}";
    }

    static void RefreshHintBlocks(TMP_Dropdown hintDropdown, TMP_Text regDstText, TMP_Text regWriteText, TMP_Text memToRegText)
    {
        // Only one authored hint block is shown at a time so the hint panel
        // behaves like a lightweight reference card rather than a wall of text.
        var selectedHint = hintDropdown != null ? hintDropdown.value : 0;
        SetHintBlockActive(regDstText, selectedHint == 1);
        SetHintBlockActive(regWriteText, selectedHint == 2);
        SetHintBlockActive(memToRegText, selectedHint == 3);
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
            DataPacketRole.Zero => "Zero",
            _ => "Packet",
        };
    }

    static void SetHintBlockActive(TMP_Text textBlock, bool isActive)
    {
        if (textBlock != null)
            textBlock.gameObject.SetActive(isActive);
    }
}

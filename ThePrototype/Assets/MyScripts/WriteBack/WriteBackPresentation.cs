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
            controller.LessonRuntimeText.text = controller.IsPracticeMode
                ? "Practice Mode\nComplete the Write Back phase using the instruction you decoded earlier."
                : BuildLessonRuntimeText(controller.CurrentInstruction);

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
                controller.RegisterStatusText.text = BuildRegisterStatusText(
                    controller.LastTargetRegister,
                    controller.RegisterBank != null ? controller.RegisterBank.GetRegisterValue(controller.LastTargetRegister) : 0,
                    false,
                    controller.IsPracticeMode);
            }
            else if (controller.AcceptedRegister == null)
            {
                var expectedRegisterId = controller.GetExpectedRegisterIdFromControlState();
                var currentValue = controller.RegisterBank != null ? controller.RegisterBank.GetRegisterValue(expectedRegisterId) : 0;
                controller.RegisterStatusText.text = BuildRegisterStatusText(expectedRegisterId, currentValue, true, controller.IsPracticeMode);
            }
            else
            {
                controller.RegisterStatusText.text = BuildRegisterStatusText(
                    controller.AcceptedRegister.RegisterId,
                    controller.AcceptedRegister.RegisterValue,
                    false,
                    controller.IsPracticeMode);
            }
        }

        if (controller.DataStatusText != null)
        {
            if (controller.HasAppliedWriteBack)
            {
                controller.DataStatusText.text =
                    controller.IsPracticeMode ? $"Packet Value: {controller.LastTransferredValue}" : $"Write Data: {controller.LastTransferredValue}";
            }
            else if (controller.AcceptedPacket == null)
            {
                controller.DataStatusText.text =
                    controller.IsPracticeMode ? "Packet Value: waiting" : "Write Data: waiting";
            }
            else
            {
                controller.DataStatusText.text =
                    controller.IsPracticeMode ? $"Packet Value: {controller.AcceptedPacket.Value}" : $"Write Data: {controller.AcceptedPacket.Value}";
            }
        }

        if (controller.ActionButtonLabel != null)
            controller.ActionButtonLabel.text = controller.IsPracticeAwaitingReset
                ? "Restart"
                : controller.IsAwaitingContinue
                ? controller.ContinueButtonText
                : controller.ExecuteButtonText;

        if (controller.ActionButton != null)
            controller.ActionButton.interactable = controller.IsPhaseActive && !controller.IsTransferRunning;

        RefreshHintBlocks(controller);
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

    static void RefreshHintBlocks(WriteBackController controller)
    {
        var isPracticeMode = controller.IsPracticeMode;
        SetObjectActive(controller.HintPanel.InfoRoot, !isPracticeMode);

        if (controller.PracticeHintButton != null)
            controller.PracticeHintButton.gameObject.SetActive(isPracticeMode);

        SetHintBlockActive(
            controller.PracticeHintText,
            isPracticeMode && !string.IsNullOrWhiteSpace(controller.PracticeHintText != null ? controller.PracticeHintText.text : string.Empty));

        if (isPracticeMode)
        {
            SetHintBlockActive(controller.HintRegDstText, false);
            SetHintBlockActive(controller.HintRegWriteText, false);
            SetHintBlockActive(controller.HintMemToRegText, false);
            return;
        }

        var selectedHint = controller.HintDropdown != null ? controller.HintDropdown.value : 0;
        SetHintBlockActive(controller.HintRegDstText, selectedHint == 1);
        SetHintBlockActive(controller.HintRegWriteText, selectedHint == 2);
        SetHintBlockActive(controller.HintMemToRegText, selectedHint == 3);
    }

    static string BuildRegisterStatusText(string registerId, int currentValue, bool isWaiting, bool isPracticeMode)
    {
        if (string.IsNullOrWhiteSpace(registerId))
            return isPracticeMode ? "Target Register: waiting" : isWaiting ? "Register Target: waiting" : "Register Target: none";

        if (isPracticeMode)
        {
            var registerNumber = RegisterNumberUtility.FormatRegisterNumber(registerId);
            return $"Target Register: {registerNumber}\nCurrent Value: {currentValue}";
        }

        var prefix = isWaiting ? $"Register Target: waiting for {registerId}" : $"Register Target: {registerId}";
        return $"{prefix}\nCurrent Value: {currentValue}";
    }

    static void SetHintBlockActive(TMP_Text textBlock, bool isActive)
    {
        if (textBlock != null)
            textBlock.gameObject.SetActive(isActive);
    }

    static void SetObjectActive(GameObject targetObject, bool isActive)
    {
        if (targetObject != null)
            targetObject.SetActive(isActive);
    }
}

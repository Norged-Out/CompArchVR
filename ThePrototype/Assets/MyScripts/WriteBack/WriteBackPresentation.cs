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

        SetObjectActive(controller.LessonPanelRoot, LessonModePolicy.UsesLessonPanel(controller.CurrentMode));
        SetObjectActive(controller.HintPanelRoot, LessonModePolicy.UsesHintPanel(controller.CurrentMode));

        if (controller.LessonRuntimeText != null)
            controller.LessonRuntimeText.text = controller.IsAssessmentMode
                ? "Assessment Mode\nComplete the Write Back phase using the instruction you decoded earlier."
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
                    controller.IsAssessmentMode);
            }
            else if (controller.AcceptedRegister == null)
            {
                var expectedRegisterId = controller.GetExpectedRegisterIdFromControlState();
                var currentValue = controller.RegisterBank != null ? controller.RegisterBank.GetRegisterValue(expectedRegisterId) : 0;
                controller.RegisterStatusText.text = BuildRegisterStatusText(expectedRegisterId, currentValue, true, controller.IsAssessmentMode);
            }
            else
            {
                controller.RegisterStatusText.text = BuildRegisterStatusText(
                    controller.AcceptedRegister.RegisterId,
                    controller.AcceptedRegister.RegisterValue,
                    false,
                    controller.IsAssessmentMode);
            }
        }

        if (controller.DataStatusText != null)
        {
            if (controller.HasAppliedWriteBack)
            {
                controller.DataStatusText.text =
                    controller.IsAssessmentMode ? $"Packet Value: {controller.LastTransferredValue}" : $"Write Data: {controller.LastTransferredValue}";
            }
            else if (controller.AcceptedPacket == null)
            {
                controller.DataStatusText.text =
                    controller.IsAssessmentMode ? "Packet Value: waiting" : "Write Data: waiting";
            }
            else
            {
                controller.DataStatusText.text =
                    controller.IsAssessmentMode ? $"Packet Value: {controller.AcceptedPacket.Value}" : $"Write Data: {controller.AcceptedPacket.Value}";
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
        InfoCatalog.Load().PopulateDropdown(PhaseInfoTopicGroup.WriteBack, hintDropdown, selectedValue);
    }

    static string BuildLessonRuntimeText(InstructionDefinition instruction)
    {
        var instructionName = instruction != null ? instruction.displayName : "instruction";
        var assembly = instruction != null ? instruction.assemblyInstructionText : "add t2, t0, t1";
        return $"Instruction: {instructionName}\nAssembly: {assembly}";
    }

    static void RefreshHintBlocks(WriteBackController controller)
    {
        var usesAssessmentHints = controller.IsAssessmentMode && LessonModePolicy.UsesHintPanel(controller.CurrentMode);
        SetObjectActive(controller.HintPanel.InfoRoot, !usesAssessmentHints);

        if (controller.PracticeHintButton != null)
            controller.PracticeHintButton.gameObject.SetActive(usesAssessmentHints);

        SetHintBlockActive(
            controller.PracticeHintText,
            usesAssessmentHints && !string.IsNullOrWhiteSpace(controller.PracticeHintText != null ? controller.PracticeHintText.text : string.Empty));

        if (usesAssessmentHints)
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

using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Presentation-only helper for the Program Counter update UI.
/// This keeps authored panel text and visibility logic out of the station
/// controller so the controller can stay focused on phase state.
/// </summary>
public static class PcUpdatePresentation
{
    /// <summary>
    /// Rebuilds the entire PC update UI from the controller's current state.
    /// </summary>
    public static void Refresh(PcUpdateController controller, PcBranchService branchService)
    {
        if (controller == null || branchService == null)
            return;

        var showEndState = controller.IsAwaitingContinue;
        var showBranchSpecificGroups = controller.IsPhaseActive && controller.BranchValue == "1" && !showEndState;

        SetObjectActive(controller.PcUpdateGroupRoot, !showEndState);
        SetObjectActive(controller.SignalsGroupRoot, !showEndState);
        SetObjectActive(controller.ImmediateGroupRoot, showBranchSpecificGroups);
        SetObjectActive(controller.BranchConditionGroupRoot, showBranchSpecificGroups);

        controller.ImmediateScanner?.SetActive(showBranchSpecificGroups);
        controller.ZeroScanner?.SetActive(showBranchSpecificGroups);
        controller.ImmediateScanner?.SetImmediateRequirements(true);
        controller.ZeroScanner?.SetExpectedPacketRole(DataPacketRole.Zero);

        RefreshLessonBlocks(controller, branchService, showEndState);

        if (controller.BranchStatusText != null)
            controller.BranchStatusText.text = $"Branch: {controller.BranchValue}";

        if (controller.JumpStatusText != null)
            controller.JumpStatusText.text = $"Jump: {controller.JumpValue}";

        if (controller.ImmediateStatusText != null)
            controller.ImmediateStatusText.text = BuildImmediateStatusText(controller);

        if (controller.ZeroStatusText != null)
            controller.ZeroStatusText.text = BuildZeroStatusText(controller);

        if (controller.PCSrcStatusText != null)
            controller.PCSrcStatusText.text = BuildPcSrcStatusText(controller, branchService);

        if (controller.ActionButton != null)
            controller.ActionButton.interactable = controller.IsPhaseActive;

        if (controller.ActionButtonLabel != null)
            controller.ActionButtonLabel.text = controller.IsAwaitingContinue
                ? controller.ContinueButtonText
                : controller.ConfirmButtonText;

        RefreshHintBlocks(controller.HintDropdown, controller.HintPcText, controller.HintPcSrcText, controller.HintBranchText, controller.HintJumpText, controller.HintShiftLeftTwoText, controller.HintZeroText);
    }

    /// <summary>
    /// Updates the authored PC update feedback field color and active state.
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
    /// Rebuilds the branch-condition dropdown options in a deterministic order.
    /// </summary>
    public static void PopulateBranchConditionDropdown(TMP_Dropdown branchConditionDropdown)
    {
        if (branchConditionDropdown == null)
            return;

        var selectedValue = Mathf.Clamp(branchConditionDropdown.value, 0, 2);
        branchConditionDropdown.ClearOptions();
        branchConditionDropdown.AddOptions(new List<string>
        {
            "Choose Option",
            "Equal",
            "Not Equal",
        });
        branchConditionDropdown.SetValueWithoutNotify(selectedValue);
    }

    /// <summary>
    /// Rebuilds the PC update hint dropdown options in authored display order.
    /// </summary>
    public static void PopulateHintDropdown(TMP_Dropdown hintDropdown)
    {
        if (hintDropdown == null)
            return;

        var selectedValue = Mathf.Clamp(hintDropdown.value, 0, 6);
        hintDropdown.ClearOptions();
        hintDropdown.AddOptions(new List<string>
        {
            "Choose Option",
            "PC",
            "PCSrc",
            "Branch",
            "Jump",
            "Shift Left 2",
            "Zero",
        });
        hintDropdown.SetValueWithoutNotify(selectedValue);
    }

    /// <summary>
    /// Shows the correct authored lesson blocks for either the active solve
    /// state or the end-of-phase recap state.
    /// </summary>
    static void RefreshLessonBlocks(PcUpdateController controller, PcBranchService branchService, bool showEndState)
    {
        SetTextActive(controller.LessonRuntimeText, !showEndState);
        SetTextActive(controller.LessonBranchText, !showEndState && ShouldShowBranchLesson(controller.CurrentInstruction));
        SetTextActive(controller.LessonShiftText, !showEndState && ShouldShowShiftLesson(controller.CurrentInstruction));
        SetTextActive(controller.LessonResultText, !showEndState && ShouldShowResultLesson(controller.CurrentInstruction));
        SetTextActive(controller.LessonEndText, showEndState);

        if (controller.LessonRuntimeText != null)
            controller.LessonRuntimeText.text = branchService.BuildLessonRuntimeText(controller.CurrentInstruction);

        if (controller.LessonEndText != null)
            controller.LessonEndText.text = branchService.BuildLessonEndText();
    }

    static bool ShouldShowBranchLesson(InstructionDefinition instruction)
    {
        return instruction != null && instruction.UsesBranchDecision();
    }

    static bool ShouldShowShiftLesson(InstructionDefinition instruction)
    {
        return instruction != null && instruction.UsesBranchDecision();
    }

    static bool ShouldShowResultLesson(InstructionDefinition instruction)
    {
        return instruction != null && instruction.UsesBranchDecision();
    }

    static string BuildImmediateStatusText(PcUpdateController controller)
    {
        if (controller.BranchValue != "1")
            return "Waiting";

        if (controller.ImmediateScanner == null || controller.ImmediateScanner.AcceptedPacket == null)
        {
            return controller.ImmediateScanner != null
                ? controller.ImmediateScanner.CurrentIssue switch
                {
                    PcUpdatePacketScanner.PacketIssue.ImmediateNotSignExtended => "Not extended",
                    _ => "Waiting",
                }
                : "Waiting";
        }

        var packet = controller.ImmediateScanner.AcceptedPacket;
        if (!packet.IsSignExtended)
            return "Not extended";

        if (packet != controller.ShiftPreparedImmediatePacket)
            return "Not shifted";

        return "Ready";
    }

    /// <summary>
    /// Returns the concise zero-result status line shown in the interaction
    /// panel.
    /// </summary>
    static string BuildZeroStatusText(PcUpdateController controller)
    {
        if (controller.BranchValue != "1")
            return "Zero: n/a";

        if (controller.ZeroScanner == null || controller.ZeroScanner.AcceptedPacket == null)
            return "Zero: waiting";

        return $"Zero: {controller.ZeroScanner.AcceptedPacket.Value}";
    }

    /// <summary>
    /// Formats the live PCSrc calculation exactly as shown to the learner in
    /// the interaction panel.
    /// </summary>
    static string BuildPcSrcStatusText(PcUpdateController controller, PcBranchService branchService)
    {
        var pcIncrement = controller.GetPcIncrementValue();

        if (controller.CurrentInstruction == null || !controller.CurrentInstruction.UsesBranchDecision())
            return $"PCSrc = 0\nNext PC: PC + {pcIncrement}";

        var zeroValue = controller.ZeroScanner != null && controller.ZeroScanner.AcceptedPacket != null
            ? controller.ZeroScanner.AcceptedPacket.Value
            : 0;

        var evaluation = branchService.Evaluate(
            controller.CurrentInstruction,
            controller.BranchValue,
            pcIncrement,
            zeroValue,
            controller.GetSelectedBranchCondition());

        return $"PCSrc = Branch({controller.BranchValue}) AND ConditionMet({(evaluation.ConditionMet ? 1 : 0)}) = {evaluation.PcSrc}\nNext PC: {evaluation.NextPcText}";
    }

    /// <summary>
    /// Turns hint blocks on one-at-a-time based on the current hint dropdown
    /// selection.
    /// </summary>
    static void RefreshHintBlocks(TMP_Dropdown hintDropdown, TMP_Text pcText, TMP_Text pcSrcText, TMP_Text branchText, TMP_Text jumpText, TMP_Text shiftLeftTwoText, TMP_Text zeroText)
    {
        var selectedHint = hintDropdown != null ? hintDropdown.value : 0;
        SetTextActive(pcText, selectedHint == 1);
        SetTextActive(pcSrcText, selectedHint == 2);
        SetTextActive(branchText, selectedHint == 3);
        SetTextActive(jumpText, selectedHint == 4);
        SetTextActive(shiftLeftTwoText, selectedHint == 5);
        SetTextActive(zeroText, selectedHint == 6);
    }

    /// <summary>
    /// Small null-safe text visibility helper used by the authored panel
    /// blocks.
    /// </summary>
    static void SetTextActive(TMP_Text textBlock, bool isActive)
    {
        if (textBlock != null)
            textBlock.gameObject.SetActive(isActive);
    }

    /// <summary>
    /// Small null-safe object visibility helper used for grouped interaction
    /// blocks.
    /// </summary>
    static void SetObjectActive(GameObject target, bool isActive)
    {
        if (target != null)
            target.SetActive(isActive);
    }
}

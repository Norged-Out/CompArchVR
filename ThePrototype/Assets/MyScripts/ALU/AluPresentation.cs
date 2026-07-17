using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Presentation-only helper for the authored ALU UI and ALU prefab label.
/// </summary>
public static class AluPresentation
{
    /// <summary>
    /// Returns the learner-facing ALU operation label for the current station
    /// state.
    /// </summary>
    public static string GetOperationDisplayName(AluController controller, AluExecutionService executionService)
    {
        if (controller.CurrentAluOpValue == "10" && !controller.HasExplicitFunctSelection)
            return "None";

        return GetOperationDisplayName(executionService.ResolveCurrentOperation(controller));
    }

    /// <summary>
    /// Rebuilds all ALU-facing labels and authored hint visibility.
    /// </summary>
    public static void Refresh(AluController controller, AluExecutionService executionService)
    {
        if (controller == null || executionService == null)
            return;

        if (controller.LessonRuntimeText != null)
        {
            var instructionName = controller.CurrentInstruction != null ? controller.CurrentInstruction.displayName : "instruction";
            var assembly = controller.CurrentInstruction != null ? controller.CurrentInstruction.assemblyInstructionText : "add t2, t0, t1";
            controller.LessonRuntimeText.text = $"Instruction: {instructionName}\nAssembly: {assembly}";
        }

        var operationDisplayName = GetOperationDisplayName(controller, executionService);

        if (controller.OperationLabelText != null)
            controller.OperationLabelText.text = operationDisplayName;

        if (controller.AluOpStatusText != null)
            controller.AluOpStatusText.text = $"ALUOp: {controller.CurrentAluOpValue}";

        if (controller.AluSrcStatusText != null)
            controller.AluSrcStatusText.text = $"ALUSrc: {controller.CurrentAluSrcValue}";

        if (controller.Input1StatusText != null)
            controller.Input1StatusText.text = BuildInputStatusText("Input 1", DataPacketRole.ReadData1, controller.InputA);

        if (controller.Input2StatusText != null)
            controller.Input2StatusText.text = BuildInputStatusText("Input 2", executionService.GetExpectedInput2Role(controller), controller.InputB);

        if (controller.FunctDropdown != null)
        {
            var showFunctDropdown = controller.CurrentAluOpValue == "10";
            controller.FunctDropdown.gameObject.SetActive(showFunctDropdown);
            controller.FunctDropdown.interactable = showFunctDropdown && !controller.HasProducedResult;

            if (showFunctDropdown && controller.HasExplicitFunctSelection)
                SyncDropdownToCurrentOperation(controller.FunctDropdown, controller.SelectedFunctOperation);
        }

        if (controller.ExecuteButtonLabel != null)
            controller.ExecuteButtonLabel.text = controller.HasProducedResult ? controller.ResultReadyButtonText : controller.ExecuteButtonText;

        if (controller.ExecuteButton != null)
            controller.ExecuteButton.interactable = controller.IsPhaseActive && controller.ComputeRoutine == null;

        RefreshHintBlocks(controller.HintDropdown, controller.HintAluOpText, controller.HintAluSrcText, controller.HintAluControlText);
    }

    /// <summary>
    /// Updates the ALU feedback field color and visibility.
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
    /// Rebuilds the ALU hint dropdown in deterministic authored order.
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
            "ALUOp",
            "ALUSrc",
            "ALU Control",
        });
        hintDropdown.SetValueWithoutNotify(selectedValue);
    }

    /// <summary>
    /// Synchronizes the funct dropdown to the current ALU operation by matching
    /// option text instead of assuming a fixed authored index order.
    /// </summary>
    public static void SyncDropdownToCurrentOperation(TMP_Dropdown functDropdown, AluOperation selectedOperation)
    {
        if (functDropdown == null || functDropdown.options == null || functDropdown.options.Count == 0)
            return;

        var targetTexts = BuildOperationDropdownAliases(selectedOperation);
        var targetIndex = 0;

        for (var index = 0; index < functDropdown.options.Count; index++)
        {
            var optionText = functDropdown.options[index].text.Trim();
            if (targetTexts.Contains(optionText))
            {
                targetIndex = index;
                break;
            }
        }

        functDropdown.SetValueWithoutNotify(targetIndex);
        functDropdown.RefreshShownValue();
    }

    /// <summary>
    /// Builds the post-execution feedback shown before the learner advances.
    /// </summary>
    public static string BuildPostExecuteFeedback(InstructionDefinition instruction, int resultValue)
    {
        if (instruction == null)
            return $"ALU result ready: {resultValue}. Click Continue.";

        if (instruction.UsesBranchDecision())
            return $"Zero result ready: {resultValue}. Click Continue to proceed to Program Counter Update.";

        if (instruction.UsesInteractiveMemoryPhase())
            return $"ALU result ready: {resultValue}. Click Continue to proceed to Memory Access.";

        if (instruction.UsesWriteBackPhase())
            return $"ALU result ready: {resultValue}. Memory Access is skipped for this instruction. Click Continue to proceed to Write Back.";

        return $"ALU result ready: {resultValue}. Click Continue to proceed to Program Counter Update.";
    }

    /// <summary>
    /// Returns the learner-facing packet role label used across ALU status
    /// text.
    /// </summary>
    public static string GetRoleDisplayName(DataPacketRole packetRole)
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

    /// <summary>
    /// Returns the short learner-facing name for a resolved ALU operation.
    /// </summary>
    public static string GetOperationDisplayName(AluOperation operation)
    {
        return operation switch
        {
            AluOperation.Subtract => "Sub",
            AluOperation.And => "And",
            AluOperation.Or => "Or",
            AluOperation.SetOnLessThan => "Slt",
            _ => "Add",
        };
    }

    /// <summary>
    /// Builds the learner-facing status line for one ALU input hand.
    /// </summary>
    static string BuildInputStatusText(string inputLabel, DataPacketRole expectedRole, AluInputScanner scanner)
    {
        if (scanner == null)
            return $"{inputLabel}: waiting for {GetRoleDisplayName(expectedRole)}";

        if (scanner.AcceptedPacket == null)
        {
            if (scanner.CurrentIssue == AluInputScanner.PacketIssue.ImmediateNotSignExtended)
                return $"{inputLabel}: Immediate detected (not sign-extended)";

            return $"{inputLabel}: waiting for {GetRoleDisplayName(expectedRole)}";
        }

        var signExtensionSuffix = scanner.AcceptedPacket.PacketRole == DataPacketRole.Immediate
            ? scanner.AcceptedPacket.IsSignExtended ? " (sign-extended)" : " (not sign-extended)"
            : string.Empty;

        return $"{inputLabel}: {scanner.AcceptedValue}{signExtensionSuffix}";
    }

    /// <summary>
    /// Enables only the hint block that matches the current hint dropdown
    /// selection.
    /// </summary>
    static void RefreshHintBlocks(TMP_Dropdown hintDropdown, TMP_Text aluOpText, TMP_Text aluSrcText, TMP_Text aluControlText)
    {
        var selectedHint = hintDropdown != null ? hintDropdown.value : 0;
        SetHintBlockActive(aluOpText, selectedHint == 1);
        SetHintBlockActive(aluSrcText, selectedHint == 2);
        SetHintBlockActive(aluControlText, selectedHint == 3);
    }

    /// <summary>
    /// Toggles an authored hint text block on or off.
    /// </summary>
    static void SetHintBlockActive(TMP_Text textBlock, bool isActive)
    {
        if (textBlock != null)
            textBlock.gameObject.SetActive(isActive);
    }

    static HashSet<string> BuildOperationDropdownAliases(AluOperation operation)
    {
        return operation switch
        {
            AluOperation.Subtract => new HashSet<string>(System.StringComparer.OrdinalIgnoreCase) { "Subtract", "Sub" },
            AluOperation.And => new HashSet<string>(System.StringComparer.OrdinalIgnoreCase) { "And" },
            AluOperation.Or => new HashSet<string>(System.StringComparer.OrdinalIgnoreCase) { "Or" },
            AluOperation.SetOnLessThan => new HashSet<string>(System.StringComparer.OrdinalIgnoreCase) { "Slt", "Set On Less Than" },
            _ => new HashSet<string>(System.StringComparer.OrdinalIgnoreCase) { "Add" },
        };
    }
}

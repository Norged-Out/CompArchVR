using System;

/// <summary>
/// Practice-mode runtime policy for the ALU phase.
/// This keeps hint order, budget consumption, and failure wording out of the
/// station controller so the controller can stay scene-focused.
/// </summary>
public sealed class AluPracticeFlow : PracticePhaseFlowBase
{
    enum HintTarget
    {
        None,
        AluOp,
        AluSrc,
        Input1,
        Input2,
        Funct,
    }

    public bool HandleValidationFailure(string validationMessage, out string feedbackText)
    {
        return ConsumeValidationFailure(validationMessage, out feedbackText);
    }

    public bool HandleScannerFailure(AluInputScanner scanner, DataPacketToken packet, AluInputScanner.PacketIssue issue, out string feedbackText)
    {
        var scannerName = scanner == null ? "alu" : scanner.name;
        var packetKey = packet != null ? packet.GetInstanceID().ToString() : issue.ToString();
        var failureKey = $"{scannerName}:{packetKey}:{issue}";
        var message = issue == AluInputScanner.PacketIssue.ImmediateNotSignExtended
            ? "That immediate is not ready yet."
            : "That packet does not belong on this input.";

        return ConsumeScannerFailure(failureKey, message, out feedbackText, out _);
    }

    public string BuildHint(AluController controller, AluExecutionService executionService)
    {
        if (!TryConsumeHint(out var noHintsFeedback))
            return noHintsFeedback;

        var hintTarget = ResolveHintTarget(controller, executionService);
        var hintMessage = hintTarget switch
        {
            HintTarget.AluOp => "Set ALUOp to the correct operation family first.",
            HintTarget.AluSrc => "Decide whether input 2 should come from a register value or an immediate.",
            HintTarget.Input1 => "Input 1 should hold the first register-read value.",
            HintTarget.Input2 => "Input 2 must match the source selected by ALUSrc.",
            HintTarget.Funct => "Match the ALU control selection to the funct you decoded earlier.",
            _ => "The current ALU setup is ready to validate.",
        };

        return BuildHintText(hintMessage);
    }

    HintTarget ResolveHintTarget(AluController controller, AluExecutionService executionService)
    {
        if (controller == null || executionService == null || controller.CurrentInstruction == null)
            return HintTarget.None;

        if (controller.CurrentAluOpValue != AluExecutionService.GetExpectedAluOpValue(controller.CurrentInstruction))
            return HintTarget.AluOp;

        if (controller.CurrentAluSrcValue != AluExecutionService.GetExpectedAluSrcValue(controller.CurrentInstruction))
            return HintTarget.AluSrc;

        if (controller.InputA == null || controller.InputA.AcceptedPacket == null)
            return HintTarget.Input1;

        var expectedInput2Role = executionService.GetExpectedInput2Role(controller);
        if (controller.InputB == null || controller.InputB.AcceptedPacket == null)
            return HintTarget.Input2;

        if (controller.InputB.AcceptedPacket.PacketRole != expectedInput2Role)
            return HintTarget.Input2;

        if (expectedInput2Role == DataPacketRole.Immediate && !controller.InputB.AcceptedPacket.IsSignExtended)
            return HintTarget.Input2;

        if (controller.CurrentAluOpValue == "10" && !controller.HasExplicitFunctSelection)
            return HintTarget.Funct;

        if (controller.CurrentAluOpValue == "10" &&
            controller.SelectedFunctOperation != AluExecutionService.ResolveExpectedFunctOperation(controller.CurrentInstruction))
        {
            return HintTarget.Funct;
        }

        return HintTarget.None;
    }
}

/// <summary>
/// Practice-mode runtime policy for the Program Counter update phase.
/// It owns attempt tracking and hint sequencing while the controller remains
/// responsible for scene references and lesson progression.
/// </summary>
public sealed class PcUpdatePracticeFlow : PracticePhaseFlowBase
{
    enum HintTarget
    {
        None,
        PcIncrement,
        Branch,
        Jump,
        Immediate,
        Shift,
        Zero,
        Condition,
    }

    public bool HandleValidationFailure(string validationMessage, out string feedbackText)
    {
        return ConsumeValidationFailure(validationMessage, out feedbackText);
    }

    public bool HandleImmediateScannerFailure(DataPacketToken packetToken, PcUpdatePacketScanner.PacketIssue issue, out string feedbackText)
    {
        var packetKey = packetToken != null ? packetToken.GetInstanceID().ToString() : issue.ToString();
        var message = issue == PcUpdatePacketScanner.PacketIssue.ImmediateNotSignExtended
            ? "That branch offset is not sign-extended yet."
            : "That packet cannot be used for the branch-offset input.";

        return ConsumeScannerFailure($"immediate:{packetKey}:{issue}", message, out feedbackText, out _);
    }

    public bool HandleZeroScannerFailure(DataPacketToken packetToken, out string feedbackText)
    {
        var packetKey = packetToken != null ? packetToken.GetInstanceID().ToString() : "zero";
        return ConsumeScannerFailure($"zero:{packetKey}", "That packet cannot be used for the branch-condition input.", out feedbackText, out _);
    }

    public string BuildHint(PcUpdateController controller, PcBranchService branchService)
    {
        if (!TryConsumeHint(out var noHintsFeedback))
            return noHintsFeedback;

        var hintTarget = ResolveHintTarget(controller, branchService);
        var hintMessage = hintTarget switch
        {
            HintTarget.PcIncrement => "The Program Counter must advance by 4 before any branch or jump decision is applied.",
            HintTarget.Branch => "Set Branch only when this instruction depends on a branch decision.",
            HintTarget.Jump => "Set Jump only when this instruction follows a jump path.",
            HintTarget.Immediate => "A branch path needs the sign-extended immediate packet.",
            HintTarget.Shift => "Shift the sign-extended branch offset left by 2 before confirming.",
            HintTarget.Zero => "A branch path also needs the ALU zero-result packet.",
            HintTarget.Condition => "Match the branch-condition dropdown to the comparison this instruction uses.",
            _ => "The current Program Counter update is ready to validate.",
        };

        return BuildHintText(hintMessage);
    }

    HintTarget ResolveHintTarget(PcUpdateController controller, PcBranchService branchService)
    {
        if (controller == null || branchService == null || controller.CurrentInstruction == null)
            return HintTarget.None;

        if (controller.GetPcIncrementValue() != 4)
            return HintTarget.PcIncrement;

        if (controller.BranchValue != controller.CurrentInstruction.GetExpectedBranchControlValue())
            return HintTarget.Branch;

        if (controller.JumpValue != controller.CurrentInstruction.GetExpectedJumpControlValue())
            return HintTarget.Jump;

        if (!controller.CurrentInstruction.UsesBranchDecision())
            return HintTarget.None;

        if (controller.ImmediateScanner == null || controller.ImmediateScanner.AcceptedPacket == null)
            return HintTarget.Immediate;

        if (!controller.ImmediateScanner.AcceptedPacket.IsSignExtended)
            return HintTarget.Immediate;

        if (controller.ImmediateScanner.AcceptedPacket != controller.ShiftPreparedImmediatePacket)
            return HintTarget.Shift;

        if (controller.ZeroScanner == null || controller.ZeroScanner.AcceptedPacket == null)
            return HintTarget.Zero;

        if (controller.GetSelectedBranchCondition() != controller.CurrentInstruction.GetExpectedBranchCondition())
            return HintTarget.Condition;

        return HintTarget.None;
    }
}

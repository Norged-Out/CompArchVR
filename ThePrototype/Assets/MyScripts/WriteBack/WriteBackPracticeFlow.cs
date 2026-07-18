/// <summary>
/// Practice-mode runtime policy for the Write Back phase.
/// This keeps hint order and attempt tracking out of the station controller.
/// </summary>
public sealed class WriteBackPracticeFlow : PracticePhaseFlowBase
{
    enum HintTarget
    {
        None,
        RegWrite,
        RegDst,
        MemToReg,
        Register,
        Data,
    }

    public bool HandleValidationFailure(string validationMessage, out string feedbackText)
    {
        return ConsumeValidationFailure(validationMessage, out feedbackText);
    }

    public bool HandleRegisterScannerFailure(RegisterToken registerToken, out string feedbackText)
    {
        var packetKey = registerToken != null ? registerToken.GetInstanceID().ToString() : "register";
        return ConsumeScannerFailure($"register:{packetKey}", "That register is not the current write-back target.", out feedbackText, out _);
    }

    public bool HandlePacketScannerFailure(DataPacketToken packetToken, out string feedbackText)
    {
        var packetKey = packetToken != null ? packetToken.GetInstanceID().ToString() : "packet";
        return ConsumeScannerFailure($"packet:{packetKey}", "That packet is not the current write-back source.", out feedbackText, out _);
    }

    public string BuildHint(WriteBackController controller)
    {
        if (!TryConsumeHint(out var noHintsFeedback))
            return noHintsFeedback;

        var hintTarget = ResolveHintTarget(controller);
        var hintMessage = hintTarget switch
        {
            HintTarget.RegWrite => "Decide whether this instruction writes back into the register file.",
            HintTarget.RegDst => "Choose which register field should become the write-back target.",
            HintTarget.MemToReg => "Choose whether write-back uses the ALU result or the memory result.",
            HintTarget.Register => "Place the destination register token that matches the selected write-back path.",
            HintTarget.Data => "Place the packet that matches the selected write-back source.",
            _ => "The current write-back setup is ready to validate.",
        };

        return BuildHintText(hintMessage);
    }

    HintTarget ResolveHintTarget(WriteBackController controller)
    {
        if (controller == null || controller.CurrentInstruction == null)
            return HintTarget.None;

        if (controller.RegWriteValue != controller.CurrentInstruction.GetExpectedRegWriteControlValue())
            return HintTarget.RegWrite;

        if (controller.RegDstValue != controller.CurrentInstruction.GetExpectedRegDstControlValue())
            return HintTarget.RegDst;

        if (controller.MemToRegValue != controller.CurrentInstruction.GetExpectedMemToRegControlValue())
            return HintTarget.MemToReg;

        if (controller.AcceptedRegister == null)
            return HintTarget.Register;

        if (controller.AcceptedPacket == null)
            return HintTarget.Data;

        return HintTarget.None;
    }
}

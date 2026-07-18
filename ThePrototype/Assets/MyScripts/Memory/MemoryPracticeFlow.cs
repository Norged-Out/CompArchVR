/// <summary>
/// Practice-mode runtime policy for the Memory phase.
/// It owns hint priority and phase-budget messages while the station
/// controller keeps ownership of scene references and transfer flow.
/// </summary>
public sealed class MemoryPracticeFlow : PracticePhaseFlowBase
{
    enum HintTarget
    {
        None,
        MemRead,
        MemWrite,
        Address,
        Data,
    }

    public bool HandleValidationFailure(string validationMessage, out string feedbackText)
    {
        return ConsumeValidationFailure(validationMessage, out feedbackText);
    }

    public bool HandleScannerFailure(string scannerName, DataPacketToken packet, out string feedbackText)
    {
        var packetKey = packet != null ? packet.GetInstanceID().ToString() : "packet";
        var failureKey = $"{scannerName}:{packetKey}";
        var message = scannerName == "Address"
            ? "That packet cannot be used as the memory address."
            : "That packet cannot be used as memory write data.";

        return ConsumeScannerFailure(failureKey, message, out feedbackText, out _);
    }

    public string BuildHint(MemoryController controller, MemoryTransferService transferService)
    {
        if (!TryConsumeHint(out var noHintsFeedback))
            return noHintsFeedback;

        var hintTarget = ResolveHintTarget(controller, transferService);
        var hintMessage = hintTarget switch
        {
            HintTarget.MemRead => "Choose whether this instruction needs MemRead.",
            HintTarget.MemWrite => "Choose whether this instruction needs MemWrite.",
            HintTarget.Address => "The address input should receive the ALU result packet.",
            HintTarget.Data => "A store needs a register-data packet on the data input.",
            _ => "The current memory setup is ready to validate.",
        };

        return BuildHintText(hintMessage);
    }

    HintTarget ResolveHintTarget(MemoryController controller, MemoryTransferService transferService)
    {
        if (controller == null || transferService == null || controller.CurrentInstruction == null)
            return HintTarget.None;

        var expectsLoad = transferService.IsLoadInstruction(controller.CurrentInstruction);
        var expectsStore = transferService.IsStoreInstruction(controller.CurrentInstruction);

        if (controller.MemReadValue != (expectsLoad ? "1" : "0"))
            return HintTarget.MemRead;

        if (controller.MemWriteValue != (expectsStore ? "1" : "0"))
            return HintTarget.MemWrite;

        if (controller.AddressScanner == null || controller.AddressScanner.AcceptedPacket == null)
            return HintTarget.Address;

        if (expectsStore && (controller.DataScanner == null || controller.DataScanner.AcceptedPacket == null))
            return HintTarget.Data;

        return HintTarget.None;
    }
}

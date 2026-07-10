using System.Collections;
using UnityEngine;

/// <summary>
/// Pure memory-phase logic shared by the Memory controller and presentation.
/// It owns validation, packet spawning, and small datapath-policy decisions.
/// </summary>
public sealed class MemoryTransferService
{
    /// <summary>
    /// Restores the memory phase to its authored baseline for the active
    /// instruction.
    /// </summary>
    public void PrepareForMemoryStep(MemoryController controller)
    {
        if (controller.ExecutionRoutine != null)
        {
            controller.StopCoroutine(controller.ExecutionRoutine);
            controller.SetExecutionRoutine(null);
        }

        controller.SetAwaitingContinueState(false, false);
        controller.SetLastTransferState(0, 0);
        controller.SetMemReadValue("0");
        controller.SetMemWriteValue("0");

        controller.AddressScanner?.ResetScanner();
        controller.DataScanner?.ResetScanner();
        ClearSpawnedMemoryPacket(controller);
        RefreshExpectedTargets(controller);
        controller.SetFeedback(string.Empty, false);
    }

    /// <summary>
    /// Toggles MemRead and clears stale scanner state when the chosen path no
    /// longer matches the authored instruction.
    /// </summary>
    public void ToggleMemRead(MemoryController controller)
    {
        controller.SetMemReadValue(controller.MemReadValue == "1" ? "0" : "1");
        ResetScannersIfSignalStateIsInvalid(controller);
        controller.SetFeedback(string.Empty, false);
    }

    /// <summary>
    /// Toggles MemWrite and clears stale scanner state when the chosen path no
    /// longer matches the authored instruction.
    /// </summary>
    public void ToggleMemWrite(MemoryController controller)
    {
        controller.SetMemWriteValue(controller.MemWriteValue == "1" ? "0" : "1");
        ResetScannersIfSignalStateIsInvalid(controller);
        controller.SetFeedback(string.Empty, false);
    }

    /// <summary>
    /// Validates the physical memory setup against the active instruction.
    /// </summary>
    public bool TryValidateSetup(MemoryController controller, out string validationMessage)
    {
        validationMessage = string.Empty;

        if (!UsesInteractiveMemory(controller.CurrentInstruction))
            return true;

        var expectedMemRead = IsLoadInstruction(controller.CurrentInstruction) ? "1" : "0";
        if (controller.MemReadValue != expectedMemRead)
        {
            validationMessage = "MemRead is not set for the required memory behavior.";
            return false;
        }

        var expectedMemWrite = IsStoreInstruction(controller.CurrentInstruction) ? "1" : "0";
        if (controller.MemWriteValue != expectedMemWrite)
        {
            validationMessage = "MemWrite is not set for the required memory behavior.";
            return false;
        }

        if (controller.AddressScanner == null || controller.AddressScanner.AcceptedPacket == null)
        {
            validationMessage = "The address input is still missing its packet.";
            return false;
        }

        var addressValue = controller.AddressScanner.AcceptedPacket.Value;
        if (controller.MemoryBank == null || !controller.MemoryBank.TryReadWord(addressValue, out _, out _))
        {
            validationMessage = "That address does not map to a valid memory word in this lesson.";
            return false;
        }

        if (IsStoreInstruction(controller.CurrentInstruction))
        {
            if (controller.DataScanner == null || controller.DataScanner.AcceptedPacket == null)
            {
                validationMessage = "The data input is still missing its packet.";
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Reconfigures the memory station's expected packet roles.
    /// </summary>
    public void RefreshExpectedTargets(MemoryController controller)
    {
        controller.AddressScanner?.SetExpectedPacketRole(DataPacketRole.AluResult);
        controller.DataScanner?.SetExpectedPacketRole(DataPacketRole.ReadData2);
    }

    /// <summary>
    /// Drops scanned packets whenever MemRead / MemWrite no longer match the
    /// currently active instruction's expected path.
    /// </summary>
    public void ResetScannersIfSignalStateIsInvalid(MemoryController controller)
    {
        if (controller.CurrentInstruction == null || !UsesInteractiveMemory(controller.CurrentInstruction))
            return;

        var expectedMemRead = IsLoadInstruction(controller.CurrentInstruction) ? "1" : "0";
        var expectedMemWrite = IsStoreInstruction(controller.CurrentInstruction) ? "1" : "0";

        if (controller.MemReadValue == expectedMemRead && controller.MemWriteValue == expectedMemWrite)
            return;

        controller.AddressScanner?.ResetScanner();
        controller.DataScanner?.ResetScanner();
        controller.MemoryBank?.ClearPreview();
    }

    /// <summary>
    /// Spawns the Memory Data packet used by the write-back phase after a load.
    /// </summary>
    public void SpawnMemoryDataPacket(MemoryController controller, int addressValue, int loadedValue)
    {
        ClearSpawnedMemoryPacket(controller);

        if (controller.MemoryDataPacketPrefab == null || controller.MemoryDataSpawnTransform == null)
            return;

        var spawnedPacket = Object.Instantiate(
            controller.MemoryDataPacketPrefab,
            controller.MemoryDataSpawnTransform.position,
            controller.MemoryDataSpawnTransform.rotation);

        spawnedPacket.Configure(
            DataPacketRole.MemoryData,
            $"mem_{addressValue}",
            "Memory Data",
            loadedValue);

        controller.SetSpawnedMemoryPacket(spawnedPacket);
    }

    /// <summary>
    /// Executes the active memory transfer and updates the controller's
    /// persisted runtime state once the transfer finishes.
    /// </summary>
    public IEnumerator RunTransferRoutine(MemoryController controller)
    {
        var addressPacket = controller.AddressScanner != null ? controller.AddressScanner.AcceptedPacket : null;
        var addressValue = addressPacket != null ? addressPacket.Value : 0;

        if (IsLoadInstruction(controller.CurrentInstruction))
        {
            var transferFinished = controller.MemoryBank == null;
            controller.MemoryBank?.PlayTransferSequence(true, () => transferFinished = true);
            while (!transferFinished)
                yield return null;

            yield return new WaitForSeconds(controller.DataSpawnDelaySeconds);
            if (controller.MemoryBank != null && controller.MemoryBank.TryReadWord(addressValue, out var loadedValue, out _))
            {
                SpawnMemoryDataPacket(controller, addressValue, loadedValue);
                controller.SetLastTransferState(addressValue, loadedValue);
                controller.SetFeedback(
                    $"Memory data ready: loaded {loadedValue} from {FormatAddress(addressValue)}. Click Continue to proceed to Write Back.",
                    false);
            }
        }
        else if (IsStoreInstruction(controller.CurrentInstruction))
        {
            var sourcePacket = controller.DataScanner != null ? controller.DataScanner.AcceptedPacket : null;
            var transferFinished = controller.MemoryBank == null;
            controller.MemoryBank?.PlayTransferSequence(false, () => transferFinished = true);
            while (!transferFinished)
                yield return null;

            if (sourcePacket != null && controller.MemoryBank != null && controller.MemoryBank.TryWriteWord(addressValue, sourcePacket.Value, out _))
            {
                controller.SetLastTransferState(addressValue, sourcePacket.Value);
                controller.SetFeedback(
                    $"Stored {sourcePacket.Value} into {FormatAddress(addressValue)}. Click Continue to proceed to Program Counter Update.",
                    false);
            }
        }

        controller.SetAwaitingContinueState(true, true);
        controller.SetExecutionRoutine(null);
        controller.AddressScanner?.ConsumeAcceptedPacket();

        if (controller.DataScanner != null && IsStoreInstruction(controller.CurrentInstruction))
            controller.DataScanner.ConsumeAcceptedPacket();

        controller.RefreshPresentation();
    }

    /// <summary>
    /// Removes any previously spawned Memory Data packet.
    /// </summary>
    public void ClearSpawnedMemoryPacket(MemoryController controller)
    {
        if (controller.SpawnedMemoryPacket == null)
            return;

        if (Application.isPlaying)
            Object.Destroy(controller.SpawnedMemoryPacket.gameObject);
        else
            Object.DestroyImmediate(controller.SpawnedMemoryPacket.gameObject);

        controller.SetSpawnedMemoryPacket(null);
    }

    public bool UsesInteractiveMemory(InstructionDefinition instruction)
    {
        return instruction != null && instruction.UsesInteractiveMemoryPhase();
    }

    public bool IsLoadInstruction(InstructionDefinition instruction)
    {
        return instruction != null && instruction.mnemonic == InstructionMnemonic.Lw;
    }

    public bool IsStoreInstruction(InstructionDefinition instruction)
    {
        return instruction != null && instruction.mnemonic == InstructionMnemonic.Sw;
    }

    public bool RequiresDataInput(InstructionDefinition instruction)
    {
        return IsStoreInstruction(instruction);
    }

    public static string GetPacketRoleLabel(DataPacketRole packetRole)
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

    public static string FormatAddress(int value)
    {
        return $"0x{value:X8}";
    }
}

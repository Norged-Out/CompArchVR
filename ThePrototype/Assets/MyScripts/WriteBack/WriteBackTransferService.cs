using System.Collections;
using UnityEngine;

/// <summary>
/// Pure write-back behavior helper.
/// It validates signal choices, resolves expected targets, and performs the
/// actual value transfer sequence once the station is configured correctly.
/// </summary>
public sealed class WriteBackTransferService
{
    /// <summary>
    /// Checks whether the current WB signal state and latched objects match the
    /// active instruction's authored expectations.
    /// </summary>
    public bool TryValidate(
        InstructionDefinition instruction,
        string regDstValue,
        string regWriteValue,
        string memToRegValue,
        WriteBackRegisterScanner registerScanner,
        WriteBackPacketScanner packetScanner,
        out string validationMessage)
    {
        validationMessage = string.Empty;

        if (instruction == null)
        {
            validationMessage = "No instruction is loaded for write-back.";
            return false;
        }

        if (regWriteValue != instruction.GetExpectedRegWriteControlValue())
        {
            validationMessage = "RegWrite does not match the behavior needed by this instruction.";
            return false;
        }

        if (regDstValue != instruction.GetExpectedRegDstControlValue())
        {
            validationMessage = "RegDst is selecting the wrong destination path.";
            return false;
        }

        if (memToRegValue != instruction.GetExpectedMemToRegControlValue())
        {
            validationMessage = "MemToReg is selecting the wrong write-back source.";
            return false;
        }

        if (registerScanner == null || registerScanner.AcceptedRegister == null)
        {
            validationMessage = "The destination register has not been placed yet.";
            return false;
        }

        if (packetScanner == null || packetScanner.AcceptedPacket == null)
        {
            validationMessage = "The write-back data packet has not been placed yet.";
            return false;
        }

        return true;
    }

    /// <summary>
    /// Resolves which architectural register should be accepted under RegDst.
    /// </summary>
    public string GetExpectedRegisterId(InstructionDefinition instruction, string regDstValue)
    {
        if (instruction == null)
            return string.Empty;

        return regDstValue == "1" ? instruction.expectedRd : instruction.expectedRt;
    }

    /// <summary>
    /// Resolves which packet role should be accepted under MemToReg.
    /// </summary>
    public DataPacketRole GetExpectedPacketRole(InstructionDefinition instruction, string memToRegValue)
    {
        if (instruction == null)
            return DataPacketRole.None;

        return memToRegValue == "1" ? DataPacketRole.MemoryData : DataPacketRole.AluResult;
    }

    /// <summary>
    /// Runs the authored WB transfer sequence, updates the register bank, and
    /// consumes the packet once the animation is finished.
    /// </summary>
    public IEnumerator RunTransferRoutine(
        RegisterBank registerBank,
        PipeSequencePlayer pipeSequencePlayer,
        ParticleSystem transferParticles,
        float particleLeadTimeSeconds,
        string destinationRegister,
        DataPacketToken packet,
        System.Action<string, int, DataPacketRole> onTransferApplied)
    {
        var packetValue = packet != null ? packet.Value : 0;
        var packetRole = packet != null ? packet.PacketRole : DataPacketRole.None;

        // The pipe sweep intentionally happens before the particle cue so the
        // learner can visually follow the transfer path toward the register.
        pipeSequencePlayer?.PlaySuccessSweep();

        if (pipeSequencePlayer != null)
            yield return new WaitForSeconds(pipeSequencePlayer.DefaultStepDelaySeconds * 5f);

        if (transferParticles != null)
            transferParticles.Play();

        yield return new WaitForSeconds(particleLeadTimeSeconds);

        if (registerBank != null && !string.IsNullOrWhiteSpace(destinationRegister))
            registerBank.SetRegisterValue(destinationRegister, packetValue);

        if (packet != null)
        {
            // A consumed WB packet should leave the scene entirely rather than
            // remain as an interactable duplicate of already-applied data.
            if (Application.isPlaying)
                Object.Destroy(packet.gameObject);
            else
                Object.DestroyImmediate(packet.gameObject);
        }

        onTransferApplied?.Invoke(destinationRegister, packetValue, packetRole);
    }
}

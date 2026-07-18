using UnityEngine;

/// <summary>
/// Small deterministic helpers used by dev-mode phase skipping.
/// The goal is to derive the authored "expected" lesson outputs without
/// depending on whatever the learner currently has or has not placed.
/// </summary>
public static class LessonDevMath
{
    public static int ComputeExpectedAluResult(InstructionDefinition instruction, RegisterBank registerBank)
    {
        if (instruction == null || registerBank == null)
            return 0;

        var rsValue = registerBank.GetRegisterValue(instruction.expectedRs);
        var rtValue = registerBank.GetRegisterValue(instruction.expectedRt);
        var immediateValue = instruction.expectedImmediateValue;

        return instruction.mnemonic switch
        {
            InstructionMnemonic.Sub => rsValue - rtValue,
            InstructionMnemonic.And => rsValue & rtValue,
            InstructionMnemonic.Andi => rsValue & immediateValue,
            InstructionMnemonic.Or => rsValue | rtValue,
            InstructionMnemonic.Ori => rsValue | immediateValue,
            InstructionMnemonic.Slt => rsValue < rtValue ? 1 : 0,
            InstructionMnemonic.Addi => rsValue + immediateValue,
            InstructionMnemonic.Lw => rsValue + immediateValue,
            InstructionMnemonic.Sw => rsValue + immediateValue,
            InstructionMnemonic.Beq => rsValue - rtValue,
            InstructionMnemonic.Bne => rsValue - rtValue,
            _ => rsValue + rtValue,
        };
    }

    public static bool TryResolveExpectedLoad(DataMemoryBank memoryBank, InstructionDefinition instruction, RegisterBank registerBank, out int addressValue, out int loadedValue)
    {
        addressValue = 0;
        loadedValue = 0;

        if (memoryBank == null || instruction == null || registerBank == null)
            return false;

        addressValue = ComputeExpectedAluResult(instruction, registerBank);
        return memoryBank.TryReadWord(addressValue, out loadedValue, out _);
    }

    public static bool TryResolveExpectedStore(DataMemoryBank memoryBank, InstructionDefinition instruction, RegisterBank registerBank, out int addressValue, out int storedValue)
    {
        addressValue = 0;
        storedValue = 0;

        if (memoryBank == null || instruction == null || registerBank == null)
            return false;

        addressValue = ComputeExpectedAluResult(instruction, registerBank);
        storedValue = registerBank.GetRegisterValue(instruction.expectedRt);
        return memoryBank.TryReadWord(addressValue, out _, out _);
    }

    public static int ResolveExpectedWriteBackValue(
        InstructionDefinition instruction,
        RegisterBank registerBank,
        DataMemoryBank memoryBank,
        int fallbackAluResult)
    {
        if (instruction == null)
            return fallbackAluResult;

        if (instruction.GetWriteBackPacketRole() != DataPacketRole.MemoryData)
            return fallbackAluResult;

        if (TryResolveExpectedLoad(memoryBank, instruction, registerBank, out _, out var loadedValue))
            return loadedValue;

        return fallbackAluResult;
    }
}

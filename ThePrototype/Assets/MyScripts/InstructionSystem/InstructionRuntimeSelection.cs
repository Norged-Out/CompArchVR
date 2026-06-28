using System;

/// <summary>
/// Stores the player's current operand choices for the selected instruction.
/// This is runtime state, not asset data.
/// 
/// For the first pass, registers are stored as strings because you mentioned
/// future dropdowns. Once the UI exists, those dropdowns can simply write into
/// these fields.
/// </summary>
[Serializable]
public class InstructionRuntimeSelection
{
    // The selected instruction asset this runtime state belongs to.
    public InstructionDefinition definition;

    // Placeholder register storage.
    // Later these could become enums or custom register references.
    public string selectedRs = string.Empty;
    public string selectedRt = string.Empty;
    public string selectedRd = string.Empty;

    // Immediate values will matter for addi, lw, sw, and branch offsets.
    public int immediateValue;

    // Simple switch in case you later want to support decimal/hex input modes.
    public bool immediateIsHex;

    // First-pass write-back confirmation used by the final add lesson stage.
    public string confirmedWriteBackRegister = string.Empty;

    // The ALU result produced during the execution phase.
    public int aluResultValue;
    public bool hasAluResult;

    /// <summary>
    /// Clears player-entered operand values while keeping the selected instruction.
    /// Useful when the same instruction should be replayed from a clean state.
    /// </summary>
    public void ResetOperands()
    {
        selectedRs = string.Empty;
        selectedRt = string.Empty;
        selectedRd = string.Empty;
        immediateValue = 0;
        immediateIsHex = false;
        confirmedWriteBackRegister = string.Empty;
        aluResultValue = 0;
        hasAluResult = false;
    }

    /// <summary>
    /// Stores a register name under a logical operand role.
    /// </summary>
    public void SetSelectedRegister(InstructionRegisterRole role, string registerName)
    {
        switch (role)
        {
            case InstructionRegisterRole.Rs:
                selectedRs = registerName;
                break;
            case InstructionRegisterRole.Rt:
                selectedRt = registerName;
                break;
            case InstructionRegisterRole.Rd:
                selectedRd = registerName;
                break;
        }
    }

    /// <summary>
    /// Reads the currently stored register for a logical role.
    /// </summary>
    public string GetSelectedRegister(InstructionRegisterRole role)
    {
        return role switch
        {
            InstructionRegisterRole.Rs => selectedRs,
            InstructionRegisterRole.Rt => selectedRt,
            InstructionRegisterRole.Rd => selectedRd,
            _ => string.Empty,
        };
    }
}

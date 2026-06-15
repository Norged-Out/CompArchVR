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
    public string selectedRs = "t0";
    public string selectedRt = "t1";
    public string selectedRd = "t2";

    // Immediate values will matter for addi, lw, sw, and branch offsets.
    public int immediateValue;

    // Simple switch in case you later want to support decimal/hex input modes.
    public bool immediateIsHex;

    /// <summary>
    /// Clears player-entered operand values while keeping the selected instruction.
    /// Useful when the same instruction should be replayed from a clean state.
    /// </summary>
    public void ResetOperands()
    {
        selectedRs = "t0";
        selectedRt = "t1";
        selectedRd = "t2";
        immediateValue = 0;
        immediateIsHex = false;
    }
}

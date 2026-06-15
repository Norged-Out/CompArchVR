using System;
using UnityEngine;

/// <summary>
/// Describes which operand fields should be shown for a selected instruction.
/// This is data, not behavior: the UI controller reads these flags and decides
/// which panels, inputs, or dropdowns should appear.
/// </summary>
[Serializable]
public class InstructionUiLayout
{
    // Usually always visible, but kept configurable in case we later want a
    // friendlier "preset-only" mode that hides raw opcode details.
    public bool showOpcode = true;

    // rs is commonly the first source register.
    public bool showRs = true;

    // rt is often the second source register, but for some I-type instructions
    // it can also behave more like a destination field from the user's perspective.
    public bool showRt = true;

    // rd is mainly used by R-type instructions.
    public bool showRd = false;

    // shift amount is not needed immediately, but this avoids redesign later
    // if we decide to support instructions like sll.
    public bool showShamt = false;

    // The immediate field matters for addi, lw, sw, and branches.
    public bool showImmediate = false;

    // funct is often hidden from users in a teaching UI, but it may still be
    // useful if you want a more low-level "show me the encoding" mode later.
    public bool showFunct = false;

    // Jump targets can use a dedicated address field rather than an immediate.
    public bool showAddress = false;
}

using UnityEngine;

/// <summary>
/// Shared enums for the first-pass instruction system draft.
/// These are intentionally broad so we can grow the prototype without rewriting
/// the basic vocabulary every time a new instruction is introduced.
/// </summary>

/// <summary>
/// The high-level instruction encoding family.
/// For now we only truly need R-type and I-type, but J-type is included so the
/// data model does not have to change later when jumps are added.
/// </summary>
public enum InstructionFormat
{
    RType,
    IType,
    JType,
}

/// <summary>
/// A gameplay-friendly grouping for instructions.
/// This is separate from format because many instructions share a format but
/// behave very differently in the datapath.
/// </summary>
public enum InstructionCategory
{
    Arithmetic,
    Logic,
    Memory,
    Branch,
    Jump,
}

/// <summary>
/// Early list of likely MIPS instructions for the first version of the project.
/// This does not need to be complete yet; it only gives the system a stable
/// set of names to reference in assets and UI code.
/// </summary>
public enum InstructionMnemonic
{
    Add,
    Sub,
    And,
    Or,
    Slt,
    Addi,
    Andi,
    Ori,
    Lw,
    Sw,
    Beq,
    Bne,
    J,
    Jal,
}

/// <summary>
/// Logical datapath locations the user can be guided through.
/// This enum is intentionally based on the mental model of the lesson, not on
/// exact scene object names, so later scenes can remap the visuals if needed.
/// </summary>
public enum DatapathNodeId
{
    None,
    ProgramCounter,
    InstructionMemory,
    Registers,
    ALU,
    DataMemory,
    WriteBack,
}

/// <summary>
/// The kind of learner action a lesson step expects before it can finish.
/// This keeps the lesson data focused on pedagogy rather than scene-object details.
/// </summary>
public enum InstructionStepInteractionType
{
    None,
    ContinueButton,
    RegisterSelection,
    Completion,
    AluExecution,
    WriteBackExecution,
}

/// <summary>
/// Logical operand roles used by the lesson system.
/// These let the controller ask for "the rs register" or "the rd register"
/// without hardcoding the actual register name into scene logic.
/// </summary>
public enum InstructionRegisterRole
{
    None,
    Rs,
    Rt,
    Rd,
}

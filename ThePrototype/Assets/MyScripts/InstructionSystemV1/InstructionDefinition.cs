using System;
using UnityEngine;

/// <summary>
/// One guided step inside an instruction walkthrough.
/// Example:
/// "Fetch instruction from instruction memory"
/// "Read source operands from the register file"
/// "Execute ALU operation"
/// </summary>
[Serializable]
public class InstructionFlowStep
{
    // Short label for the step, useful for a small header in the UI.
    public string stepName = "Fetch";

    // Which datapath node should be highlighted for this step.
    public DatapathNodeId highlightedNode = DatapathNodeId.ProgramCounter;

    // What should be explained to the player at this step.
    [TextArea(2, 5)]
    public string explanation =
        "Describe what this datapath stage is doing and why the instruction needs it.";

    // Lets us later insert pure explanation steps without necessarily moving
    // the highlight again.
    public bool advanceVisualHighlight = true;
}

/// <summary>
/// ScriptableObject draft for one instruction definition.
/// Think of this as "stored lesson data" rather than scene logic:
/// - what instruction it is
/// - what operands it needs
/// - what datapath path it should follow
/// - what text should be shown to the user
/// </summary>
[CreateAssetMenu(
    fileName = "InstructionDefinition",
    menuName = "CompArch VR/Instruction System/Instruction Definition")]
public class InstructionDefinition : ScriptableObject
{
    [Header("Identity")]

    // Friendly name to show in a dropdown or panel.
    public string displayName = "add";

    // Stable enum reference so code can branch on the instruction if needed.
    public InstructionMnemonic mnemonic = InstructionMnemonic.Add;

    public InstructionFormat format = InstructionFormat.RType;
    public InstructionCategory category = InstructionCategory.Arithmetic;

    [TextArea(2, 4)]
    public string summary =
        "Add two registers and write the result into a destination register.";

    [Header("Encoding")]

    // Kept as strings for now because the teaching UI may want to display
    // literal bit patterns exactly as entered.
    public string opcodeBits = "000000";
    public string functBits = "100000";

    [Header("Behavior Flags")]

    // High-level restrictions that can drive both explanation and validation.
    public bool touchesDataMemory = false;
    public bool writesRegisterFile = true;
    public bool usesImmediate = false;
    public bool usesDestinationRegister = true;

    [Header("UI Layout")]

    // Controls which operand fields should be visible when this instruction is selected.
    public InstructionUiLayout uiLayout = new();

    [Header("Guided Flow")]

    // Ordered walkthrough for the instruction.
    public InstructionFlowStep[] flowSteps = Array.Empty<InstructionFlowStep>();
}

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

    // Describes how the player is expected to progress past this step.
    public InstructionStepInteractionType requiredInteraction = InstructionStepInteractionType.ContinueButton;

    // When true, the controller should block progression until the required
    // interaction has been completed correctly.
    public bool blockProgressUntilValidated = true;

    // Ordered register choices needed by this step.
    // For the first pass, this is mainly used by the "Registers" stage of add.
    public InstructionRegisterRole[] requiredRegisterSelections = Array.Empty<InstructionRegisterRole>();

    // Optional single register confirmation used by steps such as write-back.
    public InstructionRegisterRole confirmationRegisterRole = InstructionRegisterRole.None;

    // Future-facing placeholder for scene object validation.
    // This is kept as a simple string for now so steps can name a required
    // object without forcing the asset to reference a specific scene instance.
    public string expectedTargetObjectName = string.Empty;
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

    [TextArea(2, 3)]
    public string assemblyInstructionText = "add t2, t0, t1";

    [TextArea(3, 8)]
    public string fieldBreakdownText =
        "opcode: 000000\nrs: t0\nrt: t1\nrd: t2\nfunct: 100000";

    [Header("Encoding")]

    // Kept as strings for now because the teaching UI may want to display
    // literal bit patterns exactly as entered.
    public string opcodeBits = "000000";
    public string functBits = "100000";

    [Header("Expected Operands")]

    // The lesson controller compares player selections against these values.
    public string expectedRs = "t0";
    public string expectedRt = "t1";
    public string expectedRd = "t2";

    // Small curated set of choices to build a first-pass physical register bank.
    public string[] registerBankChoices = { "t0", "t1", "t2", "s0", "s1", "zero" };

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

    /// <summary>
    /// Returns the expected register name for a logical operand role.
    /// </summary>
    public string GetExpectedRegisterName(InstructionRegisterRole role)
    {
        return role switch
        {
            InstructionRegisterRole.Rs => expectedRs,
            InstructionRegisterRole.Rt => expectedRt,
            InstructionRegisterRole.Rd => expectedRd,
            _ => string.Empty,
        };
    }
}

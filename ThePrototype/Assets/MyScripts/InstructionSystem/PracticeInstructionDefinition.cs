using UnityEngine;

/// <summary>
/// Practice-mode instruction data stays separate from guided lesson definitions.
/// These assets describe the encoded source the learner must decode first, then
/// optionally point back to a Learning instruction for the downstream phase flow.
/// </summary>
[CreateAssetMenu(
    fileName = "PracticeInstructionDefinition",
    menuName = "CompArch VR/Instruction System/Practice Instruction Definition")]
public sealed class PracticeInstructionDefinition : ScriptableObject
{
    [Header("Identity")]
    public string displayName = "Practice Add";

    public InstructionMnemonic expectedMnemonic = InstructionMnemonic.Add;
    public InstructionFormat expectedFormat = InstructionFormat.RType;
    public InstructionCategory expectedCategory = InstructionCategory.Arithmetic;

    [Header("Encoded Source")]
    [TextArea(2, 4)]
    public string rawInstructionBits = "00000001000010010101000000100000";

    [Header("Expected Decode")]
    public string expectedOpcodeBits = "000000";
    public string expectedFunctBits = "100000";
    public string expectedRsBits = "01000";
    public string expectedRtBits = "01001";
    public string expectedRdBits = "01010";
    public string expectedImmediateBits = string.Empty;

    [Header("Bridge To Shared Runtime")]
    [Tooltip("Optional Learning instruction used to drive the shared EX/MEM/WB/PC flow after Practice decode succeeds.")]
    public InstructionDefinition learningModeInstruction;
}

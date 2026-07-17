using System;
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

    /// <summary>
    /// Returns the encoded instruction as a whitespace-free 32-bit binary string.
    /// Invalid authoring is tolerated by falling back to the raw trimmed text.
    /// </summary>
    public string GetNormalizedBinaryInstruction()
    {
        var trimmedBits = NormalizeBits(rawInstructionBits);
        return trimmedBits.Length == 32 ? trimmedBits : trimmedBits;
    }

    /// <summary>
    /// Returns the encoded instruction as an 8-digit hexadecimal string for
    /// fetch/module display in Practice mode.
    /// </summary>
    public string GetHexInstructionText()
    {
        var normalizedBits = GetNormalizedBinaryInstruction();
        if (normalizedBits.Length != 32)
            return displayName;

        try
        {
            var encodedValue = Convert.ToUInt32(normalizedBits, 2);
            return $"0x{encodedValue:X8}";
        }
        catch
        {
            return displayName;
        }
    }

    /// <summary>
    /// Returns whether the learner should treat this encoding as one that uses
    /// a funct field during decode.
    /// </summary>
    public bool UsesFunctField()
    {
        return !string.IsNullOrWhiteSpace(expectedFunctBits);
    }

    /// <summary>
    /// Returns whether the learner should treat this encoding as one that uses
    /// an immediate field during decode.
    /// </summary>
    public bool UsesImmediateField()
    {
        return !string.IsNullOrWhiteSpace(expectedImmediateBits);
    }

    /// <summary>
    /// Counts how many source-register fields the learner must identify during
    /// practice decode before moving on to the scanner portion.
    /// </summary>
    public int GetRequiredSourceRegisterCount()
    {
        var requiredCount = 0;

        if (!string.IsNullOrWhiteSpace(expectedRsBits))
            requiredCount++;

        if (!string.IsNullOrWhiteSpace(expectedRtBits))
            requiredCount++;

        return requiredCount;
    }

    /// <summary>
    /// Returns a short learner-facing type label after opcode confirmation.
    /// </summary>
    public string GetInstructionTypeLabel()
    {
        return expectedFormat switch
        {
            InstructionFormat.RType => "R-Type",
            InstructionFormat.IType => "I-Type",
            InstructionFormat.JType => "J-Type",
            _ => "Instruction",
        };
    }

    static string NormalizeBits(string rawBits)
    {
        return string.IsNullOrWhiteSpace(rawBits)
            ? string.Empty
            : rawBits.Replace(" ", string.Empty).Replace("\n", string.Empty).Replace("\r", string.Empty).Trim();
    }
}

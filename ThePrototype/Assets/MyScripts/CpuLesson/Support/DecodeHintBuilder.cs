using System.Collections.Generic;

/// <summary>
/// Builds authored hint-panel reference text for the decode phase.
/// </summary>
public sealed class DecodeHintBuilder
{
    /// <summary>
    /// Creates the opcode lookup text shown when the learner asks for opcode help.
    /// </summary>
    public string BuildOpcodeHintText(IReadOnlyList<InstructionDefinition> availableInstructions)
    {
        return BuildBitReferenceText(
            availableInstructions,
            "Opcode reference",
            instruction => instruction != null ? instruction.opcodeBits : null);
    }

    /// <summary>
    /// Creates the funct lookup text shown when the learner asks for funct help.
    /// </summary>
    public string BuildFunctHintText(IReadOnlyList<InstructionDefinition> availableInstructions)
    {
        return BuildBitReferenceText(
            availableInstructions,
            "Funct reference",
            instruction => instruction != null ? instruction.functBits : null);
    }

    /// <summary>
    /// Generates a unique bit-pattern reference list for a chosen instruction field.
    /// </summary>
    static string BuildBitReferenceText(
        IReadOnlyList<InstructionDefinition> availableInstructions,
        string title,
        System.Func<InstructionDefinition, string> selector)
    {
        var lines = new List<string>();
        foreach (var instruction in availableInstructions)
        {
            var bits = selector(instruction);
            if (instruction == null || string.IsNullOrWhiteSpace(bits))
                continue;

            var line = $"{instruction.displayName} -> {bits.Trim()}";
            if (!lines.Contains(line))
                lines.Add(line);
        }

        return lines.Count == 0
            ? $"No {title.ToLowerInvariant()} available."
            : $"{title}\n\n{string.Join("\n", lines)}";
    }
}

using System;

/// <summary>
/// Owns the staged Practice-mode decode checks that happen before the shared
/// register-scanner step begins.
/// </summary>
public sealed class PracticeDecodeFlow
{
    int m_RevealedHintCount;

    /// <summary>
    /// Tracks whether the learner has already identified the opcode and thus
    /// unlocked the rest of the practice-decode fields.
    /// </summary>
    public bool IsOpcodeConfirmed { get; private set; }

    /// <summary>
    /// Clears staged practice-decode state when the lesson resets or returns to
    /// the pre-start intro panel.
    /// </summary>
    public void Reset(DecodePanelController panel, ref bool isRefreshing)
    {
        IsOpcodeConfirmed = false;
        m_RevealedHintCount = 0;
        panel?.ResetPracticeControls(ref isRefreshing);
        panel?.SetPracticeHintText(string.Empty);
    }

    /// <summary>
    /// Returns the learner-facing decode summary that appears after opcode
    /// confirmation and stays visible while the remaining fields are solved.
    /// </summary>
    public string GetDecodeStatusText(PracticeInstructionDefinition instruction)
    {
        if (!IsOpcodeConfirmed || instruction == null)
            return string.Empty;

        return $"Instruction Type: {instruction.GetInstructionTypeLabel()}\n" +
               $"Source Registers Needed: {instruction.GetRequiredSourceRegisterCount()}";
    }

    /// <summary>
    /// Validates the next staged Practice decode action.
    /// </summary>
    public void HandleContinue(
        CpuLessonFlow lessonFlow,
        DecodePanelController panel,
        Action<string, bool> reportFeedback)
    {
        if (lessonFlow == null || panel == null || reportFeedback == null)
            return;

        var instruction = lessonFlow.CurrentPracticeInstruction;
        if (instruction == null)
            return;

        if (!IsOpcodeConfirmed)
        {
            ValidateOpcode(panel, instruction, reportFeedback);
            return;
        }

        ValidateRemainingFields(lessonFlow, panel, instruction, reportFeedback);
    }

    /// <summary>
    /// Reveals one more staged practice hint each time the learner presses the
    /// hint button.
    /// </summary>
    public void RevealNextHint(CpuLessonFlow lessonFlow, DecodePanelController panel)
    {
        if (lessonFlow == null || panel == null)
            return;

        var instruction = lessonFlow.CurrentPracticeInstruction;
        if (instruction == null)
            return;

        m_RevealedHintCount++;
        panel.SetPracticeHintText(BuildHintText(instruction, m_RevealedHintCount));
    }

    void ValidateOpcode(
        DecodePanelController panel,
        PracticeInstructionDefinition instruction,
        Action<string, bool> reportFeedback)
    {
        var selectedOpcode = NormalizeBits(panel.GetSelectedPracticeOpcode());
        if (string.IsNullOrWhiteSpace(selectedOpcode))
        {
            reportFeedback("Select an opcode first.", true);
            return;
        }

        var expectedOpcode = NormalizeBits(instruction.expectedOpcodeBits);
        if (!string.Equals(selectedOpcode, expectedOpcode, StringComparison.Ordinal))
        {
            reportFeedback("That opcode does not match this encoded instruction.", true);
            return;
        }

        IsOpcodeConfirmed = true;
        reportFeedback($"Opcode confirmed. This is an {instruction.GetInstructionTypeLabel()} instruction.", false);
    }

    void ValidateRemainingFields(
        CpuLessonFlow lessonFlow,
        DecodePanelController panel,
        PracticeInstructionDefinition instruction,
        Action<string, bool> reportFeedback)
    {
        var selectedRs = NormalizeBits(panel.GetSelectedPracticeRs());
        if (!BitsMatch(selectedRs, instruction.expectedRsBits))
        {
            reportFeedback("The selected rs field is incorrect.", true);
            return;
        }

        var selectedRt = NormalizeBits(panel.GetSelectedPracticeRt());
        if (!BitsMatch(selectedRt, instruction.expectedRtBits))
        {
            reportFeedback("The selected rt field is incorrect.", true);
            return;
        }

        var shouldUseImmediate = instruction.UsesImmediateField();
        if (panel.GetPracticeImmediateToggleValue() != shouldUseImmediate)
        {
            reportFeedback(
                shouldUseImmediate
                    ? "This instruction does use an immediate field."
                    : "This instruction does not use an immediate field.",
                true);
            return;
        }

        if (shouldUseImmediate)
        {
            var selectedImmediate = NormalizeBits(panel.GetSelectedPracticeImmediate());
            if (!BitsMatch(selectedImmediate, instruction.expectedImmediateBits))
            {
                reportFeedback("The selected immediate field is incorrect.", true);
                return;
            }
        }

        var shouldUseFunct = instruction.UsesFunctField();
        if (panel.GetPracticeFunctToggleValue() != shouldUseFunct)
        {
            reportFeedback(
                shouldUseFunct
                    ? "This instruction does use a funct field."
                    : "This instruction does not use a funct field.",
                true);
            return;
        }

        if (shouldUseFunct)
        {
            var selectedFunct = NormalizeBits(panel.GetSelectedPracticeFunct());
            if (!BitsMatch(selectedFunct, instruction.expectedFunctBits))
            {
                reportFeedback("The selected funct field is incorrect.", true);
                return;
            }
        }

        reportFeedback("Practice decode confirmed. Continue into register collection.", false);
        lessonFlow.Advance();
    }

    static string BuildHintText(PracticeInstructionDefinition instruction, int hintIndex)
    {
        if (instruction == null)
            return string.Empty;

        return hintIndex switch
        {
            1 => "Start with the opcode. It is the first 6 bits of the 32-bit instruction.",
            2 => $"After the opcode, identify the two 5-bit source-register fields needed by this {instruction.GetInstructionTypeLabel()} instruction.",
            3 => instruction.UsesFunctField()
                ? "This encoded instruction uses a funct field and does not rely on an immediate field."
                : instruction.UsesImmediateField()
                    ? "This encoded instruction uses an immediate field instead of a funct field."
                    : "Focus on which optional field groups actually belong to this encoding.",
            _ => $"This decode should reveal {instruction.GetRequiredSourceRegisterCount()} source register field(s) before the scanner stage begins.",
        };
    }

    static bool BitsMatch(string selectedBits, string expectedBits)
    {
        return string.Equals(selectedBits, NormalizeBits(expectedBits), StringComparison.Ordinal);
    }

    static string NormalizeBits(string rawBits)
    {
        return string.IsNullOrWhiteSpace(rawBits)
            ? string.Empty
            : rawBits.Replace(" ", string.Empty).Replace("\n", string.Empty).Replace("\r", string.Empty).Trim();
    }
}

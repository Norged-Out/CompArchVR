using System;
using System.Collections.Generic;

public sealed class PracticeDecodeFlow
{
    enum PracticeHintTarget
    {
        None,
        Opcode,
        Rs,
        Rt,
        Immediate,
        Rd,
        Funct,
    }

    int m_MaxAttempts = 4;
    int m_MaxHints = 3;
    int m_RemainingAttempts = 4;
    int m_RemainingHints = 3;

    public bool IsOpcodeConfirmed { get; private set; }
    public bool IsFailed { get; private set; }

    public void Configure(int maxAttempts, int maxHints)
    {
        m_MaxAttempts = Math.Max(1, maxAttempts);
        m_MaxHints = Math.Max(0, maxHints);
    }

    public void Reset(DecodePanelController panel)
    {
        IsOpcodeConfirmed = false;
        IsFailed = false;
        m_RemainingAttempts = m_MaxAttempts;
        m_RemainingHints = m_MaxHints;
        panel?.ResetPracticeControls();
        panel?.SetPracticeHintText(string.Empty);
    }

    public string GetDecodeStatusText(PracticeInstructionDefinition instruction)
    {
        if (!IsOpcodeConfirmed || instruction == null)
            return string.Empty;

        return $"Instruction identified: {instruction.GetDecodedInstructionLabel()}\n" +
               $"Instruction Type: {instruction.GetInstructionTypeLabel()}";
    }

    public void HandleContinue(
        CpuLessonFlow lessonFlow,
        DecodePanelController panel,
        Action<string, bool> reportFeedback,
        Action onFailure)
    {
        if (lessonFlow == null || panel == null || reportFeedback == null)
            return;

        if (IsFailed)
        {
            lessonFlow.ResetLesson();
            return;
        }

        var instruction = lessonFlow.CurrentPracticeInstruction;
        if (instruction == null)
            return;

        var input = panel.GetPracticeInputState();
        if (!IsOpcodeConfirmed)
        {
            ValidateOpcode(input, instruction, reportFeedback, onFailure);
            return;
        }

        ValidateRemainingFields(lessonFlow, input, instruction, reportFeedback, onFailure);
    }

    public void RevealNextHint(CpuLessonFlow lessonFlow, DecodePanelController panel)
    {
        if (lessonFlow == null || panel == null)
            return;

        var instruction = lessonFlow.CurrentPracticeInstruction;
        if (instruction == null)
            return;

        if (m_RemainingHints <= 0)
        {
            panel.SetPracticeHintText(FormatHint("No hints remaining.", 0));
            return;
        }

        var hintTarget = ResolveHintTarget(panel.GetPracticeInputState(), instruction);
        if (hintTarget == PracticeHintTarget.None)
        {
            panel.SetPracticeHintText(FormatHint("No hint is needed for the current decode state.", m_RemainingHints));
            return;
        }

        m_RemainingHints = Math.Max(0, m_RemainingHints - 1);
        panel.SetPracticeHintText(FormatHint(BuildHintText(hintTarget), m_RemainingHints));
    }

    void ValidateOpcode(
        PracticeDecodeInputState input,
        PracticeInstructionDefinition instruction,
        Action<string, bool> reportFeedback,
        Action onFailure)
    {
        if (!BitsMatch(input.OpcodeBits, instruction.expectedOpcodeBits))
        {
            ReportIncorrect(reportFeedback, onFailure, "opcode");
            return;
        }

        IsOpcodeConfirmed = true;
        reportFeedback(
            FormatFeedback(
                $"Opcode confirmed. This is an {instruction.GetInstructionTypeLabel()} instruction.",
                m_RemainingAttempts),
            false);
    }

    void ValidateRemainingFields(
        CpuLessonFlow lessonFlow,
        PracticeDecodeInputState input,
        PracticeInstructionDefinition instruction,
        Action<string, bool> reportFeedback,
        Action onFailure)
    {
        var incorrectFields = new List<string>();

        ValidateRequiredField(incorrectFields, "rs", input.RsBits, instruction.expectedRsBits);
        ValidateRequiredField(incorrectFields, "rt", input.RtBits, instruction.expectedRtBits);
        ValidateOptionalField(incorrectFields, "rd", input.UseRd, input.RdBits, instruction.expectedRdBits);
        ValidateOptionalField(incorrectFields, "immediate", input.UseImmediate, input.ImmediateBits, instruction.expectedImmediateBits, true);
        ValidateOptionalField(incorrectFields, "funct", input.UseFunct, input.FunctBits, instruction.expectedFunctBits);

        if (incorrectFields.Count > 0)
        {
            ReportIncorrect(reportFeedback, onFailure, incorrectFields.ToArray());
            return;
        }

        reportFeedback(
            FormatFeedback("Practice decode confirmed. Continue into register collection.", m_RemainingAttempts),
            false);
        lessonFlow.Advance();
    }

    void ValidateRequiredField(List<string> incorrectFields, string fieldLabel, string selectedBits, string expectedBits)
    {
        if (!BitsMatch(selectedBits, expectedBits))
            incorrectFields.Add(fieldLabel);
    }

    void ValidateOptionalField(
        List<string> incorrectFields,
        string fieldLabel,
        bool useField,
        string selectedBits,
        string expectedBits,
        bool allowLeadingZeroTrim = false)
    {
        var shouldUseField = !string.IsNullOrWhiteSpace(expectedBits);
        if (useField != shouldUseField)
        {
            incorrectFields.Add($"{fieldLabel} toggle");
            return;
        }

        if (shouldUseField && !BitsMatch(selectedBits, expectedBits, allowLeadingZeroTrim))
            incorrectFields.Add(fieldLabel);
    }

    void ReportIncorrect(Action<string, bool> reportFeedback, Action onFailure, params string[] incorrectFields)
    {
        m_RemainingAttempts = Math.Max(0, m_RemainingAttempts - 1);

        if (m_RemainingAttempts <= 0)
        {
            IsFailed = true;
            reportFeedback(
                FormatFeedback($"Practice decode failed: {string.Join(", ", incorrectFields)}", m_RemainingAttempts),
                true);
            onFailure?.Invoke();
            return;
        }

        reportFeedback(
            FormatFeedback($"Incorrect: {string.Join(", ", incorrectFields)}", m_RemainingAttempts),
            true);
    }

    PracticeHintTarget ResolveHintTarget(PracticeDecodeInputState input, PracticeInstructionDefinition instruction)
    {
        if (input == null || instruction == null)
            return PracticeHintTarget.None;

        if (!IsOpcodeConfirmed)
            return PracticeHintTarget.Opcode;

        if (!BitsMatch(input.RsBits, instruction.expectedRsBits))
            return PracticeHintTarget.Rs;

        if (!BitsMatch(input.RtBits, instruction.expectedRtBits))
            return PracticeHintTarget.Rt;

        if (ShouldHintOptionalField(input.UseImmediate, input.ImmediateBits, instruction.expectedImmediateBits))
            return PracticeHintTarget.Immediate;

        if (ShouldHintOptionalField(input.UseRd, input.RdBits, instruction.expectedRdBits))
            return PracticeHintTarget.Rd;

        if (ShouldHintOptionalField(input.UseFunct, input.FunctBits, instruction.expectedFunctBits))
            return PracticeHintTarget.Funct;

        return PracticeHintTarget.None;
    }

    static bool ShouldHintOptionalField(bool useField, string selectedBits, string expectedBits)
    {
        if (string.IsNullOrWhiteSpace(expectedBits))
            return false;

        return !useField || !BitsMatch(selectedBits, expectedBits);
    }

    static string BuildHintText(PracticeHintTarget hintTarget)
    {
        return hintTarget switch
        {
            PracticeHintTarget.Opcode => "The opcode is the leftmost 6 bits of the 32-bit instruction.",
            PracticeHintTarget.Rs => "rs is the first 5-bit register field after the opcode.",
            PracticeHintTarget.Rt => "rt is the second 5-bit register field after the opcode.",
            PracticeHintTarget.Immediate => "This instruction uses an immediate field on the right side instead of rd and funct.",
            PracticeHintTarget.Rd => "rd is the destination register field that appears after rs and rt in an R-type instruction.",
            PracticeHintTarget.Funct => "The funct field is the final 6 bits on the right side of an R-type instruction.",
            _ => string.Empty,
        };
    }

    static bool BitsMatch(string selectedBits, string expectedBits, bool allowLeadingZeroTrim = false)
    {
        var normalizedSelectedBits = PracticeDecodeBitText.Normalize(selectedBits);
        var normalizedExpectedBits = PracticeDecodeBitText.Normalize(expectedBits);

        if (allowLeadingZeroTrim)
            return string.Equals(normalizedSelectedBits.TrimStart('0'), normalizedExpectedBits.TrimStart('0'), StringComparison.Ordinal);

        return string.Equals(normalizedSelectedBits, normalizedExpectedBits, StringComparison.Ordinal);
    }

    static string FormatFeedback(string message, int remainingAttempts)
    {
        return $"{message}\nChances remaining: {remainingAttempts}";
    }

    static string FormatHint(string message, int remainingHints)
    {
        return $"{message}\nHints remaining: {remainingHints}";
    }

}

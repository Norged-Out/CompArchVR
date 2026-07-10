using System;

/// <summary>
/// Owns the decode-phase state machine that sits between the UI and lesson flow.
/// It decides whether decode is currently checking opcode, funct, or register setup.
/// </summary>
public sealed class DecodeGuideFlow
{
    /// <summary>
    /// Tracks whether the decode panel has already accepted the opcode for an
    /// R-type instruction and is now waiting on the funct field.
    /// </summary>
    public bool IsFunctSelectionActive { get; private set; }

    /// <summary>
    /// Clears decode-specific state when the lesson restarts or exits decode.
    /// </summary>
    public void Reset(DecodePanelController panel, ref bool isRefreshing)
    {
        panel?.ResetDropdowns(ref isRefreshing);
        IsFunctSelectionActive = false;
    }

    /// <summary>
    /// Determines which authored decode sub-step should be visible right now.
    /// </summary>
    public DecodeSelectionMode GetSelectionMode(CpuLessonFlow lessonFlow)
    {
        var step = lessonFlow != null ? lessonFlow.CurrentStep : null;
        if (step == null)
            return DecodeSelectionMode.None;

        if (step.requiredInteraction == InstructionStepInteractionType.RegisterSelection)
            return DecodeSelectionMode.Registers;

        if (step.highlightedNode != DatapathNodeId.InstructionMemory)
            return DecodeSelectionMode.None;

        return IsFunctSelectionActive ? DecodeSelectionMode.Funct : DecodeSelectionMode.Opcode;
    }

    /// <summary>
    /// Processes the decode panel's Continue button based on the active sub-step.
    /// </summary>
    public void HandleContinue(
        CpuLessonFlow lessonFlow,
        DecodePanelController panel,
        Action<string, bool> reportFeedback,
        ref bool isRefreshing)
    {
        if (lessonFlow == null || panel == null || reportFeedback == null)
            return;

        switch (GetSelectionMode(lessonFlow))
        {
            case DecodeSelectionMode.Opcode:
                HandleOpcodeContinue(lessonFlow, panel, reportFeedback, ref isRefreshing);
                break;
            case DecodeSelectionMode.Funct:
                HandleFunctContinue(lessonFlow, panel, reportFeedback);
                break;
            default:
                lessonFlow.Advance();
                break;
        }
    }

    /// <summary>
    /// Identifies whether the selected instruction requires a funct field check.
    /// </summary>
    public static bool InstructionUsesFunct(InstructionDefinition instruction)
    {
        return instruction != null &&
               !string.IsNullOrWhiteSpace(instruction.functBits) &&
               string.Equals(instruction.opcodeBits != null ? instruction.opcodeBits.Trim() : string.Empty, "000000", StringComparison.Ordinal);
    }

    /// <summary>
    /// Validates the learner's opcode choice and either advances into funct checking
    /// or unlocks operand setup.
    /// </summary>
    void HandleOpcodeContinue(
        CpuLessonFlow lessonFlow,
        DecodePanelController panel,
        Action<string, bool> reportFeedback,
        ref bool isRefreshing)
    {
        var instruction = lessonFlow.CurrentInstruction;
        if (instruction == null)
            return;

        var selectedOpcode = panel.GetSelectedOpcode();
        if (string.IsNullOrWhiteSpace(selectedOpcode))
        {
            reportFeedback("Select an opcode first.", true);
            return;
        }

        var expectedOpcode = instruction.opcodeBits != null ? instruction.opcodeBits.Trim() : string.Empty;
        if (!string.Equals(selectedOpcode, expectedOpcode, StringComparison.Ordinal))
        {
            reportFeedback("That opcode does not match the selected instruction.", true);
            return;
        }

        if (InstructionUsesFunct(instruction))
        {
            IsFunctSelectionActive = true;
            panel.ResetFunctDropdown(ref isRefreshing);
            reportFeedback("Opcode confirmed. Now identify the funct field.", false);
            return;
        }

        reportFeedback("Opcode confirmed. Continue into operand setup.", false);
        lessonFlow.Advance();
    }

    /// <summary>
    /// Validates the funct choice for R-type decode before unlocking register setup.
    /// </summary>
    void HandleFunctContinue(
        CpuLessonFlow lessonFlow,
        DecodePanelController panel,
        Action<string, bool> reportFeedback)
    {
        var instruction = lessonFlow.CurrentInstruction;
        if (instruction == null)
            return;

        var selectedFunct = panel.GetSelectedFunct();
        if (string.IsNullOrWhiteSpace(selectedFunct))
        {
            reportFeedback("Select a funct value first.", true);
            return;
        }

        var expectedFunct = instruction.functBits != null ? instruction.functBits.Trim() : string.Empty;
        if (!string.Equals(selectedFunct, expectedFunct, StringComparison.Ordinal))
        {
            reportFeedback("That funct value does not match the selected instruction.", true);
            return;
        }

        IsFunctSelectionActive = false;
        reportFeedback("Funct confirmed. Continue into operand setup.", false);
        lessonFlow.Advance();
    }
}

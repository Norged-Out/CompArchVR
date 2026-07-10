using UnityEngine;

/// <summary>
/// Pure Program Counter update logic.
/// This service owns branch-condition evaluation, PCSrc computation, and
/// validation rules for the final PC update step.
/// </summary>
public sealed class PcBranchService
{
    /// <summary>
    /// Lightweight summary of the currently computed PC update state.
    /// </summary>
    public readonly struct Evaluation
    {
        public Evaluation(bool conditionMet, int pcSrc, string nextPcText)
        {
            ConditionMet = conditionMet;
            PcSrc = pcSrc;
            NextPcText = nextPcText;
        }

        public bool ConditionMet { get; }
        public int PcSrc { get; }
        public string NextPcText { get; }
    }

    /// <summary>
    /// Verifies the current PC update station state against the active
    /// instruction's expected control and datapath behavior.
    /// </summary>
    public bool TryValidate(
        InstructionDefinition instruction,
        string branchValue,
        string jumpValue,
        int pcIncrement,
        PcUpdatePacketScanner immediateScanner,
        DataPacketToken shiftedImmediatePacket,
        PcUpdatePacketScanner zeroScanner,
        BranchConditionKind selectedCondition,
        out string validationMessage)
    {
        validationMessage = string.Empty;

        if (pcIncrement != 4)
        {
            validationMessage = "PC + 4 is not set yet.";
            return false;
        }

        if (instruction == null)
            return true;

        if (branchValue != instruction.GetExpectedBranchControlValue())
        {
            validationMessage = "Branch does not match this instruction.";
            return false;
        }

        if (jumpValue != instruction.GetExpectedJumpControlValue())
        {
            validationMessage = "Jump does not match this instruction.";
            return false;
        }

        if (!instruction.UsesBranchDecision())
            return true;

        if (immediateScanner == null || immediateScanner.AcceptedPacket == null)
        {
            validationMessage = "The branch offset packet is still missing.";
            return false;
        }

        if (immediateScanner.AcceptedPacket != shiftedImmediatePacket)
        {
            validationMessage = "Shift the branch immediate left by 2 before confirming.";
            return false;
        }

        if (zeroScanner == null || zeroScanner.AcceptedPacket == null)
        {
            validationMessage = "The zero-result packet is still missing.";
            return false;
        }

        if (selectedCondition != instruction.GetExpectedBranchCondition())
        {
            validationMessage = "Branch condition does not match this instruction.";
            return false;
        }

        return true;
    }

    /// <summary>
    /// Converts the authored dropdown index into the branch-condition enum used
    /// by instruction definitions.
    /// </summary>
    public BranchConditionKind GetSelectedBranchCondition(int dropdownValue)
    {
        return dropdownValue switch
        {
            1 => BranchConditionKind.Equal,
            2 => BranchConditionKind.NotEqual,
            _ => BranchConditionKind.None,
        };
    }

    /// <summary>
    /// Computes whether the chosen branch condition is currently satisfied and
    /// which next-PC path the learner has selected.
    /// </summary>
    public Evaluation Evaluate(
        InstructionDefinition instruction,
        string branchValue,
        int pcIncrement,
        int zeroValue,
        BranchConditionKind selectedCondition)
    {
        if (instruction == null || !instruction.UsesBranchDecision())
            return new Evaluation(false, 0, $"PC + {pcIncrement}");

        var conditionMet = selectedCondition switch
        {
            BranchConditionKind.Equal => zeroValue == 1,
            BranchConditionKind.NotEqual => zeroValue == 0,
            _ => false,
        };

        var pcSrc = branchValue == "1" && conditionMet ? 1 : 0;
        var nextPcText = pcSrc == 1 ? $"PC + {pcIncrement} + Branch Offset" : $"PC + {pcIncrement}";
        return new Evaluation(conditionMet, pcSrc, nextPcText);
    }

    /// <summary>
    /// Returns the text shown in the lesson panel while the learner is still
    /// solving the PC update step.
    /// </summary>
    public string BuildLessonRuntimeText(InstructionDefinition instruction)
    {
        if (instruction == null)
            return string.Empty;

        if (instruction.UsesBranchDecision())
        {
            return "Use the control outputs and datapath results from earlier stages to decide whether the Program Counter keeps the sequential path or takes the branch target.";
        }

        if (instruction.UsesJumpDecision())
        {
            return "Use the final control signals to decide whether the Program Counter follows the normal sequential path or jumps elsewhere.";
        }

        return "Close the datapath cycle by confirming the normal sequential Program Counter update for this instruction.";
    }

    /// <summary>
    /// Returns the fixed end-state recap shown once the PC update is accepted.
    /// </summary>
    public string BuildLessonEndText()
    {
        return "Program Counter update confirmed. Continue to finish the lesson.";
    }
}

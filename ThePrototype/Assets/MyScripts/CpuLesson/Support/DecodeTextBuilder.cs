using System.Collections.Generic;

/// <summary>
/// Builds decode-panel runtime text from the current lesson state.
/// </summary>
public sealed class DecodeTextBuilder
{
    /// <summary>
    /// Shows the currently selected assembly instruction while the learner identifies
    /// either the opcode or the funct field.
    /// </summary>
    public string BuildAssemblySelectionText(InstructionDefinition instruction)
    {
        return instruction == null ? string.Empty : $"Assembly: {instruction.assemblyInstructionText}";
    }

    /// <summary>
    /// Shows register placement progress and the current operand target during the
    /// scanner portion of instruction decode.
    /// </summary>
    public string BuildRegisterSelectionText(CpuLessonFlow lessonFlow, InstructionFlowStep step)
    {
        var instruction = lessonFlow != null ? lessonFlow.CurrentInstruction : null;
        if (lessonFlow == null || instruction == null || step == null)
            return string.Empty;

        if (step.requiredInteraction != InstructionStepInteractionType.RegisterSelection)
            return step.explanation;

        var lines = new List<string>();
        var requiredRoles = LessonChecks.GetRequiredRoles(instruction, step);

        for (var index = 0; index < requiredRoles.Length; index++)
        {
            var role = requiredRoles[index];
            var registerName = instruction.GetExpectedRegisterName(role);
            var scannerName = GetScannerLabel(role);
            var status = index < lessonFlow.CurrentRegisterSelectionIndex ? "done" : "pending";
            lines.Add($"{scannerName}: {registerName} [{status}]");
        }

        if (instruction.usesImmediate)
        {
            var immediateStatus = lessonFlow.RegisterSelectionReadyToContinue ? "ready to generate" : "locked";
            lines.Add($"Immediate packet: {instruction.expectedImmediateValue} [{immediateStatus}]");
        }

        var nextAction = lessonFlow.RegisterSelectionReadyToContinue
            ? instruction.usesImmediate
                ? "Press Continue to generate the immediate packet and proceed to Execution."
                : "Press Continue to proceed to Execution."
            : $"Current target: {GetCurrentDecodeTargetLabel(lessonFlow, instruction, step)}.";

        return $"{string.Join("\n", lines)}\n\n{nextAction}";
    }

    /// <summary>
    /// Converts a logical register role into the authored scanner name visible in the scene.
    /// </summary>
    public static string GetScannerLabel(InstructionRegisterRole registerRole)
    {
        return registerRole switch
        {
            InstructionRegisterRole.Rs => "Read Register 1",
            InstructionRegisterRole.Rt => "Read Register 2",
            InstructionRegisterRole.Rd => "Write Register",
            _ => "the correct scanner",
        };
    }

    /// <summary>
    /// Reports which register the learner must place next when the decode scan is still in progress.
    /// </summary>
    static string GetCurrentDecodeTargetLabel(
        CpuLessonFlow lessonFlow,
        InstructionDefinition instruction,
        InstructionFlowStep step)
    {
        var requiredRoles = LessonChecks.GetRequiredRoles(instruction, step);
        var currentIndex = lessonFlow != null ? lessonFlow.CurrentRegisterSelectionIndex : 0;
        if (currentIndex < 0 || currentIndex >= requiredRoles.Length)
            return "Place the required register";

        var role = requiredRoles[currentIndex];
        return $"{instruction.GetExpectedRegisterName(role)} on {GetScannerLabel(role)}";
    }
}

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

        if (lessonFlow.CurrentMode == LessonMode.Practice && lessonFlow.CurrentPracticeInstruction != null)
            return BuildPracticeRegisterSelectionText(lessonFlow, step, instruction, lessonFlow.CurrentPracticeInstruction);

        var lines = new List<string>();
        var requiredRoles = LessonChecks.GetRequiredRoles(instruction, step);

        for (var index = 0; index < requiredRoles.Length; index++)
        {
            var role = requiredRoles[index];
            var registerName = instruction.GetExpectedRegisterName(role);
            var scannerName = GetScannerLabel(role);
            var status = lessonFlow.IsDecodeRegisterRoleSelected(role) ? "done" : "pending";
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
            : $"Remaining targets: {GetRemainingDecodeTargetLabel(lessonFlow, instruction, step)}.";

        return $"{string.Join("\n", lines)}\n\n{nextAction}";
    }

    string BuildPracticeRegisterSelectionText(
        CpuLessonFlow lessonFlow,
        InstructionFlowStep step,
        InstructionDefinition instruction,
        PracticeInstructionDefinition practiceInstruction)
    {
        var lines = new List<string>
        {
            $"Instruction identified: {practiceInstruction.GetDecodedInstructionLabel()}",
            $"Required source registers: {practiceInstruction.GetRequiredSourceRegisterCount()}",
            string.Empty
        };

        var requiredRoles = LessonChecks.GetRequiredRoles(instruction, step);
        for (var index = 0; index < requiredRoles.Length; index++)
        {
            var role = requiredRoles[index];
            var registerBits = practiceInstruction.GetExpectedRegisterBits(role);
            var registerNumber = practiceInstruction.GetExpectedRegisterNumber(role);
            var status = lessonFlow.IsDecodeRegisterRoleSelected(role) ? "Scanned" : "Pending";

            if (!string.IsNullOrWhiteSpace(registerBits) && registerNumber >= 0)
                lines.Add($"{index + 1}. {role.ToString().ToLowerInvariant()} = {registerBits} (#{registerNumber}) [{status}]");
            else
                lines.Add($"{index + 1}. {role.ToString().ToLowerInvariant()} [{status}]");
        }

        lines.Add(string.Empty);
        lines.Add(
            lessonFlow.RegisterSelectionReadyToContinue
                ? "Remaining targets: All required registers scanned. Press Continue."
                : $"Remaining targets: {GetPracticeRemainingTargetLabel(lessonFlow, step, instruction, practiceInstruction)}");

        return string.Join("\n", lines);
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
    static string GetRemainingDecodeTargetLabel(
        CpuLessonFlow lessonFlow,
        InstructionDefinition instruction,
        InstructionFlowStep step)
    {
        var requiredRoles = LessonChecks.GetRequiredRoles(instruction, step);
        if (requiredRoles == null || requiredRoles.Length == 0)
            return "Place the required register";

        var pendingTargets = new List<string>();
        foreach (var role in requiredRoles)
        {
            if (lessonFlow != null && lessonFlow.IsDecodeRegisterRoleSelected(role))
                continue;

            pendingTargets.Add($"{instruction.GetExpectedRegisterName(role)} on {GetScannerLabel(role)}");
        }

        return pendingTargets.Count > 0
            ? string.Join("; ", pendingTargets)
            : "All required registers are ready";
    }

    static string GetPracticeRemainingTargetLabel(
        CpuLessonFlow lessonFlow,
        InstructionFlowStep step,
        InstructionDefinition instruction,
        PracticeInstructionDefinition practiceInstruction)
    {
        var requiredRoles = LessonChecks.GetRequiredRoles(instruction, step);
        if (requiredRoles == null || requiredRoles.Length == 0)
            return "Scan the required register";

        var pendingTargets = new List<string>();
        foreach (var role in requiredRoles)
        {
            if (lessonFlow != null && lessonFlow.IsDecodeRegisterRoleSelected(role))
                continue;

            var registerNumber = practiceInstruction.GetExpectedRegisterNumber(role);
            var scannerLabel = GetScannerLabel(role);

            pendingTargets.Add(registerNumber >= 0
                ? $"{role.ToString().ToLowerInvariant()} -> #{registerNumber} on {scannerLabel}"
                : $"{role.ToString().ToLowerInvariant()} on {scannerLabel}");
        }

        return pendingTargets.Count > 0
            ? string.Join("; ", pendingTargets)
            : "All required registers are ready";
    }
}

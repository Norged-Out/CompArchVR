using System;

public partial class LessonGuideController
{
    string BuildIntroBody(InstructionFlowStep step)
    {
        var instruction = m_LessonFlow.CurrentInstruction;
        if (step.stepName.IndexOf("Fetch", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            if (m_LessonFlow.UsesInstructionTerminals)
            {
                var transportStatus = m_LessonFlow.IsInstructionReadyForDecode
                    ? "The module is docked at the decode terminal. Instruction Decode is unlocking now."
                    : "The selected instruction has been uploaded to the fetch terminal. Pick up the module, carry it to the decode terminal, and dock it there to unlock Instruction Decode.";

                return
                    $"Instruction fetch uses the Program Counter to locate the next instruction in memory.\n\n" +
                    $"Instruction: {instruction.displayName}\n" +
                    $"Assembly: {instruction.assemblyInstructionText}\n\n" +
                    $"{transportStatus}";
            }

            return
                $"Instruction fetch uses the Program Counter to locate the next instruction in memory.\n\n" +
                $"Instruction: {instruction.displayName}\n" +
                $"Assembly: {instruction.assemblyInstructionText}\n\n" +
                $"{step.explanation}\n\n" +
                "When you are ready, continue into instruction decode.";
        }

        return
            $"Instruction: {instruction.displayName}\n" +
            $"Assembly: {instruction.assemblyInstructionText}\n\n" +
            $"{step.explanation}\n\n" +
            $"Next: {GetNextStageLabel(step)}.";
    }

    string GetNextStageLabel(InstructionFlowStep currentStep)
    {
        var instruction = m_LessonFlow != null ? m_LessonFlow.CurrentInstruction : null;
        if (currentStep == null || instruction == null)
            return "Continue";

        if (currentStep.requiredInteraction == InstructionStepInteractionType.Completion)
            return "Restart";

        if (currentStep.requiredInteraction == InstructionStepInteractionType.RegisterSelection ||
            currentStep.highlightedNode == DatapathNodeId.InstructionMemory)
        {
            return currentStep.highlightedNode == DatapathNodeId.InstructionMemory ? "Register Setup" : "Execution";
        }

        if (currentStep.requiredInteraction == InstructionStepInteractionType.AluExecution)
        {
            return instruction.UsesInteractiveMemoryPhase() ? "Memory Access" :
                instruction.UsesWriteBackPhase() ? "Write Back" : "Recap";
        }

        if (currentStep.highlightedNode == DatapathNodeId.DataMemory)
            return instruction.UsesWriteBackPhase() ? "Write Back" : "Recap";

        if (currentStep.requiredInteraction == InstructionStepInteractionType.WriteBackExecution)
            return "Program Counter Update";

        if (currentStep.requiredInteraction == InstructionStepInteractionType.PcUpdateExecution)
            return "Restart";

        return "Continue";
    }
}

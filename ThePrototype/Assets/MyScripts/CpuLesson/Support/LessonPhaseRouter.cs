/// <summary>
/// Centralizes lesson-phase visibility rules so the coordinator and intro panel use
/// the same interpretation of the current flow state.
/// </summary>
public sealed class LessonPhaseRouter
{
    /// <summary>
    /// Returns true when the shared intro/fetch panel should remain visible.
    /// </summary>
    public bool ShouldShowIntroPanel(CpuLessonFlow lessonFlow)
    {
        return !ShouldShowDecodePanel(lessonFlow) &&
               !ShouldShowExecutionPanel(lessonFlow) &&
               !ShouldShowMemoryPanel(lessonFlow) &&
               !ShouldShowWriteBackPanel(lessonFlow) &&
               !ShouldShowPcUpdatePanel(lessonFlow);
    }

    /// <summary>
    /// Returns true while instruction memory decode or register setup is active.
    /// </summary>
    public bool ShouldShowDecodePanel(CpuLessonFlow lessonFlow)
    {
        if (ShouldShowExecutionPanel(lessonFlow))
            return false;

        var step = lessonFlow != null ? lessonFlow.CurrentStep : null;
        if (step == null)
            return false;

        return step.highlightedNode == DatapathNodeId.InstructionMemory ||
               step.requiredInteraction == InstructionStepInteractionType.RegisterSelection;
    }

    /// <summary>
    /// Returns true when the execution station owns the learner's next action.
    /// </summary>
    public bool ShouldShowExecutionPanel(CpuLessonFlow lessonFlow)
    {
        var step = lessonFlow != null ? lessonFlow.CurrentStep : null;
        return step != null && step.requiredInteraction == InstructionStepInteractionType.AluExecution;
    }

    /// <summary>
    /// Returns true while the memory-access station is active for load/store instructions.
    /// </summary>
    public bool ShouldShowMemoryPanel(CpuLessonFlow lessonFlow)
    {
        if (ShouldShowExecutionPanel(lessonFlow) || ShouldShowWriteBackPanel(lessonFlow))
            return false;

        var step = lessonFlow != null ? lessonFlow.CurrentStep : null;
        return step != null && step.highlightedNode == DatapathNodeId.DataMemory;
    }

    /// <summary>
    /// Returns true when write-back validation owns the learner's next action.
    /// </summary>
    public bool ShouldShowWriteBackPanel(CpuLessonFlow lessonFlow)
    {
        var step = lessonFlow != null ? lessonFlow.CurrentStep : null;
        return step != null && step.requiredInteraction == InstructionStepInteractionType.WriteBackExecution;
    }

    /// <summary>
    /// Returns true when the final program-counter update station is active.
    /// </summary>
    public bool ShouldShowPcUpdatePanel(CpuLessonFlow lessonFlow)
    {
        var step = lessonFlow != null ? lessonFlow.CurrentStep : null;
        return step != null && step.requiredInteraction == InstructionStepInteractionType.PcUpdateExecution;
    }

    /// <summary>
    /// Converts the current lesson step into a learner-facing "what happens next" label.
    /// </summary>
    public string GetNextStageLabel(CpuLessonFlow lessonFlow, InstructionFlowStep currentStep)
    {
        var instruction = lessonFlow != null ? lessonFlow.CurrentInstruction : null;
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

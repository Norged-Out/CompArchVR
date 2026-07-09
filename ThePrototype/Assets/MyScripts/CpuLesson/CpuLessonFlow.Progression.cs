using UnityEngine;

/// <summary>
/// Step progression and per-step feedback routing for the lesson state machine.
/// </summary>
public partial class CpuLessonFlow
{
    void AdvanceToNextStep()
    {
        if (m_CurrentInstruction == null || m_CurrentInstruction.flowSteps == null)
            return;

        // UI panels are toggled as part of progression. Without a small
        // debounce, the same click can occasionally be seen twice while panels
        // swap, which makes the lesson jump over authored intermediate steps.
        if (m_LastAdvanceFrame == Time.frameCount)
        {
            Debug.Log($"{k_LogPrefix} AdvanceToNextStep blocked by debounce | currentStepIndex={m_CurrentStepIndex} frame={Time.frameCount}", this);
            return;
        }

        m_LastAdvanceFrame = Time.frameCount;
        var previousStepName = CurrentStep != null ? CurrentStep.stepName : "<none>";
        var previousStepIndex = m_CurrentStepIndex;

        m_CurrentStepIndex++;
        m_CurrentRegisterSelectionIndex = 0;
        m_RegisterSelectionReadyToContinue = false;

        while (m_CurrentStepIndex < m_CurrentInstruction.flowSteps.Length &&
               ShouldSkipStep(m_CurrentInstruction.flowSteps[m_CurrentStepIndex]))
        {
            Debug.Log(
                $"{k_LogPrefix} Skipping step | stepIndex={m_CurrentStepIndex} step={m_CurrentInstruction.flowSteps[m_CurrentStepIndex].stepName} frame={Time.frameCount}",
                this);
            m_CurrentStepIndex++;
        }

        Debug.Log($"{k_LogPrefix} AdvanceToNextStep | fromIndex={previousStepIndex} fromStep={previousStepName} toIndex={m_CurrentStepIndex} frame={Time.frameCount}", this);

        if (m_CurrentStepIndex >= m_CurrentInstruction.flowSteps.Length)
        {
            m_CurrentStepIndex = m_CurrentInstruction.flowSteps.Length - 1;
            StepChanged?.Invoke(this);
            return;
        }

        PresentCurrentStep();
    }

    void PresentCurrentStep()
    {
        ConfigureScannersForCurrentStep();
        Debug.Log(
            $"{k_LogPrefix} PresentCurrentStep | stepIndex={m_CurrentStepIndex} step={CurrentStep?.stepName} interaction={CurrentStep?.requiredInteraction} highlightedNode={CurrentStep?.highlightedNode} frame={Time.frameCount}",
            this);
        StepChanged?.Invoke(this);

        switch (CurrentStep.requiredInteraction)
        {
            case InstructionStepInteractionType.ContinueButton:
                SetFeedback(IsFetchStep(CurrentStep) ? GetFetchTransportPrompt() : "Press Continue when you are ready.", false);
                break;

            case InstructionStepInteractionType.RegisterSelection:
                SetFeedback("Collect the required operands to continue.", false);
                break;

            case InstructionStepInteractionType.AluExecution:
                SetFeedback(
                    m_CurrentInstruction != null && m_CurrentInstruction.UsesInteractiveMemoryPhase()
                        ? "Set the ALU controls, place the inputs, execute the operation, then continue to Memory Access."
                        : "Set the ALU controls, place the inputs, execute the operation, then continue directly to Write Back.",
                    false);
                break;

            case InstructionStepInteractionType.WriteBackExecution:
                SetFeedback(
                    $"Write-back target: {m_CurrentInstruction.GetWriteBackTargetRegister()}. Source: {GetPacketLabel(m_CurrentInstruction.GetWriteBackPacketRole())}. Set the controls, place both inputs, and execute the transfer.",
                    false);
                break;

            case InstructionStepInteractionType.PcUpdateExecution:
                SetFeedback("Confirm how the Program Counter moves to the next instruction.", false);
                break;

            case InstructionStepInteractionType.Completion:
                SetFeedback("Lesson complete. Press Restart to play it again.", false);
                break;

            default:
                SetFeedback(string.Empty, false);
                break;
        }
    }

    bool TrySpawnImmediatePacket()
    {
        if (m_CurrentInstruction == null || !m_CurrentInstruction.usesImmediate)
            return false;

        if (m_ImmediateExtender == null)
            return false;

        m_RuntimeSelection.immediateValue = m_CurrentInstruction.expectedImmediateValue;
        return m_ImmediateExtender.SpawnImmediatePacket(m_CurrentInstruction.expectedImmediateValue);
    }

    void SetFeedback(string message, bool isFailure)
    {
        FeedbackChanged?.Invoke(message, isFailure);
    }

    bool ShouldSkipStep(InstructionFlowStep step)
    {
        if (step == null || m_CurrentInstruction == null)
            return false;

        if (step.highlightedNode == DatapathNodeId.DataMemory && !m_CurrentInstruction.UsesInteractiveMemoryPhase())
            return true;

        if ((step.highlightedNode == DatapathNodeId.WriteBack ||
             step.requiredInteraction == InstructionStepInteractionType.WriteBackExecution) &&
            !m_CurrentInstruction.UsesWriteBackPhase())
        {
            return true;
        }

        if (step.requiredInteraction == InstructionStepInteractionType.PcUpdateExecution &&
            !m_CurrentInstruction.UsesPcUpdatePhase())
        {
            return true;
        }

        return false;
    }

    string GetPostAluContinuePrompt(int resultValue)
    {
        if (m_CurrentInstruction == null)
            return $"ALU result produced: {resultValue}. Continue.";

        if (m_CurrentInstruction.UsesInteractiveMemoryPhase())
            return $"ALU result produced: {resultValue}. Continue to Memory Access.";

        if (m_CurrentInstruction.UsesWriteBackPhase())
            return $"ALU result produced: {resultValue}. Data Memory is skipped for this instruction. Continue to Write Back.";

        return $"ALU result produced: {resultValue}. Continue to Program Counter Update.";
    }
}

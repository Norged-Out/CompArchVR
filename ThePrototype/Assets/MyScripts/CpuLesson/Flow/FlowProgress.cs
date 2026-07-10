using UnityEngine;

/// <summary>
/// Step advancement, skip rules, and per-step default feedback.
/// </summary>
sealed class FlowProgress
{
    readonly CpuLessonFlow m_Flow;
    readonly LessonState m_State;
    readonly DecodeFlow m_Decode;
    readonly FetchFlow m_Fetch;

    /// <summary>
    /// Captures the lesson services needed to advance and present authored steps.
    /// </summary>
    public FlowProgress(CpuLessonFlow flow, LessonState state, DecodeFlow decode, FetchFlow fetch)
    {
        m_Flow = flow;
        m_State = state;
        m_Decode = decode;
        m_Fetch = fetch;
    }

    /// <summary>
    /// Advances to the next authored step, applying debounce protection and skip rules.
    /// </summary>
    public void AdvanceStep()
    {
        if (m_Flow.ActiveInstruction == null || m_Flow.ActiveInstruction.flowSteps == null)
            return;

        // UI panels are toggled as part of progression. Without a small
        // debounce, the same click can occasionally be seen twice while panels
        // swap, which makes the lesson jump over authored intermediate steps.
        if (!m_State.TrySetAdvanceFrame(Time.frameCount))
        {
            Debug.Log(
                $"{m_Flow.LogPrefix} AdvanceStep blocked by debounce | currentStepIndex={m_State.CurrentStepIndex} frame={Time.frameCount}",
                m_Flow);
            return;
        }

        var previousStepName = m_Flow.CurrentStep != null ? m_Flow.CurrentStep.stepName : "<none>";
        var previousStepIndex = m_State.CurrentStepIndex;

        m_State.AdvanceStep();

        while (m_State.CurrentStepIndex < m_Flow.ActiveInstruction.flowSteps.Length &&
               ShouldSkip(m_Flow.GetStepAt(m_State.CurrentStepIndex)))
        {
            var skippedStep = m_Flow.GetStepAt(m_State.CurrentStepIndex);
            Debug.Log(
                $"{m_Flow.LogPrefix} Skipping step | stepIndex={m_State.CurrentStepIndex} step={skippedStep?.stepName} frame={Time.frameCount}",
                m_Flow);
            m_State.SkipToStep(m_State.CurrentStepIndex + 1);
        }

        Debug.Log(
            $"{m_Flow.LogPrefix} AdvanceStep | fromIndex={previousStepIndex} fromStep={previousStepName} toIndex={m_State.CurrentStepIndex} frame={Time.frameCount}",
            m_Flow);

        if (m_State.CurrentStepIndex >= m_Flow.ActiveInstruction.flowSteps.Length)
        {
            m_State.SkipToStep(m_Flow.ActiveInstruction.flowSteps.Length - 1);
            m_Flow.RaiseStepChanged();
            return;
        }

        PresentStep();
    }

    /// <summary>
    /// Publishes the newly active step to UI and emits its default learner prompt.
    /// </summary>
    public void PresentStep()
    {
        var step = m_Flow.CurrentStep;
        m_Decode.ConfigureScanners(step);

        Debug.Log(
            $"{m_Flow.LogPrefix} PresentStep | stepIndex={m_State.CurrentStepIndex} step={step?.stepName} interaction={step?.requiredInteraction} highlightedNode={step?.highlightedNode} frame={Time.frameCount}",
            m_Flow);
        m_Flow.RaiseStepChanged();

        if (step == null)
        {
            m_Flow.RaiseFeedback(string.Empty, false);
            return;
        }

        switch (step.requiredInteraction)
        {
            case InstructionStepInteractionType.ContinueButton:
                m_Flow.RaiseFeedback(m_Fetch.IsFetchStep(step) ? m_Fetch.GetTransportPrompt() : "Press Continue when you are ready.", false);
                break;

            case InstructionStepInteractionType.RegisterSelection:
                m_Flow.RaiseFeedback("Collect the required operands to continue.", false);
                break;

            case InstructionStepInteractionType.AluExecution:
                m_Flow.RaiseFeedback(
                    m_Flow.ActiveInstruction != null && m_Flow.ActiveInstruction.UsesInteractiveMemoryPhase()
                        ? "Set the ALU controls, place the inputs, execute the operation, then continue to Memory Access."
                        : "Set the ALU controls, place the inputs, execute the operation, then continue directly to Write Back.",
                    false);
                break;

            case InstructionStepInteractionType.WriteBackExecution:
                m_Flow.RaiseFeedback(
                    $"Write-back target: {m_Flow.ActiveInstruction.GetWriteBackTargetRegister()}. Source: {DecodeFlow.GetPacketLabel(m_Flow.ActiveInstruction.GetWriteBackPacketRole())}. Set the controls, place both inputs, and execute the transfer.",
                    false);
                break;

            case InstructionStepInteractionType.PcUpdateExecution:
                m_Flow.RaiseFeedback("Confirm how the Program Counter moves to the next instruction.", false);
                break;

            case InstructionStepInteractionType.Completion:
                m_Flow.RaiseFeedback("Lesson complete. Press Restart to play it again.", false);
                break;

            default:
                m_Flow.RaiseFeedback(string.Empty, false);
                break;
        }
    }

    /// <summary>
    /// Produces the phase-transition prompt shown after ALU execution succeeds.
    /// </summary>
    public string GetPostAluPrompt(int resultValue)
    {
        if (m_Flow.ActiveInstruction == null)
            return $"ALU result produced: {resultValue}. Continue.";

        if (m_Flow.ActiveInstruction.UsesInteractiveMemoryPhase())
            return $"ALU result produced: {resultValue}. Continue to Memory Access.";

        if (m_Flow.ActiveInstruction.UsesWriteBackPhase())
            return $"ALU result produced: {resultValue}. Data Memory is skipped for this instruction. Continue to Write Back.";

        return $"ALU result produced: {resultValue}. Continue to Program Counter Update.";
    }

    /// <summary>
    /// Skips authored steps that do not apply to the currently selected instruction.
    /// </summary>
    bool ShouldSkip(InstructionFlowStep step)
    {
        if (step == null || m_Flow.ActiveInstruction == null)
            return false;

        if (step.highlightedNode == DatapathNodeId.DataMemory && !m_Flow.ActiveInstruction.UsesInteractiveMemoryPhase())
            return true;

        if ((step.highlightedNode == DatapathNodeId.WriteBack ||
             step.requiredInteraction == InstructionStepInteractionType.WriteBackExecution) &&
            !m_Flow.ActiveInstruction.UsesWriteBackPhase())
        {
            return true;
        }

        if (step.requiredInteraction == InstructionStepInteractionType.PcUpdateExecution &&
            !m_Flow.ActiveInstruction.UsesPcUpdatePhase())
        {
            return true;
        }

        return false;
    }
}

using UnityEngine;

/// <summary>
/// Owns learner-triggered step actions such as continue, ALU completion,
/// and write-back completion.
/// </summary>
sealed class LessonStepActions
{
    readonly CpuLessonFlow m_Flow;
    readonly LessonState m_State;
    readonly DecodeFlow m_Decode;
    readonly FetchFlow m_Fetch;
    readonly FlowProgress m_Progress;
    readonly LessonLifecycle m_Lifecycle;

    /// <summary>
    /// Captures the lesson services needed to evaluate and apply step actions.
    /// </summary>
    public LessonStepActions(
        CpuLessonFlow flow,
        LessonState state,
        DecodeFlow decode,
        FetchFlow fetch,
        FlowProgress progress,
        LessonLifecycle lifecycle)
    {
        m_Flow = flow;
        m_State = state;
        m_Decode = decode;
        m_Fetch = fetch;
        m_Progress = progress;
        m_Lifecycle = lifecycle;
    }

    /// <summary>
    /// Handles progression requests from lesson UI panels and physical phase stations.
    /// </summary>
    public void Advance()
    {
        if (!m_State.HasStarted || m_Flow.CurrentStep == null)
            return;

        Debug.Log(
            $"{m_Flow.LogPrefix} Advance requested | stepIndex={m_State.CurrentStepIndex} step={m_Flow.CurrentStep.stepName} interaction={m_Flow.CurrentStep.requiredInteraction} readyToContinue={m_State.RegisterSelectionReadyToContinue} frame={Time.frameCount}",
            m_Flow);

        switch (m_Flow.CurrentStep.requiredInteraction)
        {
            case InstructionStepInteractionType.None:
            case InstructionStepInteractionType.ContinueButton:
                if (m_Fetch.IsFetchStep(m_Flow.CurrentStep) &&
                    m_Fetch.UsesTerminals &&
                    !m_Fetch.HasDownloadedInstructionModule())
                {
                    m_Flow.RaiseFeedback(m_Fetch.GetTransportPrompt(), true);
                    m_Flow.RaiseStepChanged();
                    break;
                }

                m_Progress.AdvanceStep();
                break;

            case InstructionStepInteractionType.RegisterSelection:
                if (!m_State.RegisterSelectionReadyToContinue)
                {
                    m_Flow.RaiseFeedback("Decode work is not complete yet.", true);
                    break;
                }

                if (m_Flow.ActiveInstruction != null &&
                    m_Flow.ActiveInstruction.usesImmediate &&
                    !m_Decode.TrySpawnImmediatePacket())
                {
                    m_Flow.RaiseFeedback("Immediate Extender is missing its packet prefab or spawn anchor.", true);
                    break;
                }

                m_Progress.AdvanceStep();
                break;

            case InstructionStepInteractionType.AluExecution:
                m_Flow.RaiseFeedback("Set the ALU controls, place the inputs, and execute the operation.", false);
                break;

            case InstructionStepInteractionType.WriteBackExecution:
                if (!string.IsNullOrWhiteSpace(m_State.RuntimeSelection.confirmedWriteBackRegister) &&
                    m_State.RuntimeSelection.hasAluResult)
                {
                    m_Progress.AdvanceStep();
                }
                else
                {
                    m_Flow.RaiseFeedback("Set the write-back controls, place the register and result packet, then execute the transfer.", false);
                }

                break;

            case InstructionStepInteractionType.PcUpdateExecution:
                m_Flow.RaiseFeedback("Set PC + 4 and confirm the next PC path.", false);
                break;

            case InstructionStepInteractionType.Completion:
                m_Lifecycle.ResetLesson();
                break;
        }
    }

    /// <summary>
    /// Stores the ALU result produced by the execution station and advances the lesson.
    /// </summary>
    public void CompleteAluExecution(int resultValue)
    {
        if (!m_State.HasStarted || m_Flow.CurrentStep == null)
            return;

        if (m_Flow.CurrentStep.requiredInteraction != InstructionStepInteractionType.AluExecution)
            return;

        Debug.Log(
            $"{m_Flow.LogPrefix} CompleteAluExecution | result={resultValue} stepIndex={m_State.CurrentStepIndex} step={m_Flow.CurrentStep.stepName} frame={Time.frameCount}",
            m_Flow);

        m_State.RuntimeSelection.aluResultValue = resultValue;
        m_State.RuntimeSelection.hasAluResult = true;
        m_Flow.RaiseFeedback(m_Progress.GetPostAluPrompt(resultValue), false);
        m_Progress.AdvanceStep();
    }

    /// <summary>
    /// Applies the final write-back value after the write-back station completes its transfer.
    /// </summary>
    public void CompleteWriteBackExecution(string destinationRegister, int resultValue)
    {
        if (string.IsNullOrWhiteSpace(destinationRegister))
            return;

        Debug.Log(
            $"{m_Flow.LogPrefix} CompleteWriteBackExecution | register={destinationRegister} value={resultValue} frame={Time.frameCount}",
            m_Flow);

        m_State.RuntimeSelection.confirmedWriteBackRegister = destinationRegister;
        m_State.RuntimeSelection.aluResultValue = resultValue;
        m_State.RuntimeSelection.hasAluResult = true;
        m_Flow.RegisterBankRef?.SetRegisterValue(destinationRegister, resultValue);
        m_Flow.RaiseFeedback($"Write-back complete. {destinationRegister} now stores {resultValue}. Press Continue to finish.", false);
        m_Flow.RaiseStepChanged();
    }
}

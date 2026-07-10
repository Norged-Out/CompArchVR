using UnityEngine;

/// <summary>
/// Owns lesson start and reset behavior for the active instruction.
/// This keeps lifecycle work out of the scene-facing flow root while still
/// using the same authored scene references and runtime lesson state.
/// </summary>
sealed class LessonLifecycle
{
    readonly CpuLessonFlow m_Flow;
    readonly LessonState m_State;
    readonly DecodeFlow m_Decode;
    readonly FetchFlow m_Fetch;

    /// <summary>
    /// Captures the lesson services used during start and reset transitions.
    /// </summary>
    public LessonLifecycle(CpuLessonFlow flow, LessonState state, DecodeFlow decode, FetchFlow fetch)
    {
        m_Flow = flow;
        m_State = state;
        m_Decode = decode;
        m_Fetch = fetch;
    }

    /// <summary>
    /// Starts the current lesson instruction and prepares every dependent scene system.
    /// </summary>
    public void StartLesson()
    {
        if (m_Flow.ActiveInstruction == null)
            m_Flow.SetActiveInstructionInternal(m_Flow.LoadDefaultInstruction());

        if (m_Flow.ActiveInstruction == null ||
            m_Flow.ActiveInstruction.flowSteps == null ||
            m_Flow.ActiveInstruction.flowSteps.Length == 0)
        {
            return;
        }

        Debug.Log(
            $"{m_Flow.LogPrefix} StartLesson | instruction={m_Flow.ActiveInstruction.displayName} assembly={m_Flow.ActiveInstruction.assemblyInstructionText} frame={Time.frameCount}",
            m_Flow);

        m_Decode.Bind();
        m_State.BeginLesson(m_Flow.ActiveInstruction);
        m_Flow.ImmediateExtenderRef?.ResetScanner();
        m_Fetch.PrepareTerminals();
        m_Decode.PrepareRegisterBankForLesson();
        m_Flow.ProgressRef.PresentStep();
    }

    /// <summary>
    /// Returns the lesson to its idle state while preserving the selected instruction asset.
    /// </summary>
    public void ResetLesson()
    {
        Debug.Log($"{m_Flow.LogPrefix} ResetLesson | frame={Time.frameCount}", m_Flow);

        m_Decode.PrepareRegisterBankForLesson();
        m_State.ResetLesson(m_Flow.ActiveInstruction);
        m_Flow.ImmediateExtenderRef?.ResetScanner();
        m_Fetch.PrepareTerminals();
        m_Flow.RaiseStepChanged();
        m_Flow.RaiseFeedback(string.Empty, false);
    }
}

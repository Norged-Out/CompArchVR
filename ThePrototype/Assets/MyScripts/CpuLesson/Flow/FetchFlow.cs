using System;

/// <summary>
/// Physical instruction-fetch handoff logic shared by the fetch and decode terminals.
/// </summary>
sealed class FetchFlow
{
    readonly CpuLessonFlow m_Flow;
    readonly LessonState m_State;

    /// <summary>
    /// Captures the flow model and mutable lesson state used by the fetch step.
    /// </summary>
    public FetchFlow(CpuLessonFlow flow, LessonState state)
    {
        m_Flow = flow;
        m_State = state;
    }

    /// <summary>
    /// Returns true when both authored instruction terminals are present in the scene.
    /// </summary>
    public bool UsesTerminals => m_Flow.FetchUploadTerminal != null && m_Flow.DecodeDownloadTerminal != null;

    /// <summary>
    /// Reacts to a successful instruction upload at the fetch terminal.
    /// </summary>
    public void NotifyInstructionUploaded(InstructionDefinition instruction)
    {
        if (!m_State.HasStarted || !UsesTerminals || !IsFetchStep(m_Flow.CurrentStep) || instruction == null)
            return;

        m_Flow.RaiseFeedback(GetTransportPrompt(), false);
        m_Flow.RaiseStepChanged();
    }

    /// <summary>
    /// Reacts to a successful instruction handoff into the decode terminal.
    /// </summary>
    public void NotifyInstructionModuleDownloaded(InstructionModule module)
    {
        if (!m_State.HasStarted || !UsesTerminals || !IsFetchStep(m_Flow.CurrentStep) || module == null)
            return;

        if (!HasDownloadedInstructionModule())
            return;

        m_Flow.RaiseFeedback("Instruction received at the decode terminal. Beginning Instruction Decode.", false);
        m_Flow.Advance();
    }

    /// <summary>
    /// Produces the learner-facing prompt that explains the current fetch-terminal state.
    /// </summary>
    public string GetTransportPrompt()
    {
        if (!UsesTerminals)
            return "Press Continue when you are ready.";

        if (HasDownloadedInstructionModule())
            return "Instruction received at the decode terminal. Press Continue to begin Instruction Decode.";

        return "The selected instruction has been uploaded to the fetch terminal. Carry the module to the decode terminal before continuing.";
    }

    /// <summary>
    /// Returns true once the decode terminal is holding the active instruction module.
    /// </summary>
    public bool HasDownloadedInstructionModule()
    {
        return m_Flow.DecodeDownloadTerminal != null &&
               m_Flow.ActiveInstruction != null &&
               m_Flow.DecodeDownloadTerminal.HasMatchingDownloadedInstruction(m_Flow.ActiveInstruction);
    }

    /// <summary>
    /// Resets both terminals and uploads the currently selected instruction into the fetch station.
    /// </summary>
    public void PrepareTerminals()
    {
        if (!UsesTerminals || m_Flow.ActiveInstruction == null)
            return;

        m_Flow.DecodeDownloadTerminal.ResetTerminal(false);
        m_Flow.FetchUploadTerminal.ResetTerminal(true);
        m_Flow.FetchUploadTerminal.UploadInstruction(m_Flow.ActiveInstruction);
    }

    /// <summary>
    /// Identifies whether a lesson step belongs to instruction fetch.
    /// </summary>
    public bool IsFetchStep(InstructionFlowStep step)
    {
        return step != null &&
               step.stepName.IndexOf("Fetch", StringComparison.OrdinalIgnoreCase) >= 0;
    }
}

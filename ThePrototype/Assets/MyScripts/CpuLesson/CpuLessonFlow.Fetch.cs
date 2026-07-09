using System;

public partial class CpuLessonFlow
{
    /// <summary>
    /// Lets the fetch terminal announce that a fresh lesson instruction is now
    /// loaded into the physical module.
    /// </summary>
    public void NotifyInstructionUploaded(InstructionDefinition instruction)
    {
        if (!HasStarted || !UsesInstructionTerminals || !IsFetchStep(CurrentStep) || instruction == null)
            return;

        SetFeedback(GetFetchTransportPrompt(), false);
        StepChanged?.Invoke(this);
    }

    /// <summary>
    /// Called by the decode terminal once the learner docks the carried module.
    /// This unlocks progression out of fetch into the decode phase.
    /// </summary>
    public void NotifyInstructionModuleDownloaded(InstructionModule module)
    {
        if (!HasStarted || !UsesInstructionTerminals || !IsFetchStep(CurrentStep) || module == null)
            return;

        if (!HasDownloadedInstructionModule())
            return;

        SetFeedback("Instruction received at the decode terminal. Beginning Instruction Decode.", false);
        AdvanceToNextStep();
    }

    string GetFetchTransportPrompt()
    {
        if (!UsesInstructionTerminals)
            return "Press Continue when you are ready.";

        if (HasDownloadedInstructionModule())
            return "Instruction received at the decode terminal. Press Continue to begin Instruction Decode.";

        return "The selected instruction has been uploaded to the fetch terminal. Carry the module to the decode terminal before continuing.";
    }

    bool HasDownloadedInstructionModule()
    {
        return m_DecodeDownloadTerminal != null &&
               m_CurrentInstruction != null &&
               m_DecodeDownloadTerminal.HasMatchingDownloadedInstruction(m_CurrentInstruction);
    }

    void PrepareInstructionFetchTerminals()
    {
        if (!UsesInstructionTerminals || m_CurrentInstruction == null)
            return;

        // Decode starts empty; fetch always respawns a fresh blank module so the
        // learner sees instruction fetch from a consistent authored baseline.
        m_DecodeDownloadTerminal.ResetTerminal(false);
        m_FetchUploadTerminal.ResetTerminal(true);
        m_FetchUploadTerminal.UploadInstruction(m_CurrentInstruction);
    }

    static bool IsFetchStep(InstructionFlowStep step)
    {
        return step != null &&
               step.stepName.IndexOf("Fetch", StringComparison.OrdinalIgnoreCase) >= 0;
    }
}

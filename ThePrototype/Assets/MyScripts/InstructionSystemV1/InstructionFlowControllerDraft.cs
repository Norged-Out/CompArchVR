using UnityEngine;

/// <summary>
/// First-pass draft for the instruction walkthrough controller.
/// 
/// Current purpose:
/// - hold the selected instruction
/// - remember which explanation step we are on
/// - coordinate with the existing CpuNodeSequenceController for quick prototyping
/// 
/// Future purpose:
/// - map each InstructionFlowStep to a specific scene node
/// - show explanation text in a panel
/// - validate whether the player selected valid registers/immediates
/// - handle reset/replay for a full instruction walkthrough
/// </summary>
public class InstructionFlowControllerDraft : MonoBehaviour
{
    [SerializeField]
    InstructionDefinition m_CurrentInstruction;

    [SerializeField]
    InstructionRuntimeSelection m_RuntimeSelection = new();

    // Temporary bridge to the node highlighter you already have.
    // Right now this can advance visuals in a rough sequential way while the
    // real instruction system is still being built.
    [SerializeField]
    CpuNodeSequenceController m_NodeSequenceController;

    int m_CurrentStepIndex = -1;

    public InstructionDefinition CurrentInstruction => m_CurrentInstruction;
    public int CurrentStepIndex => m_CurrentStepIndex;
    public InstructionRuntimeSelection RuntimeSelection => m_RuntimeSelection;

    /// <summary>
    /// Selects a new instruction and resets the walkthrough back to the start.
    /// </summary>
    public void LoadInstruction(InstructionDefinition instruction)
    {
        m_CurrentInstruction = instruction;
        m_RuntimeSelection.definition = instruction;
        m_CurrentStepIndex = -1;

        // The player may want to keep the same instruction but choose new operands later,
        // so we reset only the runtime inputs here.
        m_RuntimeSelection.ResetOperands();
    }

    /// <summary>
    /// Clears the active instruction flow and restores visuals to their default state.
    /// </summary>
    [ContextMenu("Reset Instruction Flow")]
    public void ResetInstructionFlow()
    {
        m_CurrentStepIndex = -1;

        if (m_NodeSequenceController != null)
            m_NodeSequenceController.ResetHighlight();
    }

    /// <summary>
    /// Steps forward in the currently selected instruction's walkthrough.
    /// 
    /// This is intentionally lightweight for now. The visual side still relies on the
    /// existing sequential node highlighter, but the data shape is already ready for a
    /// more exact "highlight the node named in the current step" implementation later.
    /// </summary>
    [ContextMenu("Advance Instruction Flow")]
    public void AdvanceInstructionFlow()
    {
        if (m_CurrentInstruction == null || m_CurrentInstruction.flowSteps == null)
            return;

        if (m_CurrentInstruction.flowSteps.Length == 0)
            return;

        if (m_CurrentStepIndex + 1 >= m_CurrentInstruction.flowSteps.Length)
            return;

        m_CurrentStepIndex++;

        var currentStep = m_CurrentInstruction.flowSteps[m_CurrentStepIndex];
        if (!currentStep.advanceVisualHighlight)
            return;

        // Temporary behavior:
        // just keep advancing the existing node sequence in order.
        //
        // Later we should replace this with an explicit mapping from
        // currentStep.highlightedNode -> actual scene object / renderer.
        if (m_NodeSequenceController != null)
            m_NodeSequenceController.AdvanceHighlight();
    }

    /// <summary>
    /// Returns the currently active flow step, if one exists.
    /// This will be handy later for explanation panels or progress UI.
    /// </summary>
    public bool TryGetCurrentStep(out InstructionFlowStep step)
    {
        step = null;

        if (m_CurrentInstruction == null || m_CurrentInstruction.flowSteps == null)
            return false;

        if (m_CurrentStepIndex < 0 || m_CurrentStepIndex >= m_CurrentInstruction.flowSteps.Length)
            return false;

        step = m_CurrentInstruction.flowSteps[m_CurrentStepIndex];
        return step != null;
    }
}

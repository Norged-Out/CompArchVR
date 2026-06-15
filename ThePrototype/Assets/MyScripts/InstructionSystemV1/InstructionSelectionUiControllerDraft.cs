using System;
using UnityEngine;

/// <summary>
/// First-pass draft for the instruction selection UI logic.
/// 
/// This script does not create the UI for you; it simply defines how the future
/// UI can react once an instruction is selected:
/// - load the selected instruction into the flow controller
/// - show only the fields that make sense for that instruction
/// 
/// Example:
/// - R-type "add" might show rs, rt, rd
/// - I-type "addi" might show rs, rt, immediate
/// </summary>
public class InstructionSelectionUiControllerDraft : MonoBehaviour
{
    [SerializeField]
    InstructionDefinition[] m_AvailableInstructions = Array.Empty<InstructionDefinition>();

    [SerializeField]
    InstructionFlowControllerDraft m_FlowController;

    [Header("Optional UI Field Roots")]

    // These are optional scene references for future UI panels, rows, or input groups.
    // Nothing breaks if they are not assigned yet.
    [SerializeField]
    GameObject m_OpcodeFieldRoot;

    [SerializeField]
    GameObject m_RsFieldRoot;

    [SerializeField]
    GameObject m_RtFieldRoot;

    [SerializeField]
    GameObject m_RdFieldRoot;

    [SerializeField]
    GameObject m_ShamtFieldRoot;

    [SerializeField]
    GameObject m_ImmediateFieldRoot;

    [SerializeField]
    GameObject m_FunctFieldRoot;

    [SerializeField]
    GameObject m_AddressFieldRoot;

    int m_CurrentInstructionIndex = -1;

    public int CurrentInstructionIndex => m_CurrentInstructionIndex;

    /// <summary>
    /// Selects an instruction by array index.
    /// This is a convenient bridge for a future dropdown where the chosen option
    /// can simply pass an integer index into this method.
    /// </summary>
    public void SelectInstructionByIndex(int instructionIndex)
    {
        if (instructionIndex < 0 || instructionIndex >= m_AvailableInstructions.Length)
            return;

        m_CurrentInstructionIndex = instructionIndex;

        var selectedInstruction = m_AvailableInstructions[instructionIndex];
        if (m_FlowController != null)
            m_FlowController.LoadInstruction(selectedInstruction);

        ApplyLayout(selectedInstruction != null ? selectedInstruction.uiLayout : null);
    }

    /// <summary>
    /// Re-applies the currently selected instruction layout.
    /// Handy if the UI gets rebuilt or references are assigned after selection.
    /// </summary>
    [ContextMenu("Refresh Current Layout")]
    public void RefreshCurrentLayout()
    {
        if (m_CurrentInstructionIndex < 0 || m_CurrentInstructionIndex >= m_AvailableInstructions.Length)
        {
            ApplyLayout(null);
            return;
        }

        ApplyLayout(m_AvailableInstructions[m_CurrentInstructionIndex].uiLayout);
    }

    void ApplyLayout(InstructionUiLayout layout)
    {
        // If nothing is selected yet, hide everything by default.
        if (layout == null)
        {
            SetFieldActive(m_OpcodeFieldRoot, false);
            SetFieldActive(m_RsFieldRoot, false);
            SetFieldActive(m_RtFieldRoot, false);
            SetFieldActive(m_RdFieldRoot, false);
            SetFieldActive(m_ShamtFieldRoot, false);
            SetFieldActive(m_ImmediateFieldRoot, false);
            SetFieldActive(m_FunctFieldRoot, false);
            SetFieldActive(m_AddressFieldRoot, false);
            return;
        }

        SetFieldActive(m_OpcodeFieldRoot, layout.showOpcode);
        SetFieldActive(m_RsFieldRoot, layout.showRs);
        SetFieldActive(m_RtFieldRoot, layout.showRt);
        SetFieldActive(m_RdFieldRoot, layout.showRd);
        SetFieldActive(m_ShamtFieldRoot, layout.showShamt);
        SetFieldActive(m_ImmediateFieldRoot, layout.showImmediate);
        SetFieldActive(m_FunctFieldRoot, layout.showFunct);
        SetFieldActive(m_AddressFieldRoot, layout.showAddress);
    }

    static void SetFieldActive(GameObject fieldRoot, bool isActive)
    {
        if (fieldRoot != null)
            fieldRoot.SetActive(isActive);
    }
}

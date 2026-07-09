using UnityEngine;

/// <summary>
/// Thin wrapper around the authored Execution panel and ALU controller.
/// </summary>
[DisallowMultipleComponent]
public sealed class ExecutePanelController : MonoBehaviour
{
    [SerializeField]
    AluExecutionController m_Controller;

    public AluExecutionController PhaseController => m_Controller;

    public void ApplyState(bool isActive, InstructionDefinition instruction)
    {
        gameObject.SetActive(isActive);

        m_Controller?.SetPhaseState(isActive, instruction);
    }

    public void Reset()
    {
        m_Controller?.ResetExecutionState();
        gameObject.SetActive(false);
    }
}

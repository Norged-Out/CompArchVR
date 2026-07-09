using UnityEngine;

/// <summary>
/// Thin wrapper around the authored Memory panel and memory unit controller.
/// </summary>
[DisallowMultipleComponent]
public sealed class MemoryPanelController : MonoBehaviour
{
    [SerializeField]
    MemoryUnitController m_Controller;

    public MemoryUnitController PhaseController => m_Controller;

    public void ApplyState(bool isActive, InstructionDefinition instruction)
    {
        gameObject.SetActive(isActive);

        m_Controller?.SetPhaseState(isActive, instruction);

        if (!isActive)
            m_Controller?.ResetMemoryState();
    }

    public void Reset()
    {
        m_Controller?.ResetMemoryState();
        gameObject.SetActive(false);
    }
}

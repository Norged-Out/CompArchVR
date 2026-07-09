using UnityEngine;

/// <summary>
/// Thin wrapper around the authored PC Update panel and controller.
/// </summary>
[DisallowMultipleComponent]
public sealed class PcUpdatePanelController : MonoBehaviour
{
    [SerializeField]
    PcUpdateController m_Controller;

    public PcUpdateController PhaseController => m_Controller;

    public void ApplyState(bool isActive, InstructionDefinition instruction)
    {
        gameObject.SetActive(isActive);

        m_Controller?.SetPhaseState(isActive, instruction);

        if (!isActive)
            m_Controller?.ResetPcUpdateState();
    }

    public void Reset()
    {
        m_Controller?.ResetPcUpdateState();
        gameObject.SetActive(false);
    }
}

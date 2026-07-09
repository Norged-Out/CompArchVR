using UnityEngine;

/// <summary>
/// Thin wrapper around the authored Write-Back panel and controller.
/// </summary>
[DisallowMultipleComponent]
public sealed class WriteBackPanelController : MonoBehaviour
{
    [SerializeField]
    WriteBackController m_Controller;

    public WriteBackController PhaseController => m_Controller;

    public void ApplyState(bool isActive, InstructionDefinition instruction, RegisterBank registerBank)
    {
        gameObject.SetActive(isActive);

        m_Controller?.SetPhaseState(isActive, instruction, registerBank);

        if (!isActive)
            m_Controller?.ResetWriteBackState();
    }

    public void Reset()
    {
        m_Controller?.ResetWriteBackState();
        gameObject.SetActive(false);
    }
}

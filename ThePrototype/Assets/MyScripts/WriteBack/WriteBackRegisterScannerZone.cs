using UnityEngine;

/// <summary>
/// Trigger forwarder for the write-back register input pedestal.
/// The zone itself stays scene-authored; this helper simply relays enter/exit
/// events to the owning scanner component.
/// </summary>
[DisallowMultipleComponent]
public class WriteBackRegisterScannerZone : MonoBehaviour
{
    WriteBackRegisterScanner m_OwningScanner;

    public void Bind(WriteBackRegisterScanner owningScanner)
    {
        m_OwningScanner = owningScanner;
    }

    void OnTriggerEnter(Collider other)
    {
        var registerToken = other.GetComponentInParent<RegisterToken>();
        if (registerToken != null)
            m_OwningScanner?.NotifyTokenEntered(registerToken);
    }

    void OnTriggerExit(Collider other)
    {
        var registerToken = other.GetComponentInParent<RegisterToken>();
        if (registerToken != null)
            m_OwningScanner?.NotifyTokenExited(registerToken);
    }
}

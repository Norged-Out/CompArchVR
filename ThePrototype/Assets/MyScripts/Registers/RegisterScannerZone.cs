using UnityEngine;

/// <summary>
/// Small trigger forwarder for a register scanner's scan zone.
/// Keeping this separate lets the scanner live on the pedestal root while the
/// trigger collider stays on a dedicated child object.
/// </summary>
[DisallowMultipleComponent]
public class RegisterScannerZone : MonoBehaviour
{
    RegisterScanner m_OwningScanner;

    public void Bind(RegisterScanner owningScanner)
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

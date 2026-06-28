using UnityEngine;

/// <summary>
/// Trigger forwarder for ALU input packet scanners.
/// </summary>
[DisallowMultipleComponent]
public class AluInputScannerZone : MonoBehaviour
{
    AluInputScanner m_OwningScanner;

    public void Bind(AluInputScanner owningScanner)
    {
        m_OwningScanner = owningScanner;
    }

    void OnTriggerEnter(Collider other)
    {
        var dataPacketToken = other.GetComponentInParent<DataPacketToken>();
        if (dataPacketToken != null)
            m_OwningScanner?.NotifyPacketEntered(dataPacketToken);
    }

    void OnTriggerExit(Collider other)
    {
        var dataPacketToken = other.GetComponentInParent<DataPacketToken>();
        if (dataPacketToken != null)
            m_OwningScanner?.NotifyPacketExited(dataPacketToken);
    }
}

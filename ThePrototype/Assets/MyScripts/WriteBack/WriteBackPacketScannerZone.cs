using UnityEngine;

/// <summary>
/// Trigger forwarder for the write-back packet input pedestal.
/// </summary>
[DisallowMultipleComponent]
public class WriteBackPacketScannerZone : MonoBehaviour
{
    WriteBackPacketScanner m_OwningScanner;

    public void Bind(WriteBackPacketScanner owningScanner)
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

using UnityEngine;

/// <summary>
/// Trigger helper for the Memory Unit's data input.
/// </summary>
[DisallowMultipleComponent]
public class MemoryPacketScannerZone : MonoBehaviour
{
    MemoryPacketScanner m_OwningScanner;

    public void Bind(MemoryPacketScanner owningScanner)
    {
        m_OwningScanner = owningScanner;
    }

    void OnTriggerEnter(Collider other)
    {
        if (m_OwningScanner == null || other == null)
            return;

        var packetToken = other.GetComponentInParent<DataPacketToken>();
        if (packetToken != null)
            m_OwningScanner.NotifyPacketEntered(packetToken);
    }

    void OnTriggerExit(Collider other)
    {
        if (m_OwningScanner == null || other == null)
            return;

        var packetToken = other.GetComponentInParent<DataPacketToken>();
        if (packetToken != null)
            m_OwningScanner.NotifyPacketExited(packetToken);
    }
}

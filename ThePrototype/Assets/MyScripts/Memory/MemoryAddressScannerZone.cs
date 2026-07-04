using UnityEngine;

/// <summary>
/// Trigger helper for the memory address input.
/// Keeping trigger forwarding in a tiny component avoids cluttering the main
/// scanner script with Unity message plumbing.
/// </summary>
[DisallowMultipleComponent]
public class MemoryAddressScannerZone : MonoBehaviour
{
    MemoryAddressScanner m_OwningScanner;

    public void Bind(MemoryAddressScanner owningScanner)
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

using UnityEngine;

/// <summary>
/// Trigger bridge for the Program Counter update station scanners.
/// </summary>
[DisallowMultipleComponent]
public class PcUpdatePacketScannerZone : MonoBehaviour
{
    PcUpdatePacketScanner m_Scanner;

    public void Bind(PcUpdatePacketScanner scanner)
    {
        m_Scanner = scanner;
    }

    void OnTriggerEnter(Collider other)
    {
        var dataPacketToken = other.GetComponentInParent<DataPacketToken>();
        if (dataPacketToken != null)
            m_Scanner?.NotifyPacketEntered(dataPacketToken);
    }

    void OnTriggerExit(Collider other)
    {
        var dataPacketToken = other.GetComponentInParent<DataPacketToken>();
        if (dataPacketToken != null)
            m_Scanner?.NotifyPacketExited(dataPacketToken);
    }
}

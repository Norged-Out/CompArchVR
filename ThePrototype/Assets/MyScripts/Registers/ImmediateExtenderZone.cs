using UnityEngine;

/// <summary>
/// Trigger relay for the authored immediate-extension station.
/// Keeping trigger callbacks in a tiny helper avoids stuffing raw physics
/// methods into the main scanner logic.
/// </summary>
[DisallowMultipleComponent]
public class ImmediateExtenderZone : MonoBehaviour
{
    ImmediateExtender m_OwningExtender;

    public void Bind(ImmediateExtender owningExtender)
    {
        m_OwningExtender = owningExtender;
    }

    void OnTriggerEnter(Collider other)
    {
        var packetToken = other.GetComponentInParent<DataPacketToken>();
        if (packetToken != null)
            m_OwningExtender?.NotifyPacketEntered(packetToken);
    }

    void OnTriggerExit(Collider other)
    {
        var packetToken = other.GetComponentInParent<DataPacketToken>();
        if (packetToken != null)
            m_OwningExtender?.NotifyPacketExited(packetToken);
    }
}

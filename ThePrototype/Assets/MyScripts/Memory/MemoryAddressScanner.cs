using System;
using UnityEngine;

/// <summary>
/// Memory-unit input that accepts the address packet produced by the ALU.
/// It watches a trigger zone, waits for a stable dropped packet, and latches
/// the accepted address in place for the current memory access.
/// </summary>
[DisallowMultipleComponent]
public class MemoryAddressScanner : MemoryPacketLatchScannerBase<MemoryAddressScannerZone, MemoryAddressScanner>
{
    /// <summary>
    /// Convenience accessor for the currently latched byte address.
    /// </summary>
    public int AcceptedAddress => AcceptedPacket != null ? AcceptedPacket.Value : 0;

    /// <summary>
    /// Raised once the address pedestal has successfully latched a packet.
    /// </summary>
    public event Action<MemoryAddressScanner, DataPacketToken> PacketAccepted;

    /// <summary>
    /// Forwards the accepted packet to whichever controller owns this scanner.
    /// </summary>
    protected override void RaisePacketAccepted(DataPacketToken packetToken)
    {
        PacketAccepted?.Invoke(this, packetToken);
    }
}

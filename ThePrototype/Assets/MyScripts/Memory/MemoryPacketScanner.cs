using System;
using UnityEngine;

/// <summary>
/// Optional second Memory Unit input used for instructions that must provide a
/// data value to memory, such as future store operations.
/// </summary>
[DisallowMultipleComponent]
public class MemoryPacketScanner : MemoryPacketLatchScannerBase<MemoryPacketScannerZone, MemoryPacketScanner>
{
    /// <summary>
    /// Raised once the store-data pedestal has successfully latched a packet.
    /// </summary>
    public event Action<MemoryPacketScanner, DataPacketToken> PacketAccepted;

    /// <summary>
    /// Forwards the accepted packet to whichever controller owns this scanner.
    /// </summary>
    protected override void RaisePacketAccepted(DataPacketToken packetToken)
    {
        PacketAccepted?.Invoke(this, packetToken);
    }
}

using UnityEngine;

/// <summary>
/// Trigger helper for the memory address input.
/// Keeping trigger forwarding in a tiny component avoids cluttering the main
/// scanner script with Unity message plumbing.
/// </summary>
[DisallowMultipleComponent]
public class MemoryAddressScannerZone : MemoryPacketScannerZoneBase<MemoryAddressScanner>
{
    protected override void HandleCandidateEntered(MemoryAddressScanner owningScanner, DataPacketToken candidate)
    {
        owningScanner.NotifyPacketEntered(candidate);
    }

    protected override void HandleCandidateExited(MemoryAddressScanner owningScanner, DataPacketToken candidate)
    {
        owningScanner.NotifyPacketExited(candidate);
    }
}

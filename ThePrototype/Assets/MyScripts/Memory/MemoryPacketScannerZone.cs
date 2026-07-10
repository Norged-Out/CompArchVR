using UnityEngine;

/// <summary>
/// Trigger helper for the Memory Unit's data input.
/// </summary>
[DisallowMultipleComponent]
public class MemoryPacketScannerZone : MemoryPacketScannerZoneBase<MemoryPacketScanner>
{
    protected override void HandleCandidateEntered(MemoryPacketScanner owningScanner, DataPacketToken candidate)
    {
        owningScanner.NotifyPacketEntered(candidate);
    }

    protected override void HandleCandidateExited(MemoryPacketScanner owningScanner, DataPacketToken candidate)
    {
        owningScanner.NotifyPacketExited(candidate);
    }
}

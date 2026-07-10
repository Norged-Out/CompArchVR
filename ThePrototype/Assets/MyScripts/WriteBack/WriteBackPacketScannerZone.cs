using UnityEngine;

/// <summary>
/// Shared-zone implementation for the write-back packet pedestal.
/// </summary>
[DisallowMultipleComponent]
public sealed class WriteBackPacketScannerZone
    : PedestalScannerZoneBase<WriteBackPacketScanner, DataPacketToken>
{
    protected override void HandleCandidateEntered(WriteBackPacketScanner owningScanner, DataPacketToken candidate)
    {
        owningScanner.NotifyPacketEntered(candidate);
    }

    protected override void HandleCandidateExited(WriteBackPacketScanner owningScanner, DataPacketToken candidate)
    {
        owningScanner.NotifyPacketExited(candidate);
    }
}

using UnityEngine;

/// <summary>
/// Shared-zone implementation for Program Counter update packet pedestals.
/// </summary>
[DisallowMultipleComponent]
public sealed class PcUpdatePacketScannerZone
    : PedestalScannerZoneBase<PcUpdatePacketScanner, DataPacketToken>
{
    protected override void HandleCandidateEntered(PcUpdatePacketScanner owningScanner, DataPacketToken candidate)
    {
        owningScanner.NotifyPacketEntered(candidate);
    }

    protected override void HandleCandidateExited(PcUpdatePacketScanner owningScanner, DataPacketToken candidate)
    {
        owningScanner.NotifyPacketExited(candidate);
    }
}

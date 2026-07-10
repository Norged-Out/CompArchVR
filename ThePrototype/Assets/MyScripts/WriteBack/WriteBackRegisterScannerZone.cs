using UnityEngine;

/// <summary>
/// Shared-zone implementation for the write-back register pedestal.
/// </summary>
[DisallowMultipleComponent]
public sealed class WriteBackRegisterScannerZone
    : PedestalScannerZoneBase<WriteBackRegisterScanner, RegisterToken>
{
    protected override void HandleCandidateEntered(WriteBackRegisterScanner owningScanner, RegisterToken candidate)
    {
        owningScanner.NotifyTokenEntered(candidate);
    }

    protected override void HandleCandidateExited(WriteBackRegisterScanner owningScanner, RegisterToken candidate)
    {
        owningScanner.NotifyTokenExited(candidate);
    }
}

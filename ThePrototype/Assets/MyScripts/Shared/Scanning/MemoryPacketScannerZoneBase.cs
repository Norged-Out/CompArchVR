using UnityEngine;

/// <summary>
/// Shared trigger forwarder for Memory Unit packet scanners.
/// Concrete zone helpers only need to forward the resolved packet to their
/// owning scanner, rather than duplicating trigger plumbing.
/// </summary>
/// <typeparam name="TScanner">
/// Concrete memory scanner type that owns this trigger zone.
/// </typeparam>
[DisallowMultipleComponent]
public abstract class MemoryPacketScannerZoneBase<TScanner> : PedestalScannerZoneBase<TScanner, DataPacketToken>
    where TScanner : class
{
}

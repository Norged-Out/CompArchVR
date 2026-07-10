using UnityEngine;

/// <summary>
/// Generic trigger forwarder for pedestal-style scanner zones.
/// Derived zone components only need to define how the candidate is forwarded
/// to the owning scanner, instead of re-implementing the same trigger code.
/// </summary>
/// <typeparam name="TScanner">
/// Concrete scanner type that owns this zone.
/// </typeparam>
/// <typeparam name="TCandidate">
/// Concrete component type that should be resolved from the entering collider.
/// </typeparam>
[DisallowMultipleComponent]
public abstract class PedestalScannerZoneBase<TScanner, TCandidate> : MonoBehaviour
    where TScanner : class
    where TCandidate : Component
{
    TScanner m_OwningScanner;

    /// <summary>
    /// Binds the authored scanner root to this child trigger zone.
    /// </summary>
    public void Bind(TScanner owningScanner)
    {
        m_OwningScanner = owningScanner;
    }

    /// <summary>
    /// Resolves the lesson object that should be forwarded to the scanner.
    /// Override this only when the default parent lookup is not enough.
    /// </summary>
    protected virtual TCandidate ResolveCandidate(Collider other)
    {
        return other != null ? other.GetComponentInParent<TCandidate>() : null;
    }

    void OnTriggerEnter(Collider other)
    {
        if (m_OwningScanner == null)
            return;

        var candidate = ResolveCandidate(other);
        if (candidate != null)
            HandleCandidateEntered(m_OwningScanner, candidate);
    }

    void OnTriggerExit(Collider other)
    {
        if (m_OwningScanner == null)
            return;

        var candidate = ResolveCandidate(other);
        if (candidate != null)
            HandleCandidateExited(m_OwningScanner, candidate);
    }

    /// <summary>
    /// Forwards a valid enter event to the owning scanner.
    /// </summary>
    protected abstract void HandleCandidateEntered(TScanner owningScanner, TCandidate candidate);

    /// <summary>
    /// Forwards a valid exit event to the owning scanner.
    /// </summary>
    protected abstract void HandleCandidateExited(TScanner owningScanner, TCandidate candidate);
}

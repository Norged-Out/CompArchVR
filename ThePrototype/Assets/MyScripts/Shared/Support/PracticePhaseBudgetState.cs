using System;
using System.Collections.Generic;

/// <summary>
/// Shared runtime budget tracker for Practice-mode phases.
/// Each phase owns one of these so validation attempts, scanner attempts,
/// and hints all follow the same failure rules without duplicating counters.
/// </summary>
public sealed class PracticePhaseBudgetState
{
    readonly HashSet<string> m_ConsumedScannerFailures = new(StringComparer.Ordinal);

    int m_MaxValidationAttempts = 1;
    int m_MaxScannerAttempts = 1;
    int m_MaxHints;

    public int RemainingValidationAttempts { get; private set; } = 1;
    public int RemainingScannerAttempts { get; private set; } = 1;
    public int RemainingHints { get; private set; }
    public bool IsFailed { get; private set; }

    /// <summary>
    /// Configures the authored budget caps for one phase.
    /// </summary>
    public void Configure(int maxValidationAttempts, int maxScannerAttempts, int maxHints)
    {
        m_MaxValidationAttempts = Math.Max(1, maxValidationAttempts);
        m_MaxScannerAttempts = Math.Max(1, maxScannerAttempts);
        m_MaxHints = Math.Max(0, maxHints);
        Reset();
    }

    /// <summary>
    /// Restores the phase to its full budget and clears any remembered
    /// scanner-failure keys from the previous attempt.
    /// </summary>
    public void Reset()
    {
        RemainingValidationAttempts = m_MaxValidationAttempts;
        RemainingScannerAttempts = m_MaxScannerAttempts;
        RemainingHints = m_MaxHints;
        IsFailed = false;
        m_ConsumedScannerFailures.Clear();
    }

    /// <summary>
    /// Consumes one validate-button attempt and reports whether that exhausted
    /// the phase budget.
    /// </summary>
    public bool ConsumeValidationAttempt()
    {
        if (IsFailed)
            return true;

        RemainingValidationAttempts = Math.Max(0, RemainingValidationAttempts - 1);
        if (RemainingValidationAttempts <= 0)
            IsFailed = true;

        return IsFailed;
    }

    /// <summary>
    /// Consumes one scanner attempt if this exact failure key has not already
    /// been charged during the current phase run.
    /// </summary>
    public bool ConsumeScannerAttempt(string failureKey, out bool didConsume)
    {
        didConsume = false;
        if (IsFailed || string.IsNullOrWhiteSpace(failureKey))
            return IsFailed;

        if (!m_ConsumedScannerFailures.Add(failureKey))
            return IsFailed;

        RemainingScannerAttempts = Math.Max(0, RemainingScannerAttempts - 1);
        didConsume = true;

        if (RemainingScannerAttempts <= 0)
            IsFailed = true;

        return IsFailed;
    }

    /// <summary>
    /// Consumes one hint use if any remain.
    /// </summary>
    public bool TryConsumeHint()
    {
        if (RemainingHints <= 0)
            return false;

        RemainingHints = Math.Max(0, RemainingHints - 1);
        return true;
    }
}

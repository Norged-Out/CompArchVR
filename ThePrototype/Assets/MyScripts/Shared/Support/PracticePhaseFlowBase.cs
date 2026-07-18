/// <summary>
/// Shared budget and failure-state logic for Practice-mode phase controllers.
/// Concrete phase flows only need to supply their own hint order and message
/// text, while validation/scanner budgets stay consistent across the project.
/// </summary>
public abstract class PracticePhaseFlowBase
{
    readonly PracticePhaseBudgetState m_Budget = new();

    public bool IsFailed => m_Budget.IsFailed;
    public bool IsAwaitingReset { get; private set; }
    public int RemainingValidationAttempts => m_Budget.RemainingValidationAttempts;
    public int RemainingScannerAttempts => m_Budget.RemainingScannerAttempts;
    public int RemainingHints => m_Budget.RemainingHints;

    public void Configure(int maxValidationAttempts, int maxScannerAttempts, int maxHints)
    {
        m_Budget.Configure(maxValidationAttempts, maxScannerAttempts, maxHints);
        Reset();
    }

    public virtual void Reset()
    {
        m_Budget.Reset();
        IsAwaitingReset = false;
    }

    protected bool ConsumeValidationFailure(string message, out string feedbackText)
    {
        var isFailed = m_Budget.ConsumeValidationAttempt();
        if (isFailed)
            IsAwaitingReset = true;

        feedbackText = BuildFeedbackText(message);
        return isFailed;
    }

    protected bool ConsumeScannerFailure(string failureKey, string message, out string feedbackText, out bool didConsume)
    {
        var isFailed = m_Budget.ConsumeScannerAttempt(failureKey, out didConsume);
        if (isFailed)
            IsAwaitingReset = true;

        feedbackText = BuildFeedbackText(message);
        return isFailed;
    }

    protected bool TryConsumeHint(out string noHintsFeedback)
    {
        if (m_Budget.TryConsumeHint())
        {
            noHintsFeedback = string.Empty;
            return true;
        }

        noHintsFeedback = BuildHintText("No hints remaining.");
        return false;
    }

    protected string BuildFeedbackText(string message)
    {
        return $"{message}\nChecks remaining: {RemainingValidationAttempts}\nScanner attempts remaining: {RemainingScannerAttempts}";
    }

    public string BuildBudgetSummary(string message)
    {
        return BuildFeedbackText(message);
    }

    public string BuildFailureResetText(string feedbackText)
    {
        return $"{feedbackText}\nPress Restart to reset the lesson.";
    }

    protected string BuildHintText(string message)
    {
        return $"{message}\nHints remaining: {RemainingHints}";
    }
}

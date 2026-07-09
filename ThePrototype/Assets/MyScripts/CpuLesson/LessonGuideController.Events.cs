using UnityEngine;

/// <summary>
/// Event bridges between authored phase controllers and the lesson flow.
/// </summary>
public partial class LessonGuideController
{
    void HandleInstructionChanged(int selectedIndex)
    {
        if (m_IsRefreshingInstructionDropdown)
            return;

        if (selectedIndex < 0 || selectedIndex >= m_AvailableInstructions.Count)
            return;

        m_LessonFlow?.SetCurrentInstruction(m_AvailableInstructions[selectedIndex]);
        PopulateDecodeDropdowns();
        RefreshView();
    }

    void HandleDecodeOpcodeChanged(int _)
    {
        if (m_IsRefreshingDecodeDropdowns)
            return;

        RefreshView();
    }

    void HandleDecodeFunctChanged(int _)
    {
        if (m_IsRefreshingDecodeDropdowns)
            return;

        RefreshView();
    }

    void HandleDecodeHintChanged(int _)
    {
        if (m_IsRefreshingDecodeDropdowns)
            return;

        RefreshDecodeHintText();
        RefreshLayout(m_IDRoot, m_IDOpcodeSelectionText, m_IDFeedback, m_IDActionButton);
    }

    void HandleStepChanged(CpuLessonFlow _)
    {
        Debug.Log($"{k_LogPrefix} StepChanged | step={m_LessonFlow?.CurrentStep?.stepName} frame={Time.frameCount}", this);
        RefreshView();
    }

    void HandleAluExecutionCompleted(int resultValue)
    {
        m_LessonFlow?.CompleteAluExecution(resultValue);
    }

    void HandleWriteBackApplied(string destinationRegister, int resultValue)
    {
        m_LessonFlow?.CompleteWriteBackExecution(destinationRegister, resultValue);
    }

    void HandleWriteBackContinueRequested()
    {
        m_LessonFlow?.Advance();
    }

    void HandleMemoryContinueRequested()
    {
        m_LessonFlow?.Advance();
    }

    void HandlePcUpdateContinueRequested()
    {
        m_LessonFlow?.ResetLesson();
    }

    void HandleFeedbackChanged(string message, bool isFailure)
    {
        var feedbackColor = isFailure
            ? new Color(1f, 0.55f, 0.55f, 1f)
            : new Color(0.78f, 0.96f, 0.82f, 1f);

        // Only the currently visible panel owns the live feedback surface.
        if (ShouldShowIDPanel())
        {
            if (m_IDFeedback != null)
            {
                m_IDFeedback.text = message;
                m_IDFeedback.color = feedbackColor;
                m_IDFeedback.gameObject.SetActive(!string.IsNullOrWhiteSpace(message));
            }

            RefreshDecodeHintText();
            RefreshLayout(m_IDRoot, m_IDOpcodeSelectionText, m_IDFeedback, m_IDActionButton);
            return;
        }

        if (ShouldShowMemoryPanel() || ShouldShowAluPanel() || ShouldShowPcUpdatePanel())
            return;

        if (m_IntroFeedback != null)
        {
            m_IntroFeedback.text = message;
            m_IntroFeedback.color = feedbackColor;
            m_IntroFeedback.gameObject.SetActive(!string.IsNullOrWhiteSpace(message));
        }

        RefreshLayout(m_IntroRoot, m_IntroBody, m_IntroFeedback, m_IntroActionButton);
    }
}

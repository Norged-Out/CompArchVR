using UnityEngine;
using System.Collections;

/// <summary>
/// Turns phase arrow groups on and off based on the current lesson phase.
/// Each active group is pulsed in sequence so the route reads like a path.
/// </summary>
[DisallowMultipleComponent]
public sealed class LessonGuidanceController : MonoBehaviour
{
    const string k_LogPrefix = "[LessonGuidanceController]";

    [SerializeField]
    CpuLessonFlow m_LessonFlow;

    [SerializeField]
    bool m_GuidanceEnabled = true;

    [SerializeField]
    GuidanceArrow[] m_PreStartArrows;

    [SerializeField]
    GuidanceArrow[] m_FetchArrows;

    [SerializeField]
    GuidanceArrow[] m_DecodeArrows;

    [SerializeField]
    GuidanceArrow[] m_ExecuteArrows;

    [SerializeField]
    GuidanceArrow[] m_MemoryArrows;

    [SerializeField]
    GuidanceArrow[] m_WriteBackArrows;

    [SerializeField]
    GuidanceArrow[] m_PcUpdateArrows;

    [SerializeField]
    float m_SequenceStepSeconds = 0.5f;

    readonly LessonPhaseRouter m_PhaseRouter = new();
    Coroutine m_DeferredRefreshRoutine;

    public bool GuidanceEnabled => m_GuidanceEnabled;

    void OnEnable()
    {
        if (m_LessonFlow != null)
            m_LessonFlow.StepChanged += HandleStepChanged;

        RefreshGuidance();
        ScheduleDeferredRefresh();
    }

    void Start()
    {
        RefreshGuidance();
        ScheduleDeferredRefresh();
    }

    void OnDisable()
    {
        if (m_LessonFlow != null)
            m_LessonFlow.StepChanged -= HandleStepChanged;

        if (m_DeferredRefreshRoutine != null)
        {
            StopCoroutine(m_DeferredRefreshRoutine);
            m_DeferredRefreshRoutine = null;
        }

        SetAllGroupsInactive();
    }

    void HandleStepChanged(CpuLessonFlow _)
    {
        RefreshGuidance();
        ScheduleDeferredRefresh();
    }

    /// <summary>
    /// Lets external UI toggle the route guidance on or off without disabling
    /// the whole component or losing the current lesson-phase subscription.
    /// </summary>
    public void SetGuidanceEnabled(bool isEnabled)
    {
        if (m_GuidanceEnabled == isEnabled)
            return;

        m_GuidanceEnabled = isEnabled;
        RefreshGuidance();
        ScheduleDeferredRefresh();
    }

    void ScheduleDeferredRefresh()
    {
        if (!isActiveAndEnabled)
            return;

        if (m_DeferredRefreshRoutine != null)
            StopCoroutine(m_DeferredRefreshRoutine);

        m_DeferredRefreshRoutine = StartCoroutine(RefreshGuidanceNextFrame());
    }

    IEnumerator RefreshGuidanceNextFrame()
    {
        yield return null;
        Debug.Log($"{k_LogPrefix} Deferred refresh | frame={Time.frameCount}", this);
        RefreshGuidance();
        m_DeferredRefreshRoutine = null;
    }

    void RefreshGuidance()
    {
        SetAllGroupsInactive();

        var activeGroup = "None";

        if (!m_GuidanceEnabled)
        {
            LogGuidanceState(activeGroup);
            return;
        }

        if (m_LessonFlow == null || !m_LessonFlow.HasStarted)
        {
            activeGroup = "PreStart";
            SetGroupActive(m_PreStartArrows, true);
            LogGuidanceState(activeGroup);
            return;
        }

        if (m_PhaseRouter.ShouldShowIntroPanel(m_LessonFlow))
        {
            activeGroup = "Fetch";
            SetGroupActive(m_FetchArrows, true);
            LogGuidanceState(activeGroup);
            return;
        }

        if (m_PhaseRouter.ShouldShowDecodePanel(m_LessonFlow))
        {
            activeGroup = "Decode";
            SetGroupActive(m_DecodeArrows, true);
            LogGuidanceState(activeGroup);
            return;
        }

        if (m_PhaseRouter.ShouldShowExecutionPanel(m_LessonFlow))
        {
            activeGroup = "Execute";
            SetGroupActive(m_ExecuteArrows, true);
            LogGuidanceState(activeGroup);
            return;
        }

        if (m_PhaseRouter.ShouldShowMemoryPanel(m_LessonFlow))
        {
            activeGroup = "Memory";
            SetGroupActive(m_MemoryArrows, true);
            LogGuidanceState(activeGroup);
            return;
        }

        if (m_PhaseRouter.ShouldShowWriteBackPanel(m_LessonFlow))
        {
            activeGroup = "WriteBack";
            SetGroupActive(m_WriteBackArrows, true);
            LogGuidanceState(activeGroup);
            return;
        }

        if (m_PhaseRouter.ShouldShowPcUpdatePanel(m_LessonFlow))
        {
            activeGroup = "PcUpdate";
            SetGroupActive(m_PcUpdateArrows, true);
        }

        LogGuidanceState(activeGroup);
    }

    void LogGuidanceState(string activeGroup)
    {
        var hasStarted = m_LessonFlow != null && m_LessonFlow.HasStarted;
        var stepName = "<none>";

        if (hasStarted)
        {
            try
            {
                var currentStep = m_LessonFlow.CurrentStep;
                if (currentStep != null)
                    stepName = currentStep.stepName;
            }
            catch
            {
                stepName = "<unavailable>";
            }
        }

        Debug.Log(
            $"{k_LogPrefix} Refresh | frame={Time.frameCount} enabled={m_GuidanceEnabled} hasStarted={hasStarted} step={stepName} activeGroup={activeGroup}",
            this);
    }

    void SetAllGroupsInactive()
    {
        // Only one route should read as active at a time, so clear every group
        // before enabling the one that matches the current lesson phase.
        SetGroupActive(m_PreStartArrows, false);
        SetGroupActive(m_FetchArrows, false);
        SetGroupActive(m_DecodeArrows, false);
        SetGroupActive(m_ExecuteArrows, false);
        SetGroupActive(m_MemoryArrows, false);
        SetGroupActive(m_WriteBackArrows, false);
        SetGroupActive(m_PcUpdateArrows, false);
    }

    void SetGroupActive(GuidanceArrow[] arrows, bool isActive)
    {
        if (arrows == null)
            return;

        for (var i = 0; i < arrows.Length; i++)
        {
            var arrow = arrows[i];
            if (arrow == null)
                continue;

            // Offset each arrow in the group so long routes feel like they are
            // guiding the learner forward instead of blinking in perfect sync.
            var offset = isActive ? i * m_SequenceStepSeconds : 0f;
            arrow.SetGuidanceActive(isActive, offset);
        }
    }
}

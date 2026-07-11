using UnityEngine;

/// <summary>
/// Toggles authored guidance arrow groups based on the lesson's current phase.
/// This v1 controller is intentionally route-focused: each phase can enable one
/// or more pre-placed arrow sets without introducing any object lookups.
/// </summary>
[DisallowMultipleComponent]
public sealed class LessonGuidanceController : MonoBehaviour
{
    [Header("Lesson Flow")]
    [SerializeField]
    CpuLessonFlow m_LessonFlow;

    [Header("Route Arrows")]
    [SerializeField]
    GuidanceArrow[] m_PreStartArrows;

    [SerializeField]
    GuidanceArrow[] m_FetchRouteArrows;

    [SerializeField]
    GuidanceArrow[] m_DecodeRouteArrows;

    [SerializeField]
    GuidanceArrow[] m_ExecuteRouteArrows;

    [SerializeField]
    GuidanceArrow[] m_MemoryRouteArrows;

    [SerializeField]
    GuidanceArrow[] m_WriteBackRouteArrows;

    [SerializeField]
    GuidanceArrow[] m_PcUpdateRouteArrows;

    [Header("Pulse Sequencing")]
    [SerializeField]
    float m_GroupSequenceStepSeconds = 0.18f;

    readonly LessonPhaseRouter m_PhaseRouter = new();

    /// <summary>
    /// Applies the correct route set before play begins.
    /// </summary>
    void Awake()
    {
        RefreshGuidance();
    }

    /// <summary>
    /// Subscribes to lesson changes whenever the controller becomes active.
    /// </summary>
    void OnEnable()
    {
        if (m_LessonFlow != null)
            m_LessonFlow.StepChanged += HandleStepChanged;

        RefreshGuidance();
    }

    /// <summary>
    /// Removes subscriptions to avoid duplicate callbacks after re-enable.
    /// </summary>
    void OnDisable()
    {
        if (m_LessonFlow != null)
            m_LessonFlow.StepChanged -= HandleStepChanged;
    }

    /// <summary>
    /// Re-evaluates which route arrows should be visible for the current phase.
    /// </summary>
    void HandleStepChanged(CpuLessonFlow _)
    {
        RefreshGuidance();
    }

    /// <summary>
    /// Computes the active phase and toggles the matching authored arrow group.
    /// </summary>
    void RefreshGuidance()
    {
        SetGroupActive(m_PreStartArrows, false);
        SetGroupActive(m_FetchRouteArrows, false);
        SetGroupActive(m_DecodeRouteArrows, false);
        SetGroupActive(m_ExecuteRouteArrows, false);
        SetGroupActive(m_MemoryRouteArrows, false);
        SetGroupActive(m_WriteBackRouteArrows, false);
        SetGroupActive(m_PcUpdateRouteArrows, false);

        if (m_LessonFlow == null || !m_LessonFlow.HasStarted)
        {
            SetGroupActive(m_PreStartArrows, true);
            return;
        }

        if (m_PhaseRouter.ShouldShowIntroPanel(m_LessonFlow))
        {
            SetGroupActive(m_FetchRouteArrows, true);
            return;
        }

        if (m_PhaseRouter.ShouldShowDecodePanel(m_LessonFlow))
        {
            SetGroupActive(m_DecodeRouteArrows, true);
            return;
        }

        if (m_PhaseRouter.ShouldShowExecutionPanel(m_LessonFlow))
        {
            SetGroupActive(m_ExecuteRouteArrows, true);
            return;
        }

        if (m_PhaseRouter.ShouldShowMemoryPanel(m_LessonFlow))
        {
            SetGroupActive(m_MemoryRouteArrows, true);
            return;
        }

        if (m_PhaseRouter.ShouldShowWriteBackPanel(m_LessonFlow))
        {
            SetGroupActive(m_WriteBackRouteArrows, true);
            return;
        }

        if (m_PhaseRouter.ShouldShowPcUpdatePanel(m_LessonFlow))
        {
            SetGroupActive(m_PcUpdateRouteArrows, true);
            return;
        }
    }

    /// <summary>
    /// Applies the active state to every arrow in the authored group and, when
    /// enabled, staggers the pulse start so the route reads like a moving path.
    /// </summary>
    void SetGroupActive(GuidanceArrow[] arrows, bool isActive)
    {
        if (arrows == null)
            return;

        for (var i = 0; i < arrows.Length; i++)
        {
            var arrow = arrows[i];
            if (arrow == null)
                continue;

            var timeOffsetSeconds = isActive ? (i * m_GroupSequenceStepSeconds) : 0f;
            arrow.SetGuidanceActive(isActive, timeOffsetSeconds);
        }
    }
}

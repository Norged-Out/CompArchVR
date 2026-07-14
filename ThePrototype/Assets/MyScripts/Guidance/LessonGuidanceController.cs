using UnityEngine;

/// <summary>
/// Turns phase arrow groups on and off based on the current lesson phase.
/// Each active group is pulsed in sequence so the route reads like a path.
/// </summary>
[DisallowMultipleComponent]
public sealed class LessonGuidanceController : MonoBehaviour
{
    [SerializeField]
    CpuLessonFlow m_LessonFlow;

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

    void OnEnable()
    {
        if (m_LessonFlow != null)
            m_LessonFlow.StepChanged += HandleStepChanged;

        RefreshGuidance();
    }

    void Start()
    {
        RefreshGuidance();
    }

    void OnDisable()
    {
        if (m_LessonFlow != null)
            m_LessonFlow.StepChanged -= HandleStepChanged;
    }

    void HandleStepChanged(CpuLessonFlow _)
    {
        RefreshGuidance();
    }

    void RefreshGuidance()
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

        if (m_LessonFlow == null || !m_LessonFlow.HasStarted)
        {
            SetGroupActive(m_PreStartArrows, true);
            return;
        }

        if (m_PhaseRouter.ShouldShowIntroPanel(m_LessonFlow))
        {
            SetGroupActive(m_FetchArrows, true);
            return;
        }

        if (m_PhaseRouter.ShouldShowDecodePanel(m_LessonFlow))
        {
            SetGroupActive(m_DecodeArrows, true);
            return;
        }

        if (m_PhaseRouter.ShouldShowExecutionPanel(m_LessonFlow))
        {
            SetGroupActive(m_ExecuteArrows, true);
            return;
        }

        if (m_PhaseRouter.ShouldShowMemoryPanel(m_LessonFlow))
        {
            SetGroupActive(m_MemoryArrows, true);
            return;
        }

        if (m_PhaseRouter.ShouldShowWriteBackPanel(m_LessonFlow))
        {
            SetGroupActive(m_WriteBackArrows, true);
            return;
        }

        if (m_PhaseRouter.ShouldShowPcUpdatePanel(m_LessonFlow))
            SetGroupActive(m_PcUpdateArrows, true);
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

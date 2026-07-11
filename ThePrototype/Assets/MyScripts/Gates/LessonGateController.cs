using UnityEngine;

/// <summary>
/// Synchronizes authored navigation gates with the current lesson phase.
/// Gates are fully open before a lesson starts, then progressively open as the
/// learner completes each stage of the walkthrough.
/// </summary>
[DisallowMultipleComponent]
public sealed class LessonGateController : MonoBehaviour
{
    [Header("Lesson State")]
    [SerializeField]
    CpuLessonFlow m_LessonFlow;

    [SerializeField]
    PcUpdateController m_PcUpdateController;

    [Header("Route Gates")]
    [SerializeField]
    LessonGate m_FetchGate;

    [SerializeField]
    LessonGate m_DecodeGate;

    [SerializeField]
    LessonGate m_ExecuteGate;

    [SerializeField]
    LessonGate m_WriteBackShortcutGate;

    [SerializeField]
    LessonGate m_PcUpdateGate;

    [SerializeField]
    LessonGate m_ExitGate;

    readonly LessonPhaseRouter m_PhaseRouter = new();
    bool m_EndGateUnlocked;

    /// <summary>
    /// Applies the authored default gate state before the first lesson starts.
    /// </summary>
    void Awake()
    {
        SetAllOpen();
    }

    /// <summary>
    /// Binds lesson and PC-update events whenever the gate manager becomes active.
    /// </summary>
    void OnEnable()
    {
        if (m_LessonFlow != null)
            m_LessonFlow.StepChanged += HandleStepChanged;

        if (m_PcUpdateController != null)
            m_PcUpdateController.PcUpdateConfirmed += HandlePcUpdateConfirmed;

        RefreshGateState();
    }

    /// <summary>
    /// Unbinds runtime events to avoid duplicate subscriptions after re-enable.
    /// </summary>
    void OnDisable()
    {
        if (m_LessonFlow != null)
            m_LessonFlow.StepChanged -= HandleStepChanged;

        if (m_PcUpdateController != null)
            m_PcUpdateController.PcUpdateConfirmed -= HandlePcUpdateConfirmed;
    }

    /// <summary>
    /// Re-evaluates the gate route whenever the lesson changes phase.
    /// </summary>
    void HandleStepChanged(CpuLessonFlow _)
    {
        if (m_LessonFlow == null || !m_LessonFlow.HasStarted)
            m_EndGateUnlocked = false;

        RefreshGateState();
    }

    /// <summary>
    /// Opens the final exit gate once the learner has confirmed the Program Counter update.
    /// </summary>
    void HandlePcUpdateConfirmed()
    {
        m_EndGateUnlocked = true;
        RefreshGateState();
    }

    /// <summary>
    /// Applies the current gate layout using the lesson's active phase.
    /// </summary>
    void RefreshGateState()
    {
        if (m_LessonFlow == null || !m_LessonFlow.HasStarted)
        {
            SetAllOpen();
            return;
        }

        var showIntro = m_PhaseRouter.ShouldShowIntroPanel(m_LessonFlow);
        var showExecution = m_PhaseRouter.ShouldShowExecutionPanel(m_LessonFlow);
        var showMemory = m_PhaseRouter.ShouldShowMemoryPanel(m_LessonFlow);
        var showWriteBack = m_PhaseRouter.ShouldShowWriteBackPanel(m_LessonFlow);
        var showPcUpdate = m_PhaseRouter.ShouldShowPcUpdatePanel(m_LessonFlow);

        var fetchOpen = showIntro;
        var decodeOpen = showExecution || showMemory || showWriteBack || showPcUpdate;
        var executeOpen = showMemory || showWriteBack || showPcUpdate;
        var writeBackShortcutOpen = showWriteBack || showPcUpdate;
        var pcUpdateOpen = showPcUpdate;
        var exitOpen = m_EndGateUnlocked;

        m_FetchGate?.SetOpen(fetchOpen);
        m_DecodeGate?.SetOpen(decodeOpen);
        m_ExecuteGate?.SetOpen(executeOpen);
        m_WriteBackShortcutGate?.SetOpen(writeBackShortcutOpen);
        m_PcUpdateGate?.SetOpen(pcUpdateOpen);
        m_ExitGate?.SetOpen(exitOpen);
    }

    /// <summary>
    /// Restores the free-exploration layout used before lesson start and after reset.
    /// </summary>
    void SetAllOpen()
    {
        m_FetchGate?.SetOpen(true);
        m_DecodeGate?.SetOpen(true);
        m_ExecuteGate?.SetOpen(true);
        m_WriteBackShortcutGate?.SetOpen(true);
        m_PcUpdateGate?.SetOpen(true);
        m_ExitGate?.SetOpen(true);
    }
}

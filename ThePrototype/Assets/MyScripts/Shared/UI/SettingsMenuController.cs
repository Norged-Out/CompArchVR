using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Lightweight bridge between the authored settings menu widgets and the
/// current lesson runtime state.
/// It mirrors the active instruction and routed phase into text fields, exposes
/// UI-friendly reset and quit methods, and keeps the volume slider synchronized
/// with Unity's global listener volume.
/// </summary>
[DisallowMultipleComponent]
public sealed class SettingsMenuController : MonoBehaviour
{
    const string k_NoneLabel = "None";
    const string k_InstructionPrefix = "Current Instruction: ";
    const string k_PhasePrefix = "Current Phase: ";
    const string k_FpsPrefix = "FPS: ";
    const string k_FrameTimeSuffix = " ms";

    [Header("Lesson State")]
    [SerializeField]
    CpuLessonFlow m_LessonFlow;

    [SerializeField]
    TMP_Text m_InstructionText;

    [SerializeField]
    TMP_Text m_PhaseText;

    [Header("Reset Actions")]
    [SerializeField]
    RegisterBankResetButton m_RegisterResetButton;

    [SerializeField]
    DataPacketResetButton m_DataPacketResetButton;

    [Header("Guidance")]
    [SerializeField]
    LessonGuidanceController m_LessonGuidanceController;

    [SerializeField]
    Toggle m_GuidanceToggle;

    [Header("Diagnostics")]
    [SerializeField]
    TMP_Text m_FpsText;

    [SerializeField]
    float m_FpsRefreshSeconds = 0.25f;

    [Header("Audio")]
    [SerializeField]
    Slider m_VolumeSlider;

    [SerializeField]
    TMP_Text m_VolumeValueText;

    readonly LessonPhaseRouter m_PhaseRouter = new();
    float m_FpsElapsedSeconds;
    int m_FpsFrameCount;

    void Awake()
    {
        RefreshLessonStateText();
        RefreshGuidanceToggleUi();
        RefreshVolumeUiFromCurrentValue();
    }

    void OnEnable()
    {
        BindLessonEvents();
        ConfigureVolumeSlider();
        RefreshLessonStateText();
        RefreshGuidanceToggleUi();
        RefreshVolumeUiFromCurrentValue();
    }

    void OnDisable()
    {
        UnbindLessonEvents();
    }

    void Update()
    {
        RefreshFpsText();
    }

    /// <summary>
    /// Lets a Unity UI button invoke the same register reset behavior that the
    /// authored physical button already uses.
    /// </summary>
    public void ResetRegisters()
    {
        m_RegisterResetButton?.TriggerReset();
    }

    /// <summary>
    /// Lets a Unity UI button invoke the same packet reset behavior that the
    /// authored physical button already uses.
    /// </summary>
    public void ResetDataPackets()
    {
        m_DataPacketResetButton?.TriggerReset();
    }

    /// <summary>
    /// Returns the lesson to its pre-start state while preserving the currently
    /// selected instruction asset, matching the fresh state that reopens all
    /// authored lesson gates.
    /// </summary>
    public void RestartLesson()
    {
        m_LessonFlow?.ResetLesson();
    }

    /// <summary>
    /// UI toggle callback for enabling or disabling the route-guidance arrows.
    /// </summary>
    public void SetGuidanceEnabled(bool isEnabled)
    {
        m_LessonGuidanceController?.SetGuidanceEnabled(isEnabled);
        RefreshGuidanceToggleUi();
    }

    /// <summary>
    /// UI slider callback. Expects a 0-100 whole-number value and maps it onto
    /// Unity's global 0-1 listener volume.
    /// </summary>
    public void SetVolumeFromSlider(float sliderValue)
    {
        var roundedVolume = Mathf.Clamp(Mathf.RoundToInt(sliderValue), 0, 100);
        AudioListener.volume = roundedVolume / 100f;
        ApplyVolumeUiValue(roundedVolume);
    }

    /// <summary>
    /// Stops play mode in the editor and quits the application in builds.
    /// </summary>
    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void HandleStepChanged(CpuLessonFlow _)
    {
        RefreshLessonStateText();
    }

    void BindLessonEvents()
    {
        if (m_LessonFlow == null)
            return;

        m_LessonFlow.StepChanged -= HandleStepChanged;
        m_LessonFlow.StepChanged += HandleStepChanged;
    }

    void UnbindLessonEvents()
    {
        if (m_LessonFlow == null)
            return;

        m_LessonFlow.StepChanged -= HandleStepChanged;
    }

    void ConfigureVolumeSlider()
    {
        if (m_VolumeSlider == null)
            return;

        m_VolumeSlider.wholeNumbers = true;
        m_VolumeSlider.minValue = 0f;
        m_VolumeSlider.maxValue = 100f;
    }

    void RefreshLessonStateText()
    {
        if (m_InstructionText != null)
            m_InstructionText.text = k_InstructionPrefix + ResolveInstructionLabel();

        if (m_PhaseText != null)
            m_PhaseText.text = k_PhasePrefix + ResolvePhaseLabel();
    }

    void RefreshVolumeUiFromCurrentValue()
    {
        var currentVolume = Mathf.Clamp(Mathf.RoundToInt(AudioListener.volume * 100f), 0, 100);
        ApplyVolumeUiValue(currentVolume);
    }

    void RefreshGuidanceToggleUi()
    {
        if (m_GuidanceToggle == null || m_LessonGuidanceController == null)
            return;

        m_GuidanceToggle.SetIsOnWithoutNotify(m_LessonGuidanceController.GuidanceEnabled);
    }

    void RefreshFpsText()
    {
        if (m_FpsText == null)
            return;

        m_FpsElapsedSeconds += Time.unscaledDeltaTime;
        m_FpsFrameCount++;

        var refreshSeconds = Mathf.Max(0.05f, m_FpsRefreshSeconds);
        if (m_FpsElapsedSeconds < refreshSeconds)
            return;

        var averageFrameSeconds = m_FpsElapsedSeconds / Mathf.Max(1, m_FpsFrameCount);
        var fps = Mathf.RoundToInt(1f / Mathf.Max(0.0001f, averageFrameSeconds));
        var frameTimeMs = averageFrameSeconds * 1000f;
        m_FpsText.text = $"{k_FpsPrefix}{fps} | {frameTimeMs:0.0}{k_FrameTimeSuffix}";
        m_FpsElapsedSeconds = 0f;
        m_FpsFrameCount = 0;
    }

    void ApplyVolumeUiValue(int volumeValue)
    {
        if (m_VolumeSlider != null)
            m_VolumeSlider.SetValueWithoutNotify(volumeValue);

        if (m_VolumeValueText != null)
            m_VolumeValueText.text = volumeValue.ToString();
    }

    string ResolveInstructionLabel()
    {
        if (m_LessonFlow == null || !m_LessonFlow.HasStarted || m_LessonFlow.CurrentInstruction == null)
            return k_NoneLabel;

        var assemblyText = m_LessonFlow.CurrentInstruction.assemblyInstructionText;
        return string.IsNullOrWhiteSpace(assemblyText) ? k_NoneLabel : assemblyText;
    }

    string ResolvePhaseLabel()
    {
        if (m_LessonFlow == null || !m_LessonFlow.HasStarted)
            return k_NoneLabel;

        // Keep the labels short and pipeline-like so they match the rest of
        // the project's teaching UI rather than exposing internal enum names.
        if (m_PhaseRouter.ShouldShowDecodePanel(m_LessonFlow))
            return "ID";

        if (m_PhaseRouter.ShouldShowExecutionPanel(m_LessonFlow))
            return "EX";

        if (m_PhaseRouter.ShouldShowMemoryPanel(m_LessonFlow))
            return "MEM";

        if (m_PhaseRouter.ShouldShowWriteBackPanel(m_LessonFlow))
            return "WB";

        if (m_PhaseRouter.ShouldShowPcUpdatePanel(m_LessonFlow))
            return "PC Update";

        if (m_PhaseRouter.ShouldShowIntroPanel(m_LessonFlow))
            return "IF";

        return k_NoneLabel;
    }
}

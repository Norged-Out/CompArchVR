using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Drives the authored lesson guide panels already placed in Testing Ground.
/// All scene references are assigned directly in the Inspector.
/// </summary>
[DisallowMultipleComponent]
public partial class LessonGuideController : MonoBehaviour
{
    const float k_ActionButtonHeight = 56f;
    const string k_LogPrefix = "[LessonGuideController]";

    [SerializeField]
    CpuLessonFlow m_LessonFlow;

    [SerializeField]
    string m_StartButtonLabel = "Start Lesson";

    [SerializeField]
    string m_ContinueButtonLabel = "Continue";

    [SerializeField]
    string m_RestartButtonLabel = "Restart";

    [Header("Intro UI")]
    [SerializeField]
    GameObject m_IntroRoot;

    [SerializeField]
    TMP_Text m_IntroBody;

    [SerializeField]
    TMP_Text m_IntroFeedback;

    [SerializeField]
    Button m_IntroActionButton;

    [SerializeField]
    TMP_Text m_IntroActionLabel;

    [SerializeField]
    TMP_Dropdown m_InstructionDropdown;

    [Header("Instruction Decode UI")]
    [SerializeField]
    GameObject m_IDRoot;

    [SerializeField]
    TMP_Text m_IDOpcodeLessonText;

    [SerializeField]
    TMP_Text m_IDRegisterLessonText;

    [SerializeField]
    TMP_Text m_IDFunctLessonText;

    [SerializeField]
    TMP_Text m_IDOpcodeBodyText;

    [SerializeField]
    TMP_Text m_IDRegisterBodyText;

    [SerializeField]
    TMP_Text m_IDFunctBodyText;

    [SerializeField]
    TMP_Text m_IDOpcodeSelectionText;

    [SerializeField]
    TMP_Text m_IDRegisterSelectionText;

    [SerializeField]
    TMP_Text m_IDFunctSelectionText;

    [SerializeField]
    TMP_Text m_IDFeedback;

    [SerializeField]
    Button m_IDActionButton;

    [SerializeField]
    TMP_Text m_IDActionLabel;

    [SerializeField]
    TMP_Dropdown m_IDOpcodeDropdown;

    [SerializeField]
    TMP_Dropdown m_IDFunctDropdown;

    [SerializeField]
    TMP_Dropdown m_IDHintDropdown;

    [SerializeField]
    TMP_Text m_IDHintText;

    [Header("ALU UI")]
    [SerializeField]
    GameObject m_AluRoot;

    [SerializeField]
    AluExecutionController m_AluController;

    [Header("Memory UI")]
    [SerializeField]
    GameObject m_MemRoot;

    [SerializeField]
    MemoryUnitController m_MemoryController;

    [Header("Write-Back UI")]
    [SerializeField]
    GameObject m_WriteBackRoot;

    [SerializeField]
    WriteBackController m_WriteBackController;

    [Header("PC Update UI")]
    [SerializeField]
    GameObject m_PcUpdateRoot;

    [SerializeField]
    PcUpdateController m_PcUpdateController;

    // Runtime caches mirror authored dropdown content so scene-authored UIs can
    // stay simple while still reacting to the currently selected instruction set.
    readonly List<InstructionDefinition> m_AvailableInstructions = new();
    readonly List<string> m_DecodeOpcodeOptions = new();
    readonly List<string> m_DecodeFunctOptions = new();
    bool m_IsRefreshingInstructionDropdown;
    bool m_IsRefreshingDecodeDropdowns;
    bool m_IsDecodeFunctStepActive;

    void Awake()
    {
        PopulateInstructionDropdown();
        PopulateDecodeDropdowns();
        HookButtons();
        HookDropdowns();
        EnsureButtonLayout(m_IntroActionButton);
        EnsureButtonLayout(m_IDActionButton);
        RefreshView();
    }

    void OnEnable()
    {
        PopulateInstructionDropdown();
        PopulateDecodeDropdowns();
        HookDropdowns();

        if (m_AluController != null)
            m_AluController.ExecutionCompleted += HandleAluExecutionCompleted;

        if (m_WriteBackController != null)
        {
            m_WriteBackController.WriteBackApplied += HandleWriteBackApplied;
            m_WriteBackController.ContinueRequested += HandleWriteBackContinueRequested;
        }

        if (m_MemoryController != null)
            m_MemoryController.ContinueRequested += HandleMemoryContinueRequested;

        if (m_PcUpdateController != null)
            m_PcUpdateController.ContinueRequested += HandlePcUpdateContinueRequested;

        if (m_LessonFlow == null)
            return;

        m_LessonFlow.StepChanged += HandleStepChanged;
        m_LessonFlow.FeedbackChanged += HandleFeedbackChanged;
        RefreshView();
    }

    void OnDisable()
    {
        if (m_AluController != null)
            m_AluController.ExecutionCompleted -= HandleAluExecutionCompleted;

        if (m_WriteBackController != null)
        {
            m_WriteBackController.WriteBackApplied -= HandleWriteBackApplied;
            m_WriteBackController.ContinueRequested -= HandleWriteBackContinueRequested;
        }

        if (m_MemoryController != null)
            m_MemoryController.ContinueRequested -= HandleMemoryContinueRequested;

        if (m_PcUpdateController != null)
            m_PcUpdateController.ContinueRequested -= HandlePcUpdateContinueRequested;

        if (m_LessonFlow == null)
            return;

        m_LessonFlow.StepChanged -= HandleStepChanged;
        m_LessonFlow.FeedbackChanged -= HandleFeedbackChanged;
    }

    void HookButtons()
    {
        if (m_IntroActionButton != null)
        {
            m_IntroActionButton.onClick.RemoveAllListeners();
            m_IntroActionButton.onClick.AddListener(HandleIntroActionPressed);
        }

        if (m_IDActionButton != null)
        {
            m_IDActionButton.onClick.RemoveAllListeners();
            m_IDActionButton.onClick.AddListener(HandleIDActionPressed);
        }

    }

    void HookDropdowns()
    {
        if (m_InstructionDropdown != null)
        {
            m_InstructionDropdown.onValueChanged.RemoveListener(HandleInstructionChanged);
            m_InstructionDropdown.onValueChanged.AddListener(HandleInstructionChanged);
        }

        if (m_IDOpcodeDropdown != null)
        {
            m_IDOpcodeDropdown.onValueChanged.RemoveListener(HandleDecodeOpcodeChanged);
            m_IDOpcodeDropdown.onValueChanged.AddListener(HandleDecodeOpcodeChanged);
        }

        if (m_IDFunctDropdown != null)
        {
            m_IDFunctDropdown.onValueChanged.RemoveListener(HandleDecodeFunctChanged);
            m_IDFunctDropdown.onValueChanged.AddListener(HandleDecodeFunctChanged);
        }

        if (m_IDHintDropdown != null)
        {
            m_IDHintDropdown.onValueChanged.RemoveListener(HandleDecodeHintChanged);
            m_IDHintDropdown.onValueChanged.AddListener(HandleDecodeHintChanged);
        }
    }

    void HandleIntroActionPressed()
    {
        if (m_LessonFlow == null)
            return;

        Debug.Log($"{k_LogPrefix} Intro button pressed | hasStarted={m_LessonFlow.HasStarted} step={m_LessonFlow.CurrentStep?.stepName} frame={Time.frameCount}", this);

        if (!m_LessonFlow.HasStarted)
            m_LessonFlow.StartLesson();
        else
            m_LessonFlow.Advance();
    }

    void HandleIDActionPressed()
    {
        if (m_LessonFlow == null)
            return;

        Debug.Log($"{k_LogPrefix} ID button pressed | hasStarted={m_LessonFlow.HasStarted} step={m_LessonFlow.CurrentStep?.stepName} frame={Time.frameCount}", this);

        if (!m_LessonFlow.HasStarted)
            m_LessonFlow.StartLesson();
        else if (IsDecodeOpcodeSelectionStep())
            HandleDecodeOpcodeContinue();
        else if (IsDecodeFunctSelectionStep())
            HandleDecodeFunctContinue();
        else
            m_LessonFlow.Advance();
    }

}

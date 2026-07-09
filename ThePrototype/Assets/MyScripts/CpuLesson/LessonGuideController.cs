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

        if (ShouldShowMemoryPanel())
        {
            return;
        }

        if (ShouldShowAluPanel())
            return;

        if (ShouldShowPcUpdatePanel())
            return;

        if (m_IntroFeedback != null)
        {
            m_IntroFeedback.text = message;
            m_IntroFeedback.color = feedbackColor;
            m_IntroFeedback.gameObject.SetActive(!string.IsNullOrWhiteSpace(message));
        }

        RefreshLayout(m_IntroRoot, m_IntroBody, m_IntroFeedback, m_IntroActionButton);
    }

    void RefreshView()
    {
        if (m_LessonFlow == null || m_IntroRoot == null)
            return;

        var showIDPanel = ShouldShowIDPanel();
        var showAluPanel = ShouldShowAluPanel();
        var showMemoryPanel = ShouldShowMemoryPanel();
        var showWriteBackPanel = ShouldShowWriteBackPanel();
        var showPcUpdatePanel = ShouldShowPcUpdatePanel();

        Debug.Log(
            $"{k_LogPrefix} RefreshView | step={m_LessonFlow.CurrentStep?.stepName} decode={showIDPanel} alu={showAluPanel} mem={showMemoryPanel} wb={showWriteBackPanel} pc={showPcUpdatePanel} frame={Time.frameCount}",
            this);

        // Panels are authored in the scene and simply toggled on/off as the
        // lesson advances. That keeps layout work in edit mode instead of runtime.
        if (m_IDRoot != null)
            m_IDRoot.SetActive(showIDPanel);

        if (m_AluRoot != null)
            m_AluRoot.SetActive(showAluPanel);

        if (m_MemRoot != null)
            m_MemRoot.SetActive(showMemoryPanel);

        if (m_WriteBackRoot != null)
            m_WriteBackRoot.SetActive(showWriteBackPanel);

        if (m_PcUpdateRoot != null)
            m_PcUpdateRoot.SetActive(showPcUpdatePanel);

        m_AluController?.SetPhaseState(showAluPanel, m_LessonFlow.CurrentInstruction);
        m_MemoryController?.SetPhaseState(showMemoryPanel, m_LessonFlow.CurrentInstruction);
        m_WriteBackController?.SetPhaseState(showWriteBackPanel, m_LessonFlow.CurrentInstruction, m_LessonFlow.RegisterBank);
        m_PcUpdateController?.SetPhaseState(showPcUpdatePanel, m_LessonFlow.CurrentInstruction);
        if (m_MemoryController != null && !showMemoryPanel)
            m_MemoryController.ResetMemoryState();
        if (m_WriteBackController != null && !showWriteBackPanel)
            m_WriteBackController.ResetWriteBackState();
        if (m_PcUpdateController != null && !showPcUpdatePanel)
            m_PcUpdateController.ResetPcUpdateState();

        m_IntroRoot.SetActive(!showIDPanel && !showAluPanel && !showMemoryPanel && !showWriteBackPanel && !showPcUpdatePanel);

        if (!m_LessonFlow.HasStarted)
        {
            m_AluController?.ResetExecutionState();
            m_MemoryController?.ResetMemoryState();
            m_WriteBackController?.ResetWriteBackState();
            if (m_IDRoot != null)
                m_IDRoot.SetActive(false);
            if (m_AluRoot != null)
                m_AluRoot.SetActive(false);
            if (m_MemRoot != null)
                m_MemRoot.SetActive(false);
            if (m_WriteBackRoot != null)
                m_WriteBackRoot.SetActive(false);
            if (m_PcUpdateRoot != null)
                m_PcUpdateRoot.SetActive(false);
            SetText(
                m_IntroBody,
                $"Lesson Introduction\n\nSelected instruction: {m_LessonFlow.CurrentInstruction?.assemblyInstructionText ?? "add t2, t0, t1"}\n\nPress Start Lesson to begin.");
            SetText(m_IntroFeedback, string.Empty);
            SetButtonState(m_IntroActionButton, m_IntroActionLabel, m_StartButtonLabel, true);
            if (m_InstructionDropdown != null)
                m_InstructionDropdown.interactable = true;
            ResetDecodeDropdowns();
            SetButtonState(m_IDActionButton, m_IDActionLabel, m_ContinueButtonLabel, false);
            RefreshLayout(m_IntroRoot, m_IntroBody, m_IntroFeedback, m_IntroActionButton);
            RefreshLayout(m_IDRoot, m_IDOpcodeSelectionText, m_IDFeedback, m_IDActionButton);
            return;
        }

        var step = m_LessonFlow.CurrentStep;
        if (step == null)
            return;

        if (m_InstructionDropdown != null)
            m_InstructionDropdown.interactable = false;

        if (showAluPanel)
        {
            SetButtonState(m_IntroActionButton, m_IntroActionLabel, m_ContinueButtonLabel, false);
            SetButtonState(m_IDActionButton, m_IDActionLabel, m_ContinueButtonLabel, false);
            RefreshLayout(m_IntroRoot, m_IntroBody, m_IntroFeedback, m_IntroActionButton);
            RefreshLayout(m_IDRoot, m_IDOpcodeSelectionText, m_IDFeedback, m_IDActionButton);
            return;
        }

        if (showWriteBackPanel)
        {
            SetButtonState(m_IntroActionButton, m_IntroActionLabel, m_ContinueButtonLabel, false);
            SetButtonState(m_IDActionButton, m_IDActionLabel, m_ContinueButtonLabel, false);
            RefreshLayout(m_IntroRoot, m_IntroBody, m_IntroFeedback, m_IntroActionButton);
            RefreshLayout(m_IDRoot, m_IDOpcodeSelectionText, m_IDFeedback, m_IDActionButton);
            return;
        }

        if (showPcUpdatePanel)
        {
            SetButtonState(m_IntroActionButton, m_IntroActionLabel, m_ContinueButtonLabel, false);
            SetButtonState(m_IDActionButton, m_IDActionLabel, m_ContinueButtonLabel, false);
            RefreshLayout(m_IntroRoot, m_IntroBody, m_IntroFeedback, m_IntroActionButton);
            RefreshLayout(m_IDRoot, m_IDOpcodeSelectionText, m_IDFeedback, m_IDActionButton);
            return;
        }

        if (showMemoryPanel)
        {
            SetButtonState(m_IntroActionButton, m_IntroActionLabel, m_ContinueButtonLabel, false);
            SetButtonState(m_IDActionButton, m_IDActionLabel, m_ContinueButtonLabel, false);
            RefreshLayout(m_IntroRoot, m_IntroBody, m_IntroFeedback, m_IntroActionButton);
            RefreshLayout(m_IDRoot, m_IDOpcodeSelectionText, m_IDFeedback, m_IDActionButton);
            return;
        }

        if (!showIDPanel)
        {
            SetText(m_IntroBody, BuildIntroBody(step));
            SetText(m_IntroFeedback, string.Empty);
            SetButtonState(
                m_IntroActionButton,
                m_IntroActionLabel,
                step.requiredInteraction == InstructionStepInteractionType.Completion ? m_RestartButtonLabel : m_ContinueButtonLabel,
                step.requiredInteraction == InstructionStepInteractionType.ContinueButton ||
                step.requiredInteraction == InstructionStepInteractionType.Completion);
            SetButtonState(m_IDActionButton, m_IDActionLabel, m_ContinueButtonLabel, false);
            RefreshLayout(m_IntroRoot, m_IntroBody, m_IntroFeedback, m_IntroActionButton);
            RefreshLayout(m_IDRoot, m_IDOpcodeSelectionText, m_IDFeedback, m_IDActionButton);
            return;
        }

        RefreshDecodeTextBlocks(step);
        RefreshDecodeDropdownState(step);
        RefreshDecodeHintText();

        var showContinue = IsDecodeOpcodeSelectionStep() ||
                           IsDecodeFunctSelectionStep() ||
                           step.requiredInteraction == InstructionStepInteractionType.ContinueButton ||
                           step.requiredInteraction == InstructionStepInteractionType.Completion ||
                           (step.requiredInteraction == InstructionStepInteractionType.RegisterSelection &&
                            m_LessonFlow.RegisterSelectionReadyToContinue);
        SetButtonState(
            m_IDActionButton,
            m_IDActionLabel,
            step.requiredInteraction == InstructionStepInteractionType.Completion ? m_RestartButtonLabel : m_ContinueButtonLabel,
            showContinue);
        SetButtonState(m_IntroActionButton, m_IntroActionLabel, m_ContinueButtonLabel, false);
        RefreshLayout(m_IntroRoot, m_IntroBody, m_IntroFeedback, m_IntroActionButton);
        RefreshLayout(m_IDRoot, m_IDOpcodeSelectionText, m_IDFeedback, m_IDActionButton);
    }

    bool ShouldShowIDPanel()
    {
        if (ShouldShowAluPanel())
            return false;

        var step = m_LessonFlow?.CurrentStep;
        if (step == null)
            return false;

        return step.highlightedNode == DatapathNodeId.InstructionMemory ||
               step.requiredInteraction == InstructionStepInteractionType.RegisterSelection;
    }

    bool ShouldShowAluPanel()
    {
        var step = m_LessonFlow?.CurrentStep;
        if (step == null)
            return false;

        return step.requiredInteraction == InstructionStepInteractionType.AluExecution;
    }

    bool ShouldShowMemoryPanel()
    {
        if (ShouldShowAluPanel() || ShouldShowWriteBackPanel())
            return false;

        var step = m_LessonFlow?.CurrentStep;
        if (step == null)
            return false;

        return step.highlightedNode == DatapathNodeId.DataMemory;
    }

    bool ShouldShowWriteBackPanel()
    {
        var step = m_LessonFlow?.CurrentStep;
        if (step == null)
            return false;

        return step.requiredInteraction == InstructionStepInteractionType.WriteBackExecution;
    }

    bool ShouldShowPcUpdatePanel()
    {
        var step = m_LessonFlow?.CurrentStep;
        if (step == null)
            return false;

        return step.requiredInteraction == InstructionStepInteractionType.PcUpdateExecution;
    }

    void PopulateInstructionDropdown()
    {
        if (m_LessonFlow == null)
            return;

        m_AvailableInstructions.Clear();
        var loadedInstructions = Resources.LoadAll<InstructionDefinition>("InstructionDefinitions");
        if (loadedInstructions != null && loadedInstructions.Length > 0)
        {
            m_AvailableInstructions.AddRange(loadedInstructions);
            m_AvailableInstructions.Sort((left, right) =>
                string.Compare(left != null ? left.displayName : string.Empty,
                    right != null ? right.displayName : string.Empty,
                    System.StringComparison.OrdinalIgnoreCase));
        }

        if (m_AvailableInstructions.Count == 0 && m_LessonFlow.CurrentInstruction != null)
            m_AvailableInstructions.Add(m_LessonFlow.CurrentInstruction);

        if (m_InstructionDropdown == null)
            return;

        m_IsRefreshingInstructionDropdown = true;
        m_InstructionDropdown.ClearOptions();

        var optionLabels = new List<string>();
        foreach (var instruction in m_AvailableInstructions)
            optionLabels.Add(instruction != null ? instruction.displayName : "Instruction");

        if (optionLabels.Count > 0)
            m_InstructionDropdown.AddOptions(optionLabels);

        var currentIndex = 0;
        for (var index = 0; index < m_AvailableInstructions.Count; index++)
        {
            if (m_AvailableInstructions[index] == m_LessonFlow.CurrentInstruction)
            {
                currentIndex = index;
                break;
            }
        }

        if (m_InstructionDropdown.options.Count > 0)
            m_InstructionDropdown.SetValueWithoutNotify(currentIndex);

        m_IsRefreshingInstructionDropdown = false;
    }

}

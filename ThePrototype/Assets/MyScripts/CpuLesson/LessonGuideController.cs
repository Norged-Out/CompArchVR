using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Drives the authored lesson guide panels already placed in Testing Ground.
/// All scene references are assigned directly in the Inspector.
/// </summary>
[DisallowMultipleComponent]
public class LessonGuideController : MonoBehaviour
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
        CacheReferences();
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
        CacheReferences();
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
        CacheReferences();

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

    string BuildIntroBody(InstructionFlowStep step)
    {
        var instruction = m_LessonFlow.CurrentInstruction;
        if (step.stepName.IndexOf("Fetch", System.StringComparison.OrdinalIgnoreCase) >= 0)
        {
            if (m_LessonFlow.UsesInstructionTerminals)
            {
                var transportStatus = m_LessonFlow.IsInstructionReadyForDecode
                    ? "The module is docked at the decode terminal. Instruction Decode is unlocking now."
                    : "The selected instruction has been uploaded to the fetch terminal. Pick up the module, carry it to the decode terminal, and dock it there to unlock Instruction Decode.";

                return
                    $"Instruction fetch uses the Program Counter to locate the next instruction in memory.\n\n" +
                    $"Instruction: {instruction.displayName}\n" +
                    $"Assembly: {instruction.assemblyInstructionText}\n\n" +
                    $"{transportStatus}";
            }

            return
                $"Instruction fetch uses the Program Counter to locate the next instruction in memory.\n\n" +
                $"Instruction: {instruction.displayName}\n" +
                $"Assembly: {instruction.assemblyInstructionText}\n\n" +
                $"{step.explanation}\n\n" +
                "When you are ready, continue into instruction decode.";
        }

        return
            $"Instruction: {instruction.displayName}\n" +
            $"Assembly: {instruction.assemblyInstructionText}\n\n" +
            $"{step.explanation}\n\n" +
            $"Next: {GetNextStageLabel(step)}.";
    }

    string BuildDecodeOpcodeSelectionText(InstructionFlowStep step)
    {
        var instruction = m_LessonFlow.CurrentInstruction;
        if (instruction == null)
            return string.Empty;

        if (IsDecodeOpcodeSelectionStep())
        {
            return $"Assembly: {instruction.assemblyInstructionText}";
        }

        return string.Empty;
    }

    string BuildDecodeRegisterSelectionText(InstructionFlowStep step)
    {
        var instruction = m_LessonFlow.CurrentInstruction;
        if (instruction == null)
            return string.Empty;

        if (step.requiredInteraction == InstructionStepInteractionType.RegisterSelection)
        {
            var lines = new List<string>();
            var requiredRoles = LessonChecks.GetRequiredRoles(instruction, step);

            for (var index = 0; index < requiredRoles.Length; index++)
            {
                var role = requiredRoles[index];
                var registerName = instruction.GetExpectedRegisterName(role);
                var scannerName = GetScannerLabel(role);
                var status = index < m_LessonFlow.CurrentRegisterSelectionIndex ? "done" : "pending";
                lines.Add($"{scannerName}: {registerName} [{status}]");
            }

            if (instruction.usesImmediate)
            {
                var immediateStatus = m_LessonFlow.RegisterSelectionReadyToContinue ? "ready to generate" : "locked";
                lines.Add($"Immediate packet: {instruction.expectedImmediateValue} [{immediateStatus}]");
            }

            var nextAction = m_LessonFlow.RegisterSelectionReadyToContinue
                ? instruction.usesImmediate
                    ? "Press Continue to generate the immediate packet and proceed to Execution."
                    : "Press Continue to proceed to Execution."
                : $"Current target: {GetCurrentDecodeTargetLabel(instruction, step)}.";

            return $"{string.Join("\n", lines)}\n\n{nextAction}";
        }

        return step.explanation;
    }

    string BuildDecodeFunctSelectionText(InstructionFlowStep step)
    {
        var instruction = m_LessonFlow.CurrentInstruction;
        if (instruction == null)
            return string.Empty;

        if (IsDecodeFunctSelectionStep())
            return $"Assembly: {instruction.assemblyInstructionText}";

        return string.Empty;
    }

    void RefreshDecodeTextBlocks(InstructionFlowStep step)
    {
        var isOpcodeStep = IsDecodeOpcodeSelectionStep();
        var isFunctStep = IsDecodeFunctSelectionStep();
        var isRegisterStep = step != null && step.requiredInteraction == InstructionStepInteractionType.RegisterSelection;

        // Decode is now split into authored text blocks so the user can move
        // more wording into edit mode while runtime only swaps the pieces that
        // genuinely depend on the active instruction or learner progress.
        SetActive(m_IDOpcodeLessonText, isOpcodeStep);
        SetActive(m_IDFunctLessonText, isFunctStep);
        SetActive(m_IDRegisterLessonText, isRegisterStep);
        SetActive(m_IDOpcodeBodyText, isOpcodeStep);
        SetActive(m_IDFunctBodyText, isFunctStep);
        SetActive(m_IDRegisterBodyText, isRegisterStep);
        SetActive(m_IDOpcodeSelectionText, isOpcodeStep);
        SetActive(m_IDFunctSelectionText, isFunctStep);
        SetActive(m_IDRegisterSelectionText, isRegisterStep);

        SetText(m_IDOpcodeSelectionText, isOpcodeStep ? BuildDecodeOpcodeSelectionText(step) : string.Empty);
        SetText(m_IDFunctSelectionText, isFunctStep ? BuildDecodeFunctSelectionText(step) : string.Empty);
        SetText(m_IDRegisterSelectionText, isRegisterStep ? BuildDecodeRegisterSelectionText(step) : string.Empty);
    }

    string GetNextStageLabel(InstructionFlowStep currentStep)
    {
        var instruction = m_LessonFlow != null ? m_LessonFlow.CurrentInstruction : null;
        if (currentStep == null || instruction == null)
            return "Continue";

        if (currentStep.requiredInteraction == InstructionStepInteractionType.Completion)
            return "Restart";

        if (currentStep.requiredInteraction == InstructionStepInteractionType.RegisterSelection ||
            currentStep.highlightedNode == DatapathNodeId.InstructionMemory)
        {
            return currentStep.highlightedNode == DatapathNodeId.InstructionMemory ? "Register Setup" : "Execution";
        }

        if (currentStep.requiredInteraction == InstructionStepInteractionType.AluExecution)
        {
            return instruction.UsesInteractiveMemoryPhase() ? "Memory Access" :
                instruction.UsesWriteBackPhase() ? "Write Back" : "Recap";
        }

        if (currentStep.highlightedNode == DatapathNodeId.DataMemory)
            return instruction.UsesWriteBackPhase() ? "Write Back" : "Recap";

        if (currentStep.requiredInteraction == InstructionStepInteractionType.WriteBackExecution)
            return "Program Counter Update";

        if (currentStep.requiredInteraction == InstructionStepInteractionType.PcUpdateExecution)
            return "Restart";

        return "Continue";
    }

    void CacheReferences()
    {
        m_AluController ??= FindFirstSceneObject<AluExecutionController>();
        m_MemoryController ??= FindFirstSceneObject<MemoryUnitController>();
        m_WriteBackController ??= FindFirstSceneObject<WriteBackController>();
        m_PcUpdateController ??= FindFirstSceneObject<PcUpdateController>();

        if (m_AluRoot == null)
        {
            var aluRootTransform = FindSceneTransform("ALU UI");
            m_AluRoot = aluRootTransform != null ? aluRootTransform.gameObject : null;
        }

        if (m_MemRoot == null)
        {
            var memRootTransform = FindSceneTransform("Mem UI");
            m_MemRoot = memRootTransform != null ? memRootTransform.gameObject : null;
        }

        if (m_WriteBackRoot == null)
        {
            var writeBackRootTransform = FindSceneTransform("WB UI");
            m_WriteBackRoot = writeBackRootTransform != null ? writeBackRootTransform.gameObject : null;
        }

        if (m_PcUpdateRoot == null)
        {
            var pcUpdateRootTransform = FindSceneTransform("PC Update UI");
            m_PcUpdateRoot = pcUpdateRootTransform != null ? pcUpdateRootTransform.gameObject : null;
        }

        // These scene searches are only fallback glue for resilience. The
        // preferred workflow is still explicit inspector assignment.
        if (m_InstructionDropdown == null && m_IntroRoot != null)
            m_InstructionDropdown = m_IntroRoot.GetComponentInChildren<TMP_Dropdown>(true);

        if (m_IDRoot != null)
        {
            m_IDOpcodeLessonText ??= FindNamedText(m_IDRoot.transform, "Opcode lesson");
            m_IDFunctLessonText ??= FindNamedText(m_IDRoot.transform, "Funct lesson");
            m_IDRegisterLessonText ??= FindNamedText(m_IDRoot.transform, "Register lesson");
            m_IDOpcodeBodyText ??= FindNamedText(m_IDRoot.transform, "Opcode body");
            m_IDFunctBodyText ??= FindNamedText(m_IDRoot.transform, "Funct body");
            m_IDRegisterBodyText ??= FindNamedText(m_IDRoot.transform, "Register body");
            m_IDOpcodeSelectionText ??= FindNamedText(m_IDRoot.transform, "Opcode selection");
            m_IDFunctSelectionText ??= FindNamedText(m_IDRoot.transform, "Funct selection");
            m_IDRegisterSelectionText ??= FindNamedText(m_IDRoot.transform, "Register selection");
        }
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

    void PopulateDecodeDropdowns()
    {
        PopulateDecodeOpcodeDropdown();
        PopulateDecodeFunctDropdown();
        PopulateDecodeHintDropdown();
    }

    void PopulateDecodeOpcodeDropdown()
    {
        if (m_IDOpcodeDropdown == null)
            return;

        m_DecodeOpcodeOptions.Clear();

        var optionLabels = new List<string> { "Choose Opcode" };
        foreach (var instruction in m_AvailableInstructions)
        {
            if (instruction == null || string.IsNullOrWhiteSpace(instruction.opcodeBits))
                continue;

            var opcode = instruction.opcodeBits.Trim();
            if (m_DecodeOpcodeOptions.Contains(opcode))
                continue;

            m_DecodeOpcodeOptions.Add(opcode);
            optionLabels.Add(opcode);
        }

        if (m_LessonFlow != null &&
            m_LessonFlow.CurrentInstruction != null &&
            !string.IsNullOrWhiteSpace(m_LessonFlow.CurrentInstruction.opcodeBits))
        {
            var currentOpcode = m_LessonFlow.CurrentInstruction.opcodeBits.Trim();
            if (!m_DecodeOpcodeOptions.Contains(currentOpcode))
            {
                m_DecodeOpcodeOptions.Add(currentOpcode);
                optionLabels.Add(currentOpcode);
            }
        }

        m_IsRefreshingDecodeDropdowns = true;
        m_IDOpcodeDropdown.ClearOptions();
        m_IDOpcodeDropdown.AddOptions(optionLabels);
        m_IDOpcodeDropdown.SetValueWithoutNotify(0);
        m_IsRefreshingDecodeDropdowns = false;
    }

    void PopulateDecodeHintDropdown()
    {
        if (m_IDHintDropdown == null)
            return;

        m_IsRefreshingDecodeDropdowns = true;
        m_IDHintDropdown.ClearOptions();
        m_IDHintDropdown.AddOptions(new List<string> { "Choose Option", "Opcode", "Funct" });
        m_IDHintDropdown.SetValueWithoutNotify(0);
        m_IsRefreshingDecodeDropdowns = false;
    }

    void PopulateDecodeFunctDropdown()
    {
        if (m_IDFunctDropdown == null)
            return;

        m_DecodeFunctOptions.Clear();

        var optionLabels = new List<string> { "Choose Funct" };
        foreach (var instruction in m_AvailableInstructions)
        {
            if (instruction == null || string.IsNullOrWhiteSpace(instruction.functBits))
                continue;

            var funct = instruction.functBits.Trim();
            if (m_DecodeFunctOptions.Contains(funct))
                continue;

            m_DecodeFunctOptions.Add(funct);
            optionLabels.Add(funct);
        }

        if (m_LessonFlow != null &&
            m_LessonFlow.CurrentInstruction != null &&
            !string.IsNullOrWhiteSpace(m_LessonFlow.CurrentInstruction.functBits))
        {
            var currentFunct = m_LessonFlow.CurrentInstruction.functBits.Trim();
            if (!m_DecodeFunctOptions.Contains(currentFunct))
            {
                m_DecodeFunctOptions.Add(currentFunct);
                optionLabels.Add(currentFunct);
            }
        }

        m_IsRefreshingDecodeDropdowns = true;
        m_IDFunctDropdown.ClearOptions();
        m_IDFunctDropdown.AddOptions(optionLabels);
        m_IDFunctDropdown.SetValueWithoutNotify(0);
        m_IsRefreshingDecodeDropdowns = false;
    }

    void ResetDecodeDropdowns()
    {
        m_IsRefreshingDecodeDropdowns = true;

        if (m_IDOpcodeDropdown != null)
            m_IDOpcodeDropdown.SetValueWithoutNotify(0);

        if (m_IDFunctDropdown != null)
            m_IDFunctDropdown.SetValueWithoutNotify(0);

        if (m_IDHintDropdown != null)
            m_IDHintDropdown.SetValueWithoutNotify(0);

        m_IsRefreshingDecodeDropdowns = false;
        m_IsDecodeFunctStepActive = false;
        SetText(m_IDHintText, string.Empty);
    }

    void RefreshDecodeDropdownState(InstructionFlowStep step)
    {
        var showOpcodeDropdown = IsDecodeOpcodeSelectionStep();
        var showFunctDropdown = IsDecodeFunctSelectionStep();

        if (m_IDOpcodeDropdown != null)
        {
            m_IDOpcodeDropdown.gameObject.SetActive(showOpcodeDropdown);
            m_IDOpcodeDropdown.interactable = showOpcodeDropdown;
        }

        if (m_IDFunctDropdown != null)
        {
            m_IDFunctDropdown.gameObject.SetActive(showFunctDropdown);
            m_IDFunctDropdown.interactable = showFunctDropdown;
        }

        if (m_IDHintDropdown != null)
            m_IDHintDropdown.gameObject.SetActive(true);

        if (m_IDHintText != null)
            m_IDHintText.gameObject.SetActive(m_IDHintDropdown != null && m_IDHintDropdown.value > 0);
    }

    void RefreshDecodeHintText()
    {
        if (m_IDHintDropdown == null)
            return;

        // Hint panels intentionally stay reference-oriented. They should reveal
        // lookup/help text only when the learner explicitly asks for it.
        string hintText;
        switch (m_IDHintDropdown.value)
        {
            case 1:
                hintText = BuildOpcodeHintText();
                break;
            case 2:
                hintText = BuildFunctHintText();
                break;
            default:
                hintText = string.Empty;
                break;
        }

        SetText(m_IDHintText, hintText);
    }

    string BuildOpcodeHintText()
    {
        var lines = new List<string>();
        foreach (var instruction in m_AvailableInstructions)
        {
            if (instruction == null || string.IsNullOrWhiteSpace(instruction.opcodeBits))
                continue;

            var line = $"{instruction.displayName} -> {instruction.opcodeBits.Trim()}";
            if (!lines.Contains(line))
                lines.Add(line);
        }

        return lines.Count == 0
            ? "No opcode reference available."
            : "Opcode reference\n\n" + string.Join("\n", lines);
    }

    string BuildFunctHintText()
    {
        var lines = new List<string>();
        foreach (var instruction in m_AvailableInstructions)
        {
            if (instruction == null || string.IsNullOrWhiteSpace(instruction.functBits))
                continue;

            var line = $"{instruction.displayName} -> {instruction.functBits.Trim()}";
            if (!lines.Contains(line))
                lines.Add(line);
        }

        return lines.Count == 0
            ? "No funct reference available."
            : "Funct reference\n\n" + string.Join("\n", lines);
    }

    static void SetText(TMP_Text target, string text)
    {
        if (target == null)
            return;

        target.text = text;
        target.gameObject.SetActive(!string.IsNullOrWhiteSpace(text));
    }

    static void SetActive(TMP_Text target, bool isActive)
    {
        if (target == null)
            return;

        target.gameObject.SetActive(isActive);
    }

    static void SetButtonState(Button button, TMP_Text label, string labelText, bool visibleAndEnabled)
    {
        if (button == null)
            return;

        button.gameObject.SetActive(visibleAndEnabled);
        button.interactable = visibleAndEnabled;

        if (label != null)
            label.text = labelText;
    }

    static void EnsureButtonLayout(Button button)
    {
        if (button == null)
            return;

        var layoutElement = button.GetComponent<LayoutElement>();
        if (layoutElement == null)
            layoutElement = button.gameObject.AddComponent<LayoutElement>();

        if (layoutElement.preferredHeight <= 0f)
            layoutElement.preferredHeight = k_ActionButtonHeight;

        if (layoutElement.minHeight <= 0f)
            layoutElement.minHeight = k_ActionButtonHeight;
    }

    static void RefreshLayout(GameObject root, TMP_Text body, TMP_Text feedback, Button actionButton)
    {
        if (root == null || !root.activeInHierarchy)
            return;

        foreach (var textMesh in root.GetComponentsInChildren<TMP_Text>(true))
            textMesh?.ForceMeshUpdate();

        EnsureButtonLayout(actionButton);

        Canvas.ForceUpdateCanvases();

        // The guide uses scroll-view content that grows with variable text.
        // Force-rebuilding both content and viewport keeps long descriptions
        // from overlapping the action button during play mode.
        var scrollRect = root.GetComponentInChildren<ScrollRect>(true);
        if (scrollRect != null && scrollRect.content != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content);

            if (scrollRect.viewport != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.viewport);
        }

        var rootRect = root.GetComponent<RectTransform>();
        if (rootRect != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(rootRect);

        Canvas.ForceUpdateCanvases();
    }

    void HandleDecodeOpcodeContinue()
    {
        if (m_LessonFlow == null || m_LessonFlow.CurrentInstruction == null)
            return;

        var selectedOpcode = GetSelectedDecodeOpcode();
        if (string.IsNullOrWhiteSpace(selectedOpcode))
        {
            HandleFeedbackChanged("Select an opcode first.", true);
            return;
        }

        var expectedOpcode = m_LessonFlow.CurrentInstruction.opcodeBits != null
            ? m_LessonFlow.CurrentInstruction.opcodeBits.Trim()
            : string.Empty;

        if (!string.Equals(selectedOpcode, expectedOpcode, System.StringComparison.Ordinal))
        {
            HandleFeedbackChanged("That opcode does not match the selected instruction.", true);
            return;
        }

        if (InstructionUsesDecodeFunct(m_LessonFlow.CurrentInstruction))
        {
            m_IsDecodeFunctStepActive = true;
            if (m_IDFunctDropdown != null)
                m_IDFunctDropdown.SetValueWithoutNotify(0);

            HandleFeedbackChanged("Opcode confirmed. Now identify the funct field.", false);
            RefreshView();
            return;
        }

        HandleFeedbackChanged("Opcode confirmed. Continue into operand setup.", false);
        m_LessonFlow.Advance();
    }

    string GetSelectedDecodeOpcode()
    {
        if (m_IDOpcodeDropdown == null ||
            m_IDOpcodeDropdown.options == null ||
            m_IDOpcodeDropdown.value <= 0 ||
            m_IDOpcodeDropdown.value >= m_IDOpcodeDropdown.options.Count)
        {
            return string.Empty;
        }

        return m_IDOpcodeDropdown.options[m_IDOpcodeDropdown.value].text.Trim();
    }

    void HandleDecodeFunctContinue()
    {
        if (m_LessonFlow == null || m_LessonFlow.CurrentInstruction == null)
            return;

        var selectedFunct = GetSelectedDecodeFunct();
        if (string.IsNullOrWhiteSpace(selectedFunct))
        {
            HandleFeedbackChanged("Select a funct value first.", true);
            return;
        }

        var expectedFunct = m_LessonFlow.CurrentInstruction.functBits != null
            ? m_LessonFlow.CurrentInstruction.functBits.Trim()
            : string.Empty;

        if (!string.Equals(selectedFunct, expectedFunct, System.StringComparison.Ordinal))
        {
            HandleFeedbackChanged("That funct value does not match the selected instruction.", true);
            return;
        }

        m_IsDecodeFunctStepActive = false;
        HandleFeedbackChanged("Funct confirmed. Continue into operand setup.", false);
        m_LessonFlow.Advance();
    }

    string GetSelectedDecodeFunct()
    {
        if (m_IDFunctDropdown == null ||
            m_IDFunctDropdown.options == null ||
            m_IDFunctDropdown.value <= 0 ||
            m_IDFunctDropdown.value >= m_IDFunctDropdown.options.Count)
        {
            return string.Empty;
        }

        return m_IDFunctDropdown.options[m_IDFunctDropdown.value].text.Trim();
    }

    bool IsDecodeOpcodeSelectionStep()
    {
        var step = m_LessonFlow != null ? m_LessonFlow.CurrentStep : null;
        return step != null &&
               step.highlightedNode == DatapathNodeId.InstructionMemory &&
               !m_IsDecodeFunctStepActive;
    }

    bool IsDecodeFunctSelectionStep()
    {
        var step = m_LessonFlow != null ? m_LessonFlow.CurrentStep : null;
        return step != null &&
               step.highlightedNode == DatapathNodeId.InstructionMemory &&
               m_IsDecodeFunctStepActive;
    }

    static bool InstructionUsesDecodeFunct(InstructionDefinition instruction)
    {
        return instruction != null &&
               !string.IsNullOrWhiteSpace(instruction.functBits) &&
               string.Equals(instruction.opcodeBits != null ? instruction.opcodeBits.Trim() : string.Empty, "000000", System.StringComparison.Ordinal);
    }

    string GetCurrentDecodeTargetLabel(InstructionDefinition instruction, InstructionFlowStep step)
    {
        var requiredRoles = LessonChecks.GetRequiredRoles(instruction, step);
        var currentIndex = m_LessonFlow != null ? m_LessonFlow.CurrentRegisterSelectionIndex : 0;
        if (currentIndex < 0 || currentIndex >= requiredRoles.Length)
            return "Place the required register";

        var role = requiredRoles[currentIndex];
        return $"{instruction.GetExpectedRegisterName(role)} on {GetScannerLabel(role)}";
    }

    static string GetScannerLabel(InstructionRegisterRole registerRole)
    {
        return registerRole switch
        {
            InstructionRegisterRole.Rs => "Read Register 1",
            InstructionRegisterRole.Rt => "Read Register 2",
            InstructionRegisterRole.Rd => "Write Register",
            _ => "the correct scanner",
        };
    }

    static Transform FindSceneTransform(string objectName)
    {
        foreach (var sceneTransform in Resources.FindObjectsOfTypeAll<Transform>())
        {
            if (sceneTransform == null || sceneTransform.name != objectName)
                continue;

            if (!sceneTransform.gameObject.scene.IsValid() || !sceneTransform.gameObject.scene.isLoaded)
                continue;

            return sceneTransform;
        }

        return null;
    }

    static T FindFirstSceneObject<T>() where T : Component
    {
        foreach (var component in Resources.FindObjectsOfTypeAll<T>())
        {
            if (component == null)
                continue;

            if (!component.gameObject.scene.IsValid() || !component.gameObject.scene.isLoaded)
                continue;

            return component;
        }

        return null;
    }

    static TMP_Text FindNamedText(Transform root, string objectName)
    {
        if (root == null)
            return null;

        foreach (var textMesh in root.GetComponentsInChildren<TMP_Text>(true))
        {
            if (textMesh != null && textMesh.name == objectName)
                return textMesh;
        }

        return null;
    }
}

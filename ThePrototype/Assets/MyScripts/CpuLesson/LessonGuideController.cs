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

    [Header("Register Setup UI")]
    [SerializeField]
    GameObject m_RegisterRoot;

    [SerializeField]
    TMP_Text m_RegisterBody;

    [SerializeField]
    TMP_Text m_RegisterFeedback;

    [SerializeField]
    Button m_RegisterActionButton;

    [SerializeField]
    TMP_Text m_RegisterActionLabel;

    [Header("ALU UI")]
    [SerializeField]
    GameObject m_AluRoot;

    [SerializeField]
    AluExecutionController m_AluController;

    [Header("Memory UI")]
    [SerializeField]
    GameObject m_MemRoot;

    [SerializeField]
    TMP_Text m_MemBody;

    [SerializeField]
    TMP_Text m_MemFeedback;

    [SerializeField]
    Button m_MemActionButton;

    [SerializeField]
    TMP_Text m_MemActionLabel;

    [Header("Write-Back UI")]
    [SerializeField]
    GameObject m_WriteBackRoot;

    [SerializeField]
    WriteBackController m_WriteBackController;

    readonly List<InstructionDefinition> m_AvailableInstructions = new();
    bool m_IsRefreshingInstructionDropdown;

    void Awake()
    {
        CacheReferences();
        PopulateInstructionDropdown();
        HookButtons();
        HookDropdowns();
        EnsureButtonLayout(m_IntroActionButton);
        EnsureButtonLayout(m_RegisterActionButton);
        EnsureButtonLayout(m_MemActionButton);
        RefreshView();
    }

    void OnEnable()
    {
        CacheReferences();
        PopulateInstructionDropdown();
        HookDropdowns();

        if (m_AluController != null)
            m_AluController.ExecutionCompleted += HandleAluExecutionCompleted;

        if (m_WriteBackController != null)
        {
            m_WriteBackController.WriteBackApplied += HandleWriteBackApplied;
            m_WriteBackController.ContinueRequested += HandleWriteBackContinueRequested;
        }

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

        if (m_RegisterActionButton != null)
        {
            m_RegisterActionButton.onClick.RemoveAllListeners();
            m_RegisterActionButton.onClick.AddListener(HandleRegisterActionPressed);
        }

        if (m_MemActionButton != null)
        {
            m_MemActionButton.onClick.RemoveAllListeners();
            m_MemActionButton.onClick.AddListener(HandleMemActionPressed);
        }
    }

    void HookDropdowns()
    {
        if (m_InstructionDropdown == null)
            return;

        m_InstructionDropdown.onValueChanged.RemoveListener(HandleInstructionChanged);
        m_InstructionDropdown.onValueChanged.AddListener(HandleInstructionChanged);
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

    void HandleRegisterActionPressed()
    {
        if (m_LessonFlow == null)
            return;

        Debug.Log($"{k_LogPrefix} Register button pressed | hasStarted={m_LessonFlow.HasStarted} step={m_LessonFlow.CurrentStep?.stepName} frame={Time.frameCount}", this);

        if (!m_LessonFlow.HasStarted)
            m_LessonFlow.StartLesson();
        else
            m_LessonFlow.Advance();
    }

    void HandleMemActionPressed()
    {
        if (m_LessonFlow == null)
            return;

        Debug.Log($"{k_LogPrefix} Mem button pressed | hasStarted={m_LessonFlow.HasStarted} step={m_LessonFlow.CurrentStep?.stepName} frame={Time.frameCount}", this);

        if (!m_LessonFlow.HasStarted)
            m_LessonFlow.StartLesson();
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
        RefreshView();
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

    void HandleFeedbackChanged(string message, bool isFailure)
    {
        var feedbackColor = isFailure
            ? new Color(1f, 0.55f, 0.55f, 1f)
            : new Color(0.78f, 0.96f, 0.82f, 1f);

        // Only the currently visible panel owns the live feedback surface.
        if (ShouldShowRegisterPanel())
        {
            if (m_RegisterFeedback != null)
            {
                m_RegisterFeedback.text = message;
                m_RegisterFeedback.color = feedbackColor;
                m_RegisterFeedback.gameObject.SetActive(!string.IsNullOrWhiteSpace(message));
            }

            RefreshLayout(m_RegisterRoot, m_RegisterBody, m_RegisterFeedback, m_RegisterActionButton);
            return;
        }

        if (ShouldShowMemoryPanel())
        {
            if (m_MemFeedback != null)
            {
                m_MemFeedback.text = message;
                m_MemFeedback.color = feedbackColor;
                m_MemFeedback.gameObject.SetActive(!string.IsNullOrWhiteSpace(message));
            }

            RefreshLayout(m_MemRoot, m_MemBody, m_MemFeedback, m_MemActionButton);
            return;
        }

        if (ShouldShowAluPanel())
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

        var showRegisterPanel = ShouldShowRegisterPanel();
        var showAluPanel = ShouldShowAluPanel();
        var showMemoryPanel = ShouldShowMemoryPanel();
        var showWriteBackPanel = ShouldShowWriteBackPanel();

        Debug.Log(
            $"{k_LogPrefix} RefreshView | step={m_LessonFlow.CurrentStep?.stepName} register={showRegisterPanel} alu={showAluPanel} mem={showMemoryPanel} wb={showWriteBackPanel} frame={Time.frameCount}",
            this);

        // Panels are authored in the scene and simply toggled on/off as the
        // lesson advances. That keeps layout work in edit mode instead of runtime.
        if (m_RegisterRoot != null)
            m_RegisterRoot.SetActive(showRegisterPanel);

        if (m_AluRoot != null)
            m_AluRoot.SetActive(showAluPanel);

        if (m_MemRoot != null)
            m_MemRoot.SetActive(showMemoryPanel);

        if (m_WriteBackRoot != null)
            m_WriteBackRoot.SetActive(showWriteBackPanel);

        m_AluController?.SetPhaseState(showAluPanel, m_LessonFlow.CurrentInstruction);
        m_WriteBackController?.SetPhaseState(showWriteBackPanel, m_LessonFlow.CurrentInstruction, m_LessonFlow.RegisterBank);
        if (m_WriteBackController != null && !showWriteBackPanel)
            m_WriteBackController.ResetWriteBackState();

        m_IntroRoot.SetActive(!showRegisterPanel && !showAluPanel && !showMemoryPanel && !showWriteBackPanel);

        if (!m_LessonFlow.HasStarted)
        {
            m_AluController?.ResetExecutionState();
            m_WriteBackController?.ResetWriteBackState();
            if (m_RegisterRoot != null)
                m_RegisterRoot.SetActive(false);
            if (m_AluRoot != null)
                m_AluRoot.SetActive(false);
            if (m_MemRoot != null)
                m_MemRoot.SetActive(false);
            if (m_WriteBackRoot != null)
                m_WriteBackRoot.SetActive(false);
            SetText(
                m_IntroBody,
                $"Lesson Introduction\n\nSelected instruction: {m_LessonFlow.CurrentInstruction?.assemblyInstructionText ?? "add t2, t0, t1"}\n\nPress Start Lesson to begin.");
            SetText(m_IntroFeedback, string.Empty);
            SetButtonState(m_IntroActionButton, m_IntroActionLabel, m_StartButtonLabel, true);
            if (m_InstructionDropdown != null)
                m_InstructionDropdown.interactable = true;
            SetButtonState(m_RegisterActionButton, m_RegisterActionLabel, m_ContinueButtonLabel, false);
            SetButtonState(m_MemActionButton, m_MemActionLabel, m_ContinueButtonLabel, false);
            RefreshLayout(m_IntroRoot, m_IntroBody, m_IntroFeedback, m_IntroActionButton);
            RefreshLayout(m_RegisterRoot, m_RegisterBody, m_RegisterFeedback, m_RegisterActionButton);
            RefreshLayout(m_MemRoot, m_MemBody, m_MemFeedback, m_MemActionButton);
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
            SetButtonState(m_RegisterActionButton, m_RegisterActionLabel, m_ContinueButtonLabel, false);
            SetButtonState(m_MemActionButton, m_MemActionLabel, m_ContinueButtonLabel, false);
            RefreshLayout(m_IntroRoot, m_IntroBody, m_IntroFeedback, m_IntroActionButton);
            RefreshLayout(m_RegisterRoot, m_RegisterBody, m_RegisterFeedback, m_RegisterActionButton);
            RefreshLayout(m_MemRoot, m_MemBody, m_MemFeedback, m_MemActionButton);
            return;
        }

        if (showWriteBackPanel)
        {
            SetButtonState(m_IntroActionButton, m_IntroActionLabel, m_ContinueButtonLabel, false);
            SetButtonState(m_RegisterActionButton, m_RegisterActionLabel, m_ContinueButtonLabel, false);
            SetButtonState(m_MemActionButton, m_MemActionLabel, m_ContinueButtonLabel, false);
            RefreshLayout(m_IntroRoot, m_IntroBody, m_IntroFeedback, m_IntroActionButton);
            RefreshLayout(m_RegisterRoot, m_RegisterBody, m_RegisterFeedback, m_RegisterActionButton);
            RefreshLayout(m_MemRoot, m_MemBody, m_MemFeedback, m_MemActionButton);
            return;
        }

        if (showMemoryPanel)
        {
            SetText(m_MemBody, BuildMemoryBody(step));
            SetText(m_MemFeedback, string.Empty);
            SetButtonState(m_MemActionButton, m_MemActionLabel, m_ContinueButtonLabel, true);
            SetButtonState(m_IntroActionButton, m_IntroActionLabel, m_ContinueButtonLabel, false);
            SetButtonState(m_RegisterActionButton, m_RegisterActionLabel, m_ContinueButtonLabel, false);
            RefreshLayout(m_IntroRoot, m_IntroBody, m_IntroFeedback, m_IntroActionButton);
            RefreshLayout(m_RegisterRoot, m_RegisterBody, m_RegisterFeedback, m_RegisterActionButton);
            RefreshLayout(m_MemRoot, m_MemBody, m_MemFeedback, m_MemActionButton);
            return;
        }

        if (!showRegisterPanel)
        {
            SetText(m_IntroBody, BuildIntroBody(step));
            SetText(m_IntroFeedback, string.Empty);
            SetButtonState(
                m_IntroActionButton,
                m_IntroActionLabel,
                step.requiredInteraction == InstructionStepInteractionType.Completion ? m_RestartButtonLabel : m_ContinueButtonLabel,
                step.requiredInteraction == InstructionStepInteractionType.ContinueButton ||
                step.requiredInteraction == InstructionStepInteractionType.Completion);
            SetButtonState(m_RegisterActionButton, m_RegisterActionLabel, m_ContinueButtonLabel, false);
            SetButtonState(m_MemActionButton, m_MemActionLabel, m_ContinueButtonLabel, false);
            RefreshLayout(m_IntroRoot, m_IntroBody, m_IntroFeedback, m_IntroActionButton);
            RefreshLayout(m_RegisterRoot, m_RegisterBody, m_RegisterFeedback, m_RegisterActionButton);
            RefreshLayout(m_MemRoot, m_MemBody, m_MemFeedback, m_MemActionButton);
            return;
        }

        SetText(m_RegisterBody, BuildRegisterBody(step));
        var showContinue = step.requiredInteraction == InstructionStepInteractionType.ContinueButton ||
                           step.requiredInteraction == InstructionStepInteractionType.Completion ||
                           (step.requiredInteraction == InstructionStepInteractionType.RegisterSelection &&
                            m_LessonFlow.RegisterSelectionReadyToContinue);
        SetButtonState(
            m_RegisterActionButton,
            m_RegisterActionLabel,
            step.requiredInteraction == InstructionStepInteractionType.Completion ? m_RestartButtonLabel : m_ContinueButtonLabel,
            showContinue);
        SetButtonState(m_IntroActionButton, m_IntroActionLabel, m_ContinueButtonLabel, false);
        SetButtonState(m_MemActionButton, m_MemActionLabel, m_ContinueButtonLabel, false);
        RefreshLayout(m_IntroRoot, m_IntroBody, m_IntroFeedback, m_IntroActionButton);
        RefreshLayout(m_RegisterRoot, m_RegisterBody, m_RegisterFeedback, m_RegisterActionButton);
        RefreshLayout(m_MemRoot, m_MemBody, m_MemFeedback, m_MemActionButton);
    }

    bool ShouldShowRegisterPanel()
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

    string BuildIntroBody(InstructionFlowStep step)
    {
        var instruction = m_LessonFlow.CurrentInstruction;
        var body =
            $"Instruction: {instruction.displayName}\n\n" +
            $"Assembly: {instruction.assemblyInstructionText}\n\n" +
            $"Stage: {step.stepName}\n\n" +
            $"{step.explanation}";

        if (step.stepName.IndexOf("Fetch", System.StringComparison.OrdinalIgnoreCase) >= 0)
            body += "\n\nPress Continue to move into instruction decode.";

        return body;
    }

    string BuildRegisterBody(InstructionFlowStep step)
    {
        var instruction = m_LessonFlow.CurrentInstruction;
        if (instruction == null)
            return string.Empty;

        if (step.highlightedNode == DatapathNodeId.InstructionMemory)
        {
            var decodeFocus = instruction.usesImmediate
                ? "Identify rs, rt, and the immediate field. Operand 2 will come from the immediate path instead of a second register read."
                : "Identify rs, rt, and the destination register for this instruction.";

            return
                $"Instruction Decode\n\n" +
                $"Instruction: {instruction.displayName}\n\n" +
                $"Assembly: {instruction.assemblyInstructionText}\n\n" +
                $"Field Breakdown:\n{instruction.fieldBreakdownText}\n\n" +
                $"{decodeFocus}\n\n" +
                $"{step.explanation}";
        }

        if (step.requiredInteraction == InstructionStepInteractionType.RegisterSelection)
        {
            var requiredRoles = LessonChecks.GetRequiredRoles(instruction, step);
            var registerLines = string.Empty;
            foreach (var registerRole in requiredRoles)
            {
                registerLines += registerRole switch
                {
                    InstructionRegisterRole.Rs => $"Read Register 1 <- rs ({instruction.expectedRs})\n",
                    InstructionRegisterRole.Rt => $"Read Register 2 <- rt ({instruction.expectedRt})\n",
                    InstructionRegisterRole.Rd => $"Write Register <- rd ({instruction.expectedRd})\n",
                    _ => string.Empty,
                };
            }

            if (instruction.usesImmediate)
            {
                registerLines += $"Operand 2 <- immediate ({instruction.expectedImmediateValue})\n";
                registerLines += $"Write-back target remembered for later <- rt ({instruction.expectedRt})\n";
                registerLines += "Immediate packet <- spawned automatically, then sent through the Immediate Extender\n";
            }
            else if (instruction.usesDestinationRegister)
            {
                registerLines += $"Write-back target remembered for later <- rd ({instruction.expectedRd})\n";
            }

            return
                $"Instruction Decode\n\n" +
                $"Instruction: {instruction.displayName}\n\n" +
                $"Assembly: {instruction.assemblyInstructionText}\n\n" +
                $"Place the required source registers on the active scanners.\n\n" +
                $"{registerLines}\n" +
                $"{step.explanation}";
        }

        return
            $"Instruction: {instruction.displayName}\n\n" +
            $"Assembly: {instruction.assemblyInstructionText}\n\n" +
            $"Stage: {step.stepName}\n\n" +
            $"{step.explanation}";
    }

    string BuildMemoryBody(InstructionFlowStep step)
    {
        var instruction = m_LessonFlow.CurrentInstruction;
        if (instruction == null)
            return string.Empty;

        var memorySummary = instruction.touchesDataMemory
            ? "This instruction uses the data-memory path."
            : "This instruction skips the data-memory path.";

        return
            $"Memory Access\n\n" +
            $"Instruction: {instruction.displayName}\n\n" +
            $"Assembly: {instruction.assemblyInstructionText}\n\n" +
            $"{memorySummary}\n\n" +
            $"Review this stage, then continue when ready.\n\n" +
            $"{step.explanation}";
    }

    void CacheReferences()
    {
        m_AluController ??= FindFirstSceneObject<AluExecutionController>();
        m_WriteBackController ??= FindFirstSceneObject<WriteBackController>();

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

        if (m_MemRoot != null)
        {
            m_MemBody ??= FindNamedText(m_MemRoot.transform, "Text Body");
            m_MemFeedback ??= FindNamedText(m_MemRoot.transform, "Text Feedback");
            m_MemActionButton ??= m_MemRoot.GetComponentInChildren<Button>(true);
            m_MemActionLabel ??= m_MemActionButton != null
                ? m_MemActionButton.GetComponentInChildren<TMP_Text>(true)
                : null;
        }

        if (m_InstructionDropdown == null && m_IntroRoot != null)
            m_InstructionDropdown = m_IntroRoot.GetComponentInChildren<TMP_Dropdown>(true);
    }

    void PopulateInstructionDropdown()
    {
        if (m_InstructionDropdown == null || m_LessonFlow == null)
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

    static void SetText(TMP_Text target, string text)
    {
        if (target == null)
            return;

        target.text = text;
        target.gameObject.SetActive(!string.IsNullOrWhiteSpace(text));
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

        body?.ForceMeshUpdate();
        feedback?.ForceMeshUpdate();
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

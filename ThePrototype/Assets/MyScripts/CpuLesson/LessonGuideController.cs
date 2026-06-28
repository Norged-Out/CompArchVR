using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Drives the authored lesson guide panels already placed in Testing Ground.
/// All scene references are assigned directly in the Inspector.
/// </summary>
[DisallowMultipleComponent]
public class LessonGuideController : MonoBehaviour
{
    const float k_ActionButtonHeight = 56f;

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

    [Header("Control Decode UI")]
    [SerializeField]
    GameObject m_ControlDecodeRoot;

    [SerializeField]
    ControlDecodeController m_ControlDecodeController;

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

    void Awake()
    {
        CacheReferences();
        HookButtons();
        EnsureButtonLayout(m_IntroActionButton);
        EnsureButtonLayout(m_RegisterActionButton);
        RefreshView();
    }

    void OnEnable()
    {
        CacheReferences();

        if (m_AluController != null)
            m_AluController.ExecutionCompleted += HandleAluExecutionCompleted;

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
    }

    void HandleIntroActionPressed()
    {
        if (m_LessonFlow == null)
            return;

        if (!m_LessonFlow.HasStarted)
            m_LessonFlow.StartLesson();
        else
            m_LessonFlow.Advance();
    }

    void HandleRegisterActionPressed()
    {
        if (m_LessonFlow == null)
            return;

        if (!m_LessonFlow.HasStarted)
            m_LessonFlow.StartLesson();
        else
            m_LessonFlow.Advance();
    }

    void HandleStepChanged(CpuLessonFlow _)
    {
        RefreshView();
    }

    void HandleAluExecutionCompleted(int resultValue)
    {
        m_LessonFlow?.CompleteAluExecution(resultValue);
    }

    void HandleFeedbackChanged(string message, bool isFailure)
    {
        var feedbackColor = isFailure
            ? new Color(1f, 0.55f, 0.55f, 1f)
            : new Color(0.78f, 0.96f, 0.82f, 1f);

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

        if (ShouldShowAluPanel())
            return;

        if (ShouldShowControlDecodePanel())
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

        var showControlDecode = ShouldShowControlDecodePanel();
        var showRegisterPanel = ShouldShowRegisterPanel();
        var showAluPanel = ShouldShowAluPanel();

        if (m_ControlDecodeRoot != null)
            m_ControlDecodeRoot.SetActive(showControlDecode);

        m_ControlDecodeController?.SetPhaseState(showControlDecode, m_LessonFlow.CurrentInstruction);

        if (m_RegisterRoot != null)
            m_RegisterRoot.SetActive(showRegisterPanel);

        if (m_AluRoot != null)
            m_AluRoot.SetActive(showAluPanel);

        m_AluController?.SetPhaseState(showAluPanel, m_LessonFlow.CurrentInstruction);

        m_IntroRoot.SetActive(!showControlDecode && !showRegisterPanel && !showAluPanel);

        if (!m_LessonFlow.HasStarted)
        {
            m_AluController?.ResetExecutionState();
            SetText(
                m_IntroBody,
                $"Lesson Introduction\n\nSelected instruction: {m_LessonFlow.CurrentInstruction?.assemblyInstructionText ?? "add t2, t0, t1"}\n\nPress Start Lesson to begin the walkthrough.");
            SetText(m_IntroFeedback, string.Empty);
            SetButtonState(m_IntroActionButton, m_IntroActionLabel, m_StartButtonLabel, true);
            SetButtonState(m_RegisterActionButton, m_RegisterActionLabel, m_ContinueButtonLabel, false);
            RefreshLayout(m_IntroRoot, m_IntroBody, m_IntroFeedback, m_IntroActionButton);
            RefreshLayout(m_RegisterRoot, m_RegisterBody, m_RegisterFeedback, m_RegisterActionButton);
            return;
        }

        var step = m_LessonFlow.CurrentStep;
        if (step == null)
            return;

        if (showControlDecode)
        {
            SetButtonState(m_IntroActionButton, m_IntroActionLabel, m_ContinueButtonLabel, false);
            SetButtonState(m_RegisterActionButton, m_RegisterActionLabel, m_ContinueButtonLabel, false);
            RefreshLayout(m_IntroRoot, m_IntroBody, m_IntroFeedback, m_IntroActionButton);
            RefreshLayout(m_RegisterRoot, m_RegisterBody, m_RegisterFeedback, m_RegisterActionButton);
            return;
        }

        if (showAluPanel)
        {
            SetButtonState(m_IntroActionButton, m_IntroActionLabel, m_ContinueButtonLabel, false);
            SetButtonState(m_RegisterActionButton, m_RegisterActionLabel, m_ContinueButtonLabel, false);
            RefreshLayout(m_IntroRoot, m_IntroBody, m_IntroFeedback, m_IntroActionButton);
            RefreshLayout(m_RegisterRoot, m_RegisterBody, m_RegisterFeedback, m_RegisterActionButton);
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
                step.requiredInteraction == InstructionStepInteractionType.ContinueButton || step.requiredInteraction == InstructionStepInteractionType.Completion);
            SetButtonState(m_RegisterActionButton, m_RegisterActionLabel, m_ContinueButtonLabel, false);
            RefreshLayout(m_IntroRoot, m_IntroBody, m_IntroFeedback, m_IntroActionButton);
            RefreshLayout(m_RegisterRoot, m_RegisterBody, m_RegisterFeedback, m_RegisterActionButton);
            return;
        }

        SetText(m_RegisterBody, BuildRegisterBody(step));
        var showContinue = step.requiredInteraction == InstructionStepInteractionType.ContinueButton ||
                           step.requiredInteraction == InstructionStepInteractionType.Completion;
        SetButtonState(
            m_RegisterActionButton,
            m_RegisterActionLabel,
            step.requiredInteraction == InstructionStepInteractionType.Completion ? m_RestartButtonLabel : m_ContinueButtonLabel,
            showContinue);
        SetButtonState(m_IntroActionButton, m_IntroActionLabel, m_ContinueButtonLabel, false);
        RefreshLayout(m_IntroRoot, m_IntroBody, m_IntroFeedback, m_IntroActionButton);
        RefreshLayout(m_RegisterRoot, m_RegisterBody, m_RegisterFeedback, m_RegisterActionButton);
    }

    bool ShouldShowControlDecodePanel()
    {
        var step = m_LessonFlow?.CurrentStep;
        if (step == null)
            return false;

        return step.stepName.IndexOf("Instruction Memory", System.StringComparison.OrdinalIgnoreCase) >= 0;
    }

    bool ShouldShowRegisterPanel()
    {
        if (ShouldShowControlDecodePanel() || ShouldShowAluPanel())
            return false;

        var step = m_LessonFlow?.CurrentStep;
        if (step == null)
            return false;

        return step.requiredInteraction == InstructionStepInteractionType.RegisterSelection;
    }

    bool ShouldShowAluPanel()
    {
        var step = m_LessonFlow?.CurrentStep;
        if (step == null)
            return false;

        return step.requiredInteraction == InstructionStepInteractionType.AluExecution;
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
            body += "\n\nPress Continue when you are ready to move into instruction decode.";

        if (step.highlightedNode == DatapathNodeId.WriteBack && m_LessonFlow.RuntimeSelection.hasAluResult)
        {
            body +=
                $"\n\nWrite-back target: {instruction.expectedRd}" +
                $"\nALU result value: {m_LessonFlow.RuntimeSelection.aluResultValue}" +
                "\n\nPress Continue to complete write-back.";
        }

        return body;
    }

    string BuildRegisterBody(InstructionFlowStep step)
    {
        var instruction = m_LessonFlow.CurrentInstruction;

        if (step.requiredInteraction == InstructionStepInteractionType.RegisterSelection)
        {
            return
                $"Instruction: {instruction.displayName}\n\n" +
                $"Assembly: {instruction.assemblyInstructionText}\n\n" +
                $"Place the correct registers on the scanners.\n\n" +
                $"Read Register 1 <- rs ({instruction.expectedRs})\n" +
                $"Read Register 2 <- rt ({instruction.expectedRt})\n" +
                $"Write Register <- rd ({instruction.expectedRd})\n\n" +
                $"{step.explanation}";
        }

        if (step.requiredInteraction == InstructionStepInteractionType.WriteBackRegisterConfirmation)
        {
            return
                $"Instruction: {instruction.displayName}\n\n" +
                $"Assembly: {instruction.assemblyInstructionText}\n\n" +
                $"Confirm write-back by placing {instruction.expectedRd} on Write Register.\n\n" +
                $"{step.explanation}";
        }

        return
            $"Instruction: {instruction.displayName}\n\n" +
            $"Assembly: {instruction.assemblyInstructionText}\n\n" +
            $"Stage: {step.stepName}\n\n" +
            $"{step.explanation}";
    }

    void CacheReferences()
    {
        m_AluController ??= FindFirstSceneObject<AluExecutionController>();

        if (m_AluRoot == null)
        {
            var aluRootTransform = FindSceneTransform("ALU UI");
            m_AluRoot = aluRootTransform != null ? aluRootTransform.gameObject : null;
        }
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
}

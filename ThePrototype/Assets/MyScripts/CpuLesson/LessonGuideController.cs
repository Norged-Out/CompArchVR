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

    void Awake()
    {
        HookButtons();
        RefreshView();
    }

    void OnEnable()
    {
        if (m_LessonFlow == null)
            return;

        m_LessonFlow.StepChanged += HandleStepChanged;
        m_LessonFlow.FeedbackChanged += HandleFeedbackChanged;
        RefreshView();
    }

    void OnDisable()
    {
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
            }

            return;
        }

        if (ShouldShowControlDecodePanel())
            return;

        if (m_IntroFeedback != null)
        {
            m_IntroFeedback.text = message;
            m_IntroFeedback.color = feedbackColor;
        }
    }

    void RefreshView()
    {
        if (m_LessonFlow == null || m_IntroRoot == null)
            return;

        var showControlDecode = ShouldShowControlDecodePanel();
        var showRegisterPanel = ShouldShowRegisterPanel();

        if (m_ControlDecodeRoot != null)
            m_ControlDecodeRoot.SetActive(showControlDecode);

        m_ControlDecodeController?.SetPhaseState(showControlDecode, m_LessonFlow.CurrentInstruction);

        if (m_RegisterRoot != null)
            m_RegisterRoot.SetActive(showRegisterPanel);

        m_IntroRoot.SetActive(!showControlDecode && (!showRegisterPanel || !m_LessonFlow.HasStarted));

        if (!m_LessonFlow.HasStarted)
        {
            SetText(
                m_IntroBody,
                $"Lesson Introduction\n\nSelected instruction: {m_LessonFlow.CurrentInstruction?.assemblyInstructionText ?? "add t2, t0, t1"}\n\nPress Start Lesson to begin the walkthrough.");
            SetText(m_IntroFeedback, string.Empty);
            SetButtonState(m_IntroActionButton, m_IntroActionLabel, m_StartButtonLabel, true);
            SetButtonState(m_RegisterActionButton, m_RegisterActionLabel, m_ContinueButtonLabel, false);
            return;
        }

        var step = m_LessonFlow.CurrentStep;
        if (step == null)
            return;

        if (showControlDecode)
        {
            SetButtonState(m_IntroActionButton, m_IntroActionLabel, m_ContinueButtonLabel, false);
            SetButtonState(m_RegisterActionButton, m_RegisterActionLabel, m_ContinueButtonLabel, false);
            return;
        }

        if (!showRegisterPanel)
        {
            SetText(m_IntroBody, BuildIntroBody(step));
            SetButtonState(
                m_IntroActionButton,
                m_IntroActionLabel,
                step.requiredInteraction == InstructionStepInteractionType.Completion ? m_RestartButtonLabel : m_ContinueButtonLabel,
                step.requiredInteraction == InstructionStepInteractionType.ContinueButton || step.requiredInteraction == InstructionStepInteractionType.Completion);
            SetButtonState(m_RegisterActionButton, m_RegisterActionLabel, m_ContinueButtonLabel, false);
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
        if (ShouldShowControlDecodePanel())
            return false;

        var step = m_LessonFlow?.CurrentStep;
        if (step == null)
            return false;

        return step.requiredInteraction == InstructionStepInteractionType.RegisterSelection ||
               step.requiredInteraction == InstructionStepInteractionType.WriteBackRegisterConfirmation;
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

    static void SetText(TMP_Text target, string text)
    {
        if (target != null)
            target.text = text;
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
}

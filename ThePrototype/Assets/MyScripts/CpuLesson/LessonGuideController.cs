using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Scene-side bridge between the authored Lesson Guide panels and the
/// CpuLessonFlow state machine.
/// It keeps the current MVP grounded in the scene the user is actively editing:
/// - `Intro UI` handles lesson start plus fetch/decode text
/// - a cloned register-area panel handles register / ALU / write-back prompts
/// </summary>
[DisallowMultipleComponent]
public class LessonGuideController : MonoBehaviour
{
    [SerializeField]
    string m_IntroRootName = "Intro UI";

    [SerializeField]
    string m_RegisterAnchorName = "Instruction Decode";

    [SerializeField]
    string m_StartButtonLabel = "Start Lesson";

    [SerializeField]
    string m_ContinueButtonLabel = "Continue";

    [SerializeField]
    string m_RestartButtonLabel = "Restart";

    CpuLessonFlow m_LessonFlow;
    RegisterBank m_RegisterBank;

    Transform m_RegisterAnchor;
    GameObject m_IntroRoot;
    GameObject m_RegisterRoot;

    TMP_Text m_IntroHeader;
    TMP_Text m_IntroBody;
    TMP_Text m_IntroFeedback;
    Button m_IntroActionButton;
    TMP_Text m_IntroActionLabel;

    TMP_Text m_RegisterHeader;
    TMP_Text m_RegisterBody;
    TMP_Text m_RegisterFeedback;
    Button m_RegisterActionButton;
    TMP_Text m_RegisterActionLabel;

    void Awake()
    {
        m_LessonFlow = GetComponent<CpuLessonFlow>() ?? FindFirstObjectByType<CpuLessonFlow>();
        m_RegisterBank = FindFirstObjectByType<RegisterBank>();

        CacheSceneObjects();
        EnsureRegisterPanel();
        EnsurePanelWidgets(
            m_IntroRoot,
            "Instruction Overview",
            out m_IntroHeader,
            out m_IntroBody,
            out m_IntroFeedback,
            out m_IntroActionButton,
            out m_IntroActionLabel);
        EnsurePanelWidgets(
            m_RegisterRoot,
            "Register Stage",
            out m_RegisterHeader,
            out m_RegisterBody,
            out m_RegisterFeedback,
            out m_RegisterActionButton,
            out m_RegisterActionLabel);
        HookButtons();
        RefreshView();
    }

    void OnEnable()
    {
        if (m_LessonFlow == null)
            return;

        m_LessonFlow.StepChanged += HandleStepChanged;
        m_LessonFlow.FeedbackChanged += HandleFeedbackChanged;
    }

    void OnDisable()
    {
        if (m_LessonFlow == null)
            return;

        m_LessonFlow.StepChanged -= HandleStepChanged;
        m_LessonFlow.FeedbackChanged -= HandleFeedbackChanged;
    }

    void CacheSceneObjects()
    {
        if (m_IntroRoot == null)
        {
            foreach (Transform child in transform)
            {
                if (child.name == m_IntroRootName)
                {
                    m_IntroRoot = child.gameObject;
                    break;
                }
            }
        }

        m_RegisterAnchor ??= transform.Find(m_RegisterAnchorName);
    }

    void EnsureRegisterPanel()
    {
        if (m_RegisterAnchor == null || m_IntroRoot == null)
            return;

        if (m_RegisterRoot == null)
        {
            var existing = m_RegisterAnchor.Find("Register UI");
            if (existing != null)
            {
                m_RegisterRoot = existing.gameObject;
            }
            else
            {
                var introPanel = m_IntroRoot.transform.Find("Spatial Panel Scroll");
                if (introPanel != null)
                {
                    var clonedPanel = Instantiate(introPanel.gameObject, m_RegisterAnchor, false);
                    clonedPanel.name = "Register UI";
                    m_RegisterRoot = clonedPanel;
                }
            }
        }

        if (m_RegisterRoot == null || m_RegisterBank == null)
            return;

        // Put the register panel beside the authored register zone without
        // destroying the original Lesson Guide structure.
        var registerZone = m_RegisterBank.transform;
        m_RegisterAnchor.position = registerZone.position + new Vector3(0.2f, 1.25f, -0.8f);
        m_RegisterAnchor.rotation = m_IntroRoot.transform.rotation;
    }

    void EnsurePanelWidgets(
        GameObject panelRoot,
        string defaultHeader,
        out TMP_Text headerText,
        out TMP_Text bodyText,
        out TMP_Text feedbackText,
        out Button actionButton,
        out TMP_Text actionLabel)
    {
        headerText = null;
        bodyText = null;
        feedbackText = null;
        actionButton = null;
        actionLabel = null;

        if (panelRoot == null)
            return;

        headerText = FindHeaderText(panelRoot.transform);
        if (headerText != null && string.IsNullOrWhiteSpace(headerText.text))
            headerText.text = defaultHeader;

        var content = panelRoot.transform.Find("Spatial Panel Scroll/Scroll View/Viewport/Content");
        if (content == null)
            content = panelRoot.transform.Find("Scroll View/Viewport/Content");

        if (content == null)
            return;

        bodyText = EnsureTextItem(content, "Body Text", 190f, 15, new Color(0.92f, 0.96f, 1f, 1f));
        feedbackText = EnsureTextItem(content, "Feedback Text", 90f, 14, Color.white);
        actionButton = EnsureActionButton(content, out actionLabel);
    }

    TMP_Text FindHeaderText(Transform root)
    {
        foreach (var text in root.GetComponentsInChildren<TMP_Text>(true))
        {
            if (text == null)
                continue;

            if (text.transform.name.Contains("Header"))
                return text;
        }

        return null;
    }

    TMP_Text EnsureTextItem(Transform parent, string objectName, float preferredHeight, float fontSize, Color color)
    {
        var existing = parent.Find(objectName);
        TextMeshProUGUI text;

        if (existing == null)
        {
            var textObject = new GameObject(
                objectName,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(TextMeshProUGUI),
                typeof(LayoutElement));
            textObject.transform.SetParent(parent, false);
            text = textObject.GetComponent<TextMeshProUGUI>();
            var layout = textObject.GetComponent<LayoutElement>();
            layout.preferredHeight = preferredHeight;
            layout.flexibleHeight = 0;
        }
        else
        {
            text = existing.GetComponent<TextMeshProUGUI>();
        }

        text.enableWordWrapping = true;
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = TextAlignmentOptions.TopLeft;
        text.raycastTarget = false;
        text.text = string.Empty;
        return text;
    }

    Button EnsureActionButton(Transform parent, out TMP_Text labelText)
    {
        labelText = null;
        var existing = parent.Find("Action Button");
        GameObject buttonObject;

        if (existing == null)
        {
            buttonObject = new GameObject(
                "Action Button",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(Button),
                typeof(LayoutElement));
            buttonObject.transform.SetParent(parent, false);

            var layout = buttonObject.GetComponent<LayoutElement>();
            layout.preferredHeight = 62f;
            layout.flexibleHeight = 0;

            var image = buttonObject.GetComponent<Image>();
            image.color = new Color(0.15f, 0.52f, 0.9f, 1f);

            var labelObject = new GameObject(
                "Label",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(TextMeshProUGUI));
            labelObject.transform.SetParent(buttonObject.transform, false);

            var labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            labelText = labelObject.GetComponent<TextMeshProUGUI>();
            labelText.alignment = TextAlignmentOptions.Center;
            labelText.fontSize = 18;
            labelText.color = Color.white;
            labelText.raycastTarget = false;
        }
        else
        {
            buttonObject = existing.gameObject;
            labelText = existing.GetComponentInChildren<TMP_Text>(true);
        }

        return buttonObject.GetComponent<Button>();
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
        }
        else if (m_IntroFeedback != null)
        {
            m_IntroFeedback.text = message;
            m_IntroFeedback.color = feedbackColor;
        }
    }

    void RefreshView()
    {
        if (m_LessonFlow == null || m_IntroRoot == null)
            return;

        var showRegisterPanel = ShouldShowRegisterPanel();
        if (m_RegisterRoot != null)
            m_RegisterRoot.SetActive(showRegisterPanel);

        m_IntroRoot.SetActive(!showRegisterPanel || !m_LessonFlow.HasStarted);

        if (!m_LessonFlow.HasStarted)
        {
            SetHeader(m_IntroHeader, "Lesson Introduction");
            SetText(
                m_IntroBody,
                $"Selected instruction: {m_LessonFlow.CurrentInstruction?.assemblyInstructionText ?? "add t2, t0, t1"}\n\nPress Start Lesson to begin the walkthrough.");
            SetText(m_IntroFeedback, string.Empty);
            SetButtonState(m_IntroActionButton, m_IntroActionLabel, m_StartButtonLabel, true);
            SetButtonState(m_RegisterActionButton, m_RegisterActionLabel, m_ContinueButtonLabel, false);
            return;
        }

        var step = m_LessonFlow.CurrentStep;
        if (step == null)
            return;

        if (!showRegisterPanel)
        {
            SetHeader(m_IntroHeader, $"Instruction: {m_LessonFlow.CurrentInstruction.displayName}");
            SetText(m_IntroBody, BuildIntroBody(step));
            SetButtonState(
                m_IntroActionButton,
                m_IntroActionLabel,
                step.requiredInteraction == InstructionStepInteractionType.Completion ? m_RestartButtonLabel : m_ContinueButtonLabel,
                step.requiredInteraction == InstructionStepInteractionType.ContinueButton || step.requiredInteraction == InstructionStepInteractionType.Completion);
        }
        else
        {
            SetHeader(m_RegisterHeader, $"Instruction: {m_LessonFlow.CurrentInstruction.displayName}");
            SetText(m_RegisterBody, BuildRegisterBody(step));
            var showContinue = step.requiredInteraction == InstructionStepInteractionType.ContinueButton ||
                               step.requiredInteraction == InstructionStepInteractionType.Completion;
            SetButtonState(
                m_RegisterActionButton,
                m_RegisterActionLabel,
                step.requiredInteraction == InstructionStepInteractionType.Completion ? m_RestartButtonLabel : m_ContinueButtonLabel,
                showContinue);
        }
    }

    bool ShouldShowRegisterPanel()
    {
        var step = m_LessonFlow?.CurrentStep;
        if (step == null)
            return false;

        return step.requiredInteraction == InstructionStepInteractionType.RegisterSelection ||
               step.requiredInteraction == InstructionStepInteractionType.WriteBackRegisterConfirmation ||
               step.stepName.IndexOf("ALU", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
               step.stepName.IndexOf("Write", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
               step.requiredInteraction == InstructionStepInteractionType.Completion;
    }

    string BuildIntroBody(InstructionFlowStep step)
    {
        var instruction = m_LessonFlow.CurrentInstruction;
        var body = $"Instruction: {instruction.assemblyInstructionText}\n\nStage: {step.stepName}\n\n{step.explanation}";

        if (step.stepName.IndexOf("Instruction Memory", System.StringComparison.OrdinalIgnoreCase) >= 0)
        {
            body += $"\n\nField breakdown:\n{instruction.fieldBreakdownText}";
        }

        return body;
    }

    string BuildRegisterBody(InstructionFlowStep step)
    {
        var instruction = m_LessonFlow.CurrentInstruction;

        if (step.requiredInteraction == InstructionStepInteractionType.RegisterSelection)
        {
            return
                $"Instruction: {instruction.assemblyInstructionText}\n\n" +
                $"Place the correct registers on the scanners.\n\n" +
                $"Read Register 1 <- rs ({instruction.expectedRs})\n" +
                $"Read Register 2 <- rt ({instruction.expectedRt})\n" +
                $"Write Register <- rd ({instruction.expectedRd})\n\n" +
                $"{step.explanation}";
        }

        if (step.requiredInteraction == InstructionStepInteractionType.WriteBackRegisterConfirmation)
        {
            return
                $"Instruction: {instruction.assemblyInstructionText}\n\n" +
                $"Confirm write-back by placing {instruction.expectedRd} on Write Register.\n\n" +
                $"{step.explanation}";
        }

        return $"Instruction: {instruction.assemblyInstructionText}\n\nStage: {step.stepName}\n\n{step.explanation}";
    }

    static void SetHeader(TMP_Text target, string text)
    {
        if (target != null)
            target.text = text;
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

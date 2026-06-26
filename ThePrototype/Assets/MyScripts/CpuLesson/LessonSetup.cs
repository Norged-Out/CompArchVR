using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Temporary scene bootstrapper for the lesson prototype.
///
/// Preferred long-term direction:
/// - the user authors the scene layout in Unity
/// - this component only binds those references
///
/// Current role:
/// - prefer authored scene refs when they exist
/// - otherwise create a runtime fallback so the prototype still runs
/// </summary>
public class LessonSetup : MonoBehaviour
{
    [Header("Runtime Fallbacks")]

    [SerializeField]
    bool m_AllowRuntimeFallbacks = true;

    [SerializeField]
    Vector3 m_LessonUiLocalPosition = new(0f, 1.45f, 2.1f);

    [SerializeField]
    Vector3 m_RegisterBankLocalPosition = new(0f, 0.05f, 2.55f);

    [SerializeField]
    Vector3 m_ResetButtonLocalPosition = new(-0.85f, 0.35f, 1.25f);

    [Header("Optional Scene Authored Refs")]

    [SerializeField]
    Transform m_RuntimeRoot;

    [SerializeField]
    LessonUI m_LessonUi;

    [SerializeField]
    RegisterBank m_RegisterBank;

    [SerializeField]
    RegisterButton m_ResetButton;

    public LessonUI lessonUi => m_LessonUi;
    public RegisterBank registerBank => m_RegisterBank;

    /// <summary>
    /// Ensures the scene has the minimum services needed for the current lesson.
    /// </summary>
    public void Prepare(CpuLessonFlow flow, InstructionDefinition instruction)
    {
        EnsureDataMemoryPlaceholder();
        EnsureRuntimeRoot();
        EnsureLessonUi();
        EnsureRegisterBank();
        EnsureResetButton(flow);
        RenameAdvanceButtonText();

        if (instruction != null && m_RegisterBank != null)
            m_RegisterBank.BuildButtons(instruction.registerBankChoices);
    }

    void EnsureDataMemoryPlaceholder()
    {
        if (transform.Find("Data Memory") != null)
            return;

        if (!m_AllowRuntimeFallbacks)
            return;

        var aluNode = transform.Find("ALU");
        var writeBackNode = transform.Find("Write Back");
        if (aluNode == null || writeBackNode == null)
            return;

        var clonedNode = UnityEngine.Object.Instantiate(writeBackNode.gameObject, transform);
        clonedNode.name = "Data Memory";

        var spacing = writeBackNode.localPosition - aluNode.localPosition;
        clonedNode.transform.localPosition = writeBackNode.localPosition;
        clonedNode.transform.localRotation = writeBackNode.localRotation;
        clonedNode.transform.localScale = writeBackNode.localScale;

        // Keep Data Memory between ALU and Write Back in the placeholder chain.
        writeBackNode.localPosition += spacing;

        foreach (var tmpText in clonedNode.GetComponentsInChildren<TMP_Text>(true))
            tmpText.text = "Data Memory";
    }

    void EnsureRuntimeRoot()
    {
        if (m_RuntimeRoot != null)
            return;

        var runtimeRoot = transform.Find("Lesson Runtime");
        if (runtimeRoot == null && m_AllowRuntimeFallbacks)
        {
            var runtimeRootObject = new GameObject("Lesson Runtime");
            runtimeRootObject.transform.SetParent(transform, false);
            runtimeRootObject.transform.localPosition = Vector3.zero;
            runtimeRootObject.transform.localRotation = Quaternion.identity;
            runtimeRootObject.transform.localScale = Vector3.one;
            runtimeRoot = runtimeRootObject.transform;
        }

        m_RuntimeRoot = runtimeRoot;
    }

    void EnsureLessonUi()
    {
        if (m_LessonUi != null && m_LessonUi.HasTextReferences)
            return;

        m_LessonUi ??= GetComponentInChildren<LessonUI>(true);
        if (m_LessonUi != null && m_LessonUi.HasTextReferences)
            return;

        if (!m_AllowRuntimeFallbacks || m_RuntimeRoot == null)
            return;

        CreateFallbackLessonUi();
    }

    void EnsureRegisterBank()
    {
        if (m_RegisterBank != null)
            return;

        m_RegisterBank ??= GetComponentInChildren<RegisterBank>(true);
        if (m_RegisterBank != null)
            return;

        m_RegisterBank = FindRegisterBankInScene();
        if (m_RegisterBank != null)
            return;

        if (!m_AllowRuntimeFallbacks || m_RuntimeRoot == null)
            return;

        var registerBankRoot = new GameObject("Register Bank");
        registerBankRoot.transform.SetParent(m_RuntimeRoot, false);
        registerBankRoot.transform.localPosition = m_RegisterBankLocalPosition;
        registerBankRoot.transform.localRotation = Quaternion.identity;
        registerBankRoot.transform.localScale = Vector3.one;
        m_RegisterBank = registerBankRoot.AddComponent<RegisterBank>();
    }

    RegisterBank FindRegisterBankInScene()
    {
        foreach (var registerBank in Resources.FindObjectsOfTypeAll<RegisterBank>())
        {
            if (registerBank == null)
                continue;

            if (!registerBank.gameObject.scene.IsValid() || !registerBank.gameObject.scene.isLoaded)
                continue;

            if (registerBank.gameObject.scene != gameObject.scene)
                continue;

            if (string.Equals(registerBank.gameObject.name, "Register Bank", StringComparison.OrdinalIgnoreCase))
                return registerBank;
        }

        foreach (var registerBank in Resources.FindObjectsOfTypeAll<RegisterBank>())
        {
            if (registerBank == null)
                continue;

            if (!registerBank.gameObject.scene.IsValid() || !registerBank.gameObject.scene.isLoaded)
                continue;

            if (registerBank.gameObject.scene != gameObject.scene)
                continue;

            return registerBank;
        }

        return null;
    }

    void EnsureResetButton(CpuLessonFlow flow)
    {
        if (flow == null)
            return;

        if (m_ResetButton == null)
            m_ResetButton = FindResetButton();

        if (m_ResetButton == null)
        {
            if (!m_AllowRuntimeFallbacks || m_RuntimeRoot == null)
                return;

            m_ResetButton = CreateFallbackResetButton();
        }

        if (m_ResetButton == null)
            return;

        m_ResetButton.Pressed -= flow.HandleResetButtonPressed;
        m_ResetButton.Pressed += flow.HandleResetButtonPressed;
    }

    RegisterButton FindResetButton()
    {
        foreach (var button in GetComponentsInChildren<RegisterButton>(true))
        {
            if (button.buttonKind == RegisterButton.ButtonKind.Reset)
                return button;
        }

        return null;
    }

    RegisterButton CreateFallbackResetButton()
    {
        return ButtonFactory.CreateButton(
            parent: m_RuntimeRoot,
            objectName: "Reset Lesson Button",
            localPosition: m_ResetButtonLocalPosition,
            label: "Reset",
            buttonId: "Reset",
            buttonKind: RegisterButton.ButtonKind.Reset);
    }

    void RenameAdvanceButtonText()
    {
        var advanceButton = transform.Find("Advance Node Button");
        if (advanceButton == null)
            return;

        foreach (var tmpText in advanceButton.GetComponentsInChildren<TMP_Text>(true))
        {
            if (tmpText.text.IndexOf("Advance", StringComparison.OrdinalIgnoreCase) >= 0 ||
                tmpText.text.IndexOf("Node", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                tmpText.text = "Advance Step";
            }
        }
    }

    void CreateFallbackLessonUi()
    {
        var canvasRoot = new GameObject(
            "Lesson UI",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster));

        canvasRoot.transform.SetParent(m_RuntimeRoot, false);
        canvasRoot.transform.localPosition = m_LessonUiLocalPosition;
        canvasRoot.transform.localRotation = Quaternion.identity;
        canvasRoot.transform.localScale = new Vector3(0.0024f, 0.0024f, 0.0024f);

        var canvas = canvasRoot.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = Camera.main;

        var canvasRect = canvasRoot.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(850f, 470f);

        var scaler = canvasRoot.GetComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 10f;

        var background = new GameObject("Background", typeof(RectTransform), typeof(Image));
        background.transform.SetParent(canvasRoot.transform, false);
        var backgroundRect = background.GetComponent<RectTransform>();
        backgroundRect.anchorMin = Vector2.zero;
        backgroundRect.anchorMax = Vector2.one;
        backgroundRect.offsetMin = Vector2.zero;
        backgroundRect.offsetMax = Vector2.zero;
        background.GetComponent<Image>().color = new Color(0.08f, 0.11f, 0.16f, 0.88f);

        var instructionNameText = CreateCanvasText(
            canvasRoot.transform,
            "Instruction Name",
            new Vector2(0.05f, 0.78f),
            new Vector2(0.95f, 0.95f),
            38,
            FontStyles.Bold);

        var stageNameText = CreateCanvasText(
            canvasRoot.transform,
            "Stage Name",
            new Vector2(0.05f, 0.65f),
            new Vector2(0.95f, 0.78f),
            30,
            FontStyles.Bold);

        var explanationText = CreateCanvasText(
            canvasRoot.transform,
            "Explanation",
            new Vector2(0.05f, 0.2f),
            new Vector2(0.95f, 0.66f),
            27,
            FontStyles.Normal);

        var feedbackText = CreateCanvasText(
            canvasRoot.transform,
            "Feedback",
            new Vector2(0.05f, 0.05f),
            new Vector2(0.95f, 0.18f),
            26,
            FontStyles.Bold);

        m_LessonUi = canvasRoot.AddComponent<LessonUI>();
        m_LessonUi.Bind(instructionNameText, stageNameText, explanationText, feedbackText);
    }

    static TextMeshProUGUI CreateCanvasText(
        Transform parent,
        string objectName,
        Vector2 anchorMin,
        Vector2 anchorMax,
        int fontSize,
        FontStyles fontStyle)
    {
        var textObject = new GameObject(objectName, typeof(RectTransform));
        textObject.transform.SetParent(parent, false);

        var rectTransform = textObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        var text = textObject.AddComponent<TextMeshProUGUI>();
        text.enableWordWrapping = true;
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.alignment = TextAlignmentOptions.TopLeft;
        text.color = Color.white;
        text.text = string.Empty;
        text.margin = new Vector4(12f, 8f, 12f, 8f);
        return text;
    }

}

using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Presentation owner for the instruction-decode lesson panel.
/// </summary>
[DisallowMultipleComponent]
public sealed class DecodePanelController : MonoBehaviour
{
    const float k_ActionButtonHeight = 56f;

    [SerializeField]
    TMP_Text m_OpcodeLessonText;

    [SerializeField]
    TMP_Text m_RegisterLessonText;

    [SerializeField]
    TMP_Text m_FunctLessonText;

    [SerializeField]
    TMP_Text m_OpcodeBodyText;

    [SerializeField]
    TMP_Text m_RegisterBodyText;

    [SerializeField]
    TMP_Text m_FunctBodyText;

    [SerializeField]
    TMP_Text m_OpcodeSelectionText;

    [SerializeField]
    TMP_Text m_RegisterSelectionText;

    [SerializeField]
    TMP_Text m_FunctSelectionText;

    [SerializeField]
    TMP_Text m_Feedback;

    [SerializeField]
    Button m_ActionButton;

    [SerializeField]
    TMP_Text m_ActionLabel;

    [SerializeField]
    TMP_Dropdown m_OpcodeDropdown;

    [SerializeField]
    TMP_Dropdown m_FunctDropdown;

    [SerializeField]
    TMP_Dropdown m_HintDropdown;

    [SerializeField]
    TMP_Text m_HintText;

    readonly List<string> m_OpcodeOptions = new();
    readonly List<string> m_FunctOptions = new();

    public Button ActionButton => m_ActionButton;
    public TMP_Dropdown OpcodeDropdown => m_OpcodeDropdown;
    public TMP_Dropdown FunctDropdown => m_FunctDropdown;
    public TMP_Dropdown HintDropdown => m_HintDropdown;

    public void SetVisible(bool isVisible)
    {
        gameObject.SetActive(isVisible);
    }

    public void HideAction()
    {
        SetButtonState(m_ActionButton, m_ActionLabel, string.Empty, false);
        RefreshLayout();
    }

    public void PopulateDropdowns(IReadOnlyList<InstructionDefinition> availableInstructions, InstructionDefinition currentInstruction, ref bool isRefreshing)
    {
        PopulateOpcodeDropdown(availableInstructions, currentInstruction, ref isRefreshing);
        PopulateFunctDropdown(availableInstructions, currentInstruction, ref isRefreshing);
        PopulateHintDropdown(ref isRefreshing);
    }

    public void ResetDropdowns(ref bool isRefreshing, ref bool isDecodeFunctStepActive)
    {
        isRefreshing = true;

        if (m_OpcodeDropdown != null)
            m_OpcodeDropdown.SetValueWithoutNotify(0);

        if (m_FunctDropdown != null)
            m_FunctDropdown.SetValueWithoutNotify(0);

        if (m_HintDropdown != null)
            m_HintDropdown.SetValueWithoutNotify(0);

        isRefreshing = false;
        isDecodeFunctStepActive = false;
        SetText(m_HintText, string.Empty);
    }

    public void ResetFunctDropdown(ref bool isRefreshing)
    {
        if (m_FunctDropdown == null)
            return;

        isRefreshing = true;
        m_FunctDropdown.SetValueWithoutNotify(0);
        isRefreshing = false;
    }

    public void Refresh(
        CpuLessonFlow lessonFlow,
        InstructionFlowStep step,
        IReadOnlyList<InstructionDefinition> availableInstructions,
        bool isDecodeFunctStepActive,
        string continueLabel,
        string restartLabel)
    {
        if (lessonFlow == null || step == null)
            return;

        SetVisible(true);

        var isOpcodeStep = IsOpcodeSelectionStep(lessonFlow, isDecodeFunctStepActive);
        var isFunctStep = IsFunctSelectionStep(lessonFlow, isDecodeFunctStepActive);
        var isRegisterStep = step.requiredInteraction == InstructionStepInteractionType.RegisterSelection;

        SetActive(m_OpcodeLessonText, isOpcodeStep);
        SetActive(m_FunctLessonText, isFunctStep);
        SetActive(m_RegisterLessonText, isRegisterStep);
        SetActive(m_OpcodeBodyText, isOpcodeStep);
        SetActive(m_FunctBodyText, isFunctStep);
        SetActive(m_RegisterBodyText, isRegisterStep);
        SetActive(m_OpcodeSelectionText, isOpcodeStep);
        SetActive(m_FunctSelectionText, isFunctStep);
        SetActive(m_RegisterSelectionText, isRegisterStep);

        SetText(m_OpcodeSelectionText, isOpcodeStep ? BuildOpcodeSelectionText(lessonFlow) : string.Empty);
        SetText(m_FunctSelectionText, isFunctStep ? BuildFunctSelectionText(lessonFlow) : string.Empty);
        SetText(m_RegisterSelectionText, isRegisterStep ? BuildRegisterSelectionText(lessonFlow, step) : string.Empty);

        RefreshDropdownState(lessonFlow, step, isDecodeFunctStepActive);
        RefreshHintText(availableInstructions);

        var showContinue = isOpcodeStep ||
                           isFunctStep ||
                           step.requiredInteraction == InstructionStepInteractionType.ContinueButton ||
                           step.requiredInteraction == InstructionStepInteractionType.Completion ||
                           (step.requiredInteraction == InstructionStepInteractionType.RegisterSelection &&
                            lessonFlow.RegisterSelectionReadyToContinue);

        SetButtonState(
            m_ActionButton,
            m_ActionLabel,
            step.requiredInteraction == InstructionStepInteractionType.Completion ? restartLabel : continueLabel,
            showContinue);

        RefreshLayout();
    }

    public void SetFeedback(string message, bool isFailure, IReadOnlyList<InstructionDefinition> availableInstructions)
    {
        if (m_Feedback == null)
            return;

        m_Feedback.text = message;
        m_Feedback.color = isFailure
            ? new Color(1f, 0.55f, 0.55f, 1f)
            : new Color(0.78f, 0.96f, 0.82f, 1f);
        m_Feedback.gameObject.SetActive(!string.IsNullOrWhiteSpace(message));
        RefreshHintText(availableInstructions);
        RefreshLayout();
    }

    public void RefreshHintText(IReadOnlyList<InstructionDefinition> availableInstructions)
    {
        if (m_HintDropdown == null)
            return;

        string hintText;
        switch (m_HintDropdown.value)
        {
            case 1:
                hintText = BuildOpcodeHintText(availableInstructions);
                break;
            case 2:
                hintText = BuildFunctHintText(availableInstructions);
                break;
            default:
                hintText = string.Empty;
                break;
        }

        SetText(m_HintText, hintText);
        RefreshLayout();
    }

    public string GetSelectedOpcode()
    {
        if (m_OpcodeDropdown == null ||
            m_OpcodeDropdown.options == null ||
            m_OpcodeDropdown.value <= 0 ||
            m_OpcodeDropdown.value >= m_OpcodeDropdown.options.Count)
        {
            return string.Empty;
        }

        return m_OpcodeDropdown.options[m_OpcodeDropdown.value].text.Trim();
    }

    public string GetSelectedFunct()
    {
        if (m_FunctDropdown == null ||
            m_FunctDropdown.options == null ||
            m_FunctDropdown.value <= 0 ||
            m_FunctDropdown.value >= m_FunctDropdown.options.Count)
        {
            return string.Empty;
        }

        return m_FunctDropdown.options[m_FunctDropdown.value].text.Trim();
    }

    public bool IsOpcodeSelectionStep(CpuLessonFlow lessonFlow, bool isDecodeFunctStepActive)
    {
        var step = lessonFlow != null ? lessonFlow.CurrentStep : null;
        return step != null &&
               step.highlightedNode == DatapathNodeId.InstructionMemory &&
               !isDecodeFunctStepActive;
    }

    public bool IsFunctSelectionStep(CpuLessonFlow lessonFlow, bool isDecodeFunctStepActive)
    {
        var step = lessonFlow != null ? lessonFlow.CurrentStep : null;
        return step != null &&
               step.highlightedNode == DatapathNodeId.InstructionMemory &&
               isDecodeFunctStepActive;
    }

    public static bool InstructionUsesDecodeFunct(InstructionDefinition instruction)
    {
        return instruction != null &&
               !string.IsNullOrWhiteSpace(instruction.functBits) &&
               string.Equals(instruction.opcodeBits != null ? instruction.opcodeBits.Trim() : string.Empty, "000000", StringComparison.Ordinal);
    }

    string BuildOpcodeSelectionText(CpuLessonFlow lessonFlow)
    {
        var instruction = lessonFlow.CurrentInstruction;
        if (instruction == null)
            return string.Empty;

        return $"Assembly: {instruction.assemblyInstructionText}";
    }

    string BuildFunctSelectionText(CpuLessonFlow lessonFlow)
    {
        var instruction = lessonFlow.CurrentInstruction;
        if (instruction == null)
            return string.Empty;

        return $"Assembly: {instruction.assemblyInstructionText}";
    }

    string BuildRegisterSelectionText(CpuLessonFlow lessonFlow, InstructionFlowStep step)
    {
        var instruction = lessonFlow.CurrentInstruction;
        if (instruction == null)
            return string.Empty;

        if (step.requiredInteraction != InstructionStepInteractionType.RegisterSelection)
            return step.explanation;

        var lines = new List<string>();
        var requiredRoles = LessonChecks.GetRequiredRoles(instruction, step);

        for (var index = 0; index < requiredRoles.Length; index++)
        {
            var role = requiredRoles[index];
            var registerName = instruction.GetExpectedRegisterName(role);
            var scannerName = GetScannerLabel(role);
            var status = index < lessonFlow.CurrentRegisterSelectionIndex ? "done" : "pending";
            lines.Add($"{scannerName}: {registerName} [{status}]");
        }

        if (instruction.usesImmediate)
        {
            var immediateStatus = lessonFlow.RegisterSelectionReadyToContinue ? "ready to generate" : "locked";
            lines.Add($"Immediate packet: {instruction.expectedImmediateValue} [{immediateStatus}]");
        }

        var nextAction = lessonFlow.RegisterSelectionReadyToContinue
            ? instruction.usesImmediate
                ? "Press Continue to generate the immediate packet and proceed to Execution."
                : "Press Continue to proceed to Execution."
            : $"Current target: {GetCurrentDecodeTargetLabel(lessonFlow, instruction, step)}.";

        return $"{string.Join("\n", lines)}\n\n{nextAction}";
    }

    string BuildOpcodeHintText(IReadOnlyList<InstructionDefinition> availableInstructions)
    {
        var lines = new List<string>();
        foreach (var instruction in availableInstructions)
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

    string BuildFunctHintText(IReadOnlyList<InstructionDefinition> availableInstructions)
    {
        var lines = new List<string>();
        foreach (var instruction in availableInstructions)
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

    string GetCurrentDecodeTargetLabel(CpuLessonFlow lessonFlow, InstructionDefinition instruction, InstructionFlowStep step)
    {
        var requiredRoles = LessonChecks.GetRequiredRoles(instruction, step);
        var currentIndex = lessonFlow != null ? lessonFlow.CurrentRegisterSelectionIndex : 0;
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

    void PopulateOpcodeDropdown(IReadOnlyList<InstructionDefinition> availableInstructions, InstructionDefinition currentInstruction, ref bool isRefreshing)
    {
        if (m_OpcodeDropdown == null)
            return;

        m_OpcodeOptions.Clear();
        var optionLabels = new List<string> { "Choose Opcode" };
        foreach (var instruction in availableInstructions)
        {
            if (instruction == null || string.IsNullOrWhiteSpace(instruction.opcodeBits))
                continue;

            var opcode = instruction.opcodeBits.Trim();
            if (m_OpcodeOptions.Contains(opcode))
                continue;

            m_OpcodeOptions.Add(opcode);
            optionLabels.Add(opcode);
        }

        if (currentInstruction != null && !string.IsNullOrWhiteSpace(currentInstruction.opcodeBits))
        {
            var currentOpcode = currentInstruction.opcodeBits.Trim();
            if (!m_OpcodeOptions.Contains(currentOpcode))
            {
                m_OpcodeOptions.Add(currentOpcode);
                optionLabels.Add(currentOpcode);
            }
        }

        isRefreshing = true;
        m_OpcodeDropdown.ClearOptions();
        m_OpcodeDropdown.AddOptions(optionLabels);
        m_OpcodeDropdown.SetValueWithoutNotify(0);
        isRefreshing = false;
    }

    void PopulateFunctDropdown(IReadOnlyList<InstructionDefinition> availableInstructions, InstructionDefinition currentInstruction, ref bool isRefreshing)
    {
        if (m_FunctDropdown == null)
            return;

        m_FunctOptions.Clear();
        var optionLabels = new List<string> { "Choose Funct" };
        foreach (var instruction in availableInstructions)
        {
            if (instruction == null || string.IsNullOrWhiteSpace(instruction.functBits))
                continue;

            var funct = instruction.functBits.Trim();
            if (m_FunctOptions.Contains(funct))
                continue;

            m_FunctOptions.Add(funct);
            optionLabels.Add(funct);
        }

        if (currentInstruction != null && !string.IsNullOrWhiteSpace(currentInstruction.functBits))
        {
            var currentFunct = currentInstruction.functBits.Trim();
            if (!m_FunctOptions.Contains(currentFunct))
            {
                m_FunctOptions.Add(currentFunct);
                optionLabels.Add(currentFunct);
            }
        }

        isRefreshing = true;
        m_FunctDropdown.ClearOptions();
        m_FunctDropdown.AddOptions(optionLabels);
        m_FunctDropdown.SetValueWithoutNotify(0);
        isRefreshing = false;
    }

    void PopulateHintDropdown(ref bool isRefreshing)
    {
        if (m_HintDropdown == null)
            return;

        isRefreshing = true;
        m_HintDropdown.ClearOptions();
        m_HintDropdown.AddOptions(new List<string> { "Choose Option", "Opcode", "Funct" });
        m_HintDropdown.SetValueWithoutNotify(0);
        isRefreshing = false;
    }

    void RefreshDropdownState(CpuLessonFlow lessonFlow, InstructionFlowStep step, bool isDecodeFunctStepActive)
    {
        var showOpcodeDropdown = IsOpcodeSelectionStep(lessonFlow, isDecodeFunctStepActive);
        var showFunctDropdown = IsFunctSelectionStep(lessonFlow, isDecodeFunctStepActive);

        if (m_OpcodeDropdown != null)
        {
            m_OpcodeDropdown.gameObject.SetActive(showOpcodeDropdown);
            m_OpcodeDropdown.interactable = showOpcodeDropdown;
        }

        if (m_FunctDropdown != null)
        {
            m_FunctDropdown.gameObject.SetActive(showFunctDropdown);
            m_FunctDropdown.interactable = showFunctDropdown;
        }

        if (m_HintDropdown != null)
            m_HintDropdown.gameObject.SetActive(true);

        if (m_HintText != null)
            m_HintText.gameObject.SetActive(m_HintDropdown != null && m_HintDropdown.value > 0);
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

    void RefreshLayout()
    {
        if (!gameObject.activeInHierarchy)
            return;

        foreach (var textMesh in GetComponentsInChildren<TMP_Text>(true))
            textMesh?.ForceMeshUpdate();

        EnsureButtonLayout(m_ActionButton);
        Canvas.ForceUpdateCanvases();

        var scrollRect = GetComponentInChildren<ScrollRect>(true);
        if (scrollRect != null && scrollRect.content != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content);

            if (scrollRect.viewport != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.viewport);
        }

        var rootRect = GetComponent<RectTransform>();
        if (rootRect != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(rootRect);

        Canvas.ForceUpdateCanvases();
    }
}

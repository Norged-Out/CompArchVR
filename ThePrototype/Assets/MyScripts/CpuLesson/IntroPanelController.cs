using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Presentation owner for the intro and instruction-fetch lesson panel.
/// </summary>
[DisallowMultipleComponent]
public sealed class IntroPanelController : MonoBehaviour
{
    const float k_ActionButtonHeight = 56f;

    [SerializeField]
    TMP_Text m_Body;

    [SerializeField]
    TMP_Text m_Feedback;

    [SerializeField]
    Button m_ActionButton;

    [SerializeField]
    TMP_Text m_ActionLabel;

    [SerializeField]
    TMP_Dropdown m_InstructionDropdown;

    public Button ActionButton => m_ActionButton;
    public TMP_Dropdown InstructionDropdown => m_InstructionDropdown;

    public void PopulateInstructionDropdown(IReadOnlyList<InstructionDefinition> instructions, InstructionDefinition currentInstruction, ref bool isRefreshing)
    {
        if (m_InstructionDropdown == null)
            return;

        isRefreshing = true;
        m_InstructionDropdown.ClearOptions();

        var optionLabels = new List<string>();
        foreach (var instruction in instructions)
            optionLabels.Add(instruction != null ? instruction.displayName : "Instruction");

        if (optionLabels.Count > 0)
            m_InstructionDropdown.AddOptions(optionLabels);

        var currentIndex = 0;
        for (var index = 0; index < instructions.Count; index++)
        {
            if (instructions[index] == currentInstruction)
            {
                currentIndex = index;
                break;
            }
        }

        if (m_InstructionDropdown.options.Count > 0)
            m_InstructionDropdown.SetValueWithoutNotify(currentIndex);

        isRefreshing = false;
    }

    public void SetInstructionDropdownInteractable(bool interactable)
    {
        if (m_InstructionDropdown != null)
            m_InstructionDropdown.interactable = interactable;
    }

    public void SetVisible(bool isVisible)
    {
        gameObject.SetActive(isVisible);
    }

    public void ShowBeforeStart(InstructionDefinition currentInstruction, string startButtonLabel)
    {
        SetVisible(true);
        SetText(
            m_Body,
            $"Lesson Introduction\n\nSelected instruction: {currentInstruction?.assemblyInstructionText ?? "add t2, t0, t1"}\n\nPress Start Lesson to begin.");
        SetText(m_Feedback, string.Empty);
        SetButtonState(m_ActionButton, m_ActionLabel, startButtonLabel, true);
        SetInstructionDropdownInteractable(true);
        RefreshLayout();
    }

    public void ShowStep(CpuLessonFlow lessonFlow, InstructionFlowStep step, string continueButtonLabel, string restartButtonLabel)
    {
        if (lessonFlow == null || step == null)
            return;

        SetVisible(true);
        SetText(m_Body, BuildIntroBody(lessonFlow, step));
        SetText(m_Feedback, string.Empty);
        SetButtonState(
            m_ActionButton,
            m_ActionLabel,
            step.requiredInteraction == InstructionStepInteractionType.Completion ? restartButtonLabel : continueButtonLabel,
            step.requiredInteraction == InstructionStepInteractionType.ContinueButton ||
            step.requiredInteraction == InstructionStepInteractionType.Completion);
        RefreshLayout();
    }

    public void HideAction()
    {
        SetButtonState(m_ActionButton, m_ActionLabel, string.Empty, false);
        RefreshLayout();
    }

    public void SetFeedback(string message, bool isFailure)
    {
        if (m_Feedback == null)
            return;

        m_Feedback.text = message;
        m_Feedback.color = isFailure
            ? new Color(1f, 0.55f, 0.55f, 1f)
            : new Color(0.78f, 0.96f, 0.82f, 1f);
        m_Feedback.gameObject.SetActive(!string.IsNullOrWhiteSpace(message));
        RefreshLayout();
    }

    string BuildIntroBody(CpuLessonFlow lessonFlow, InstructionFlowStep step)
    {
        var instruction = lessonFlow.CurrentInstruction;
        if (instruction == null)
            return string.Empty;

        if (step.stepName.IndexOf("Fetch", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            if (lessonFlow.UsesInstructionTerminals)
            {
                var transportStatus = lessonFlow.IsInstructionReadyForDecode
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
            $"Next: {GetNextStageLabel(lessonFlow, step)}.";
    }

    static string GetNextStageLabel(CpuLessonFlow lessonFlow, InstructionFlowStep currentStep)
    {
        var instruction = lessonFlow != null ? lessonFlow.CurrentInstruction : null;
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

using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

/// <summary>
/// Presentation owner for the intro and instruction-fetch lesson panel.
/// </summary>
[DisallowMultipleComponent]
public sealed class IntroPanelController : LessonPanelBase
{
    readonly LessonPhaseRouter m_PhaseRouter = new();

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

    /// <summary>
    /// Rebuilds the instruction dropdown from the current authored instruction catalog.
    /// </summary>
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

    /// <summary>
    /// Locks or unlocks instruction selection before the lesson officially begins.
    /// </summary>
    public void SetInstructionDropdownInteractable(bool interactable)
    {
        if (m_InstructionDropdown != null)
            m_InstructionDropdown.interactable = interactable;
    }

    /// <summary>
    /// Shows or hides the authored intro panel.
    /// </summary>
    public void SetVisible(bool isVisible)
    {
        SetPanelVisible(isVisible);
    }

    /// <summary>
    /// Shows the pre-start screen where the learner chooses an instruction and begins the walkthrough.
    /// </summary>
    public void ShowBeforeStart(InstructionDefinition currentInstruction, string startButtonLabel)
    {
        SetVisible(true);
        SetTextField(
            m_Body,
            $"Lesson Introduction\n\nSelected instruction: {currentInstruction?.assemblyInstructionText ?? "add t2, t0, t1"}\n\nPress Start Lesson to begin.");
        SetTextField(m_Feedback, string.Empty);
        SetButtonState(m_ActionButton, m_ActionLabel, startButtonLabel, true);
        SetInstructionDropdownInteractable(true);
        RefreshPanelLayout(m_ActionButton);
    }

    /// <summary>
    /// Shows the currently active fetch or recap step on the intro panel.
    /// </summary>
    public void ShowStep(CpuLessonFlow lessonFlow, InstructionFlowStep step, string continueButtonLabel, string restartButtonLabel)
    {
        if (lessonFlow == null || step == null)
            return;

        SetVisible(true);
        SetTextField(m_Body, BuildIntroBody(lessonFlow, step));
        SetTextField(m_Feedback, string.Empty);
        SetButtonState(
            m_ActionButton,
            m_ActionLabel,
            step.requiredInteraction == InstructionStepInteractionType.Completion ? restartButtonLabel : continueButtonLabel,
            step.requiredInteraction == InstructionStepInteractionType.ContinueButton ||
            step.requiredInteraction == InstructionStepInteractionType.Completion);
        RefreshPanelLayout(m_ActionButton);
    }

    /// <summary>
    /// Hides the authored action button when another phase owns the learner's progression button.
    /// </summary>
    public void HideAction()
    {
        SetButtonState(m_ActionButton, m_ActionLabel, string.Empty, false);
        RefreshPanelLayout(m_ActionButton);
    }

    /// <summary>
    /// Applies shared intro feedback styling.
    /// </summary>
    public void SetFeedback(string message, bool isFailure)
    {
        SetFeedbackField(m_Feedback, message, isFailure);
        RefreshPanelLayout(m_ActionButton);
    }

    /// <summary>
    /// Rebinds the authored intro action button to the supplied lesson callback.
    /// </summary>
    public void BindAction(UnityAction listener)
    {
        if (m_ActionButton == null)
            return;

        m_ActionButton.onClick.RemoveAllListeners();
        if (listener != null)
            m_ActionButton.onClick.AddListener(listener);
    }

    /// <summary>
    /// Rebinds the instruction dropdown to the supplied lesson callback.
    /// </summary>
    public void BindInstructionSelection(UnityAction<int> listener)
    {
        if (m_InstructionDropdown == null)
            return;

        m_InstructionDropdown.onValueChanged.RemoveAllListeners();
        if (listener != null)
            m_InstructionDropdown.onValueChanged.AddListener(listener);
    }

    /// <summary>
    /// Builds the learner-facing description for the current intro-owned lesson step.
    /// </summary>
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
            $"Next: {m_PhaseRouter.GetNextStageLabel(lessonFlow, step)}.";
    }
}

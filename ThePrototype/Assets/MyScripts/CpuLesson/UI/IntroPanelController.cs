using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Presentation owner for the intro and instruction-fetch lesson panel.
/// </summary>
[DisallowMultipleComponent]
public sealed class IntroPanelController : LessonPanelBase
{
    const string k_LogPrefix = "[IntroPanelController]";

    readonly LessonPhaseRouter m_PhaseRouter = new();

    [SerializeField]
    TMP_Text m_Body;

    [SerializeField]
    TMP_Text m_Header;

    [SerializeField]
    Button m_ActionButton;

    [SerializeField]
    TMP_Text m_ActionLabel;

    [SerializeField]
    GameObject m_ModeGroupRoot;

    [SerializeField]
    TMP_Dropdown m_ModeDropdown;

    [SerializeField]
    GameObject m_InstructionGroupRoot;

    [SerializeField]
    TMP_Dropdown m_InstructionDropdown;

    /// <summary>
    /// Rebuilds the mode dropdown from the currently supported lesson modes.
    /// </summary>
    public void PopulateModeDropdown(LessonMode currentMode, ref bool isRefreshing)
    {
        if (m_ModeDropdown == null)
            return;

        isRefreshing = true;
        m_ModeDropdown.ClearOptions();
        m_ModeDropdown.AddOptions(new List<string> { "Learning", "Practice", "Test" });
        m_ModeDropdown.SetValueWithoutNotify((int)currentMode);
        isRefreshing = false;
    }

    /// <summary>
    /// Rebuilds the second intro dropdown from whatever selection bank the
    /// current lesson mode wants to expose.
    /// </summary>
    public void PopulateSelectionDropdown(IReadOnlyList<string> optionLabels, int currentIndex, ref bool isRefreshing)
    {
        if (m_InstructionDropdown == null)
            return;

        isRefreshing = true;
        m_InstructionDropdown.ClearOptions();

        if (optionLabels.Count > 0)
            m_InstructionDropdown.AddOptions(new List<string>(optionLabels));

        if (m_InstructionDropdown.options.Count > 0)
            m_InstructionDropdown.SetValueWithoutNotify(Mathf.Clamp(currentIndex, 0, m_InstructionDropdown.options.Count - 1));

        isRefreshing = false;
    }

    /// <summary>
    /// Locks or unlocks mode selection before the lesson officially begins.
    /// </summary>
    public void SetModeDropdownInteractable(bool interactable)
    {
        if (m_ModeDropdown != null)
            m_ModeDropdown.interactable = interactable;
    }

    /// <summary>
    /// Shows or hides the authored mode dropdown when the lesson begins or resets.
    /// </summary>
    public void SetModeDropdownVisible(bool isVisible)
    {
        SetDropdownGroupVisible(m_ModeGroupRoot, m_ModeDropdown, isVisible);
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
    /// Shows or hides the authored instruction dropdown when the lesson begins or resets.
    /// </summary>
    public void SetInstructionDropdownVisible(bool isVisible)
    {
        SetDropdownGroupVisible(m_InstructionGroupRoot, m_InstructionDropdown, isVisible);
    }

    /// <summary>
    /// Shows or hides the authored intro panel.
    /// </summary>
    public void SetVisible(bool isVisible)
    {
        Debug.Log($"{k_LogPrefix} SetVisible | visible={isVisible} frame={Time.frameCount}", this);
        SetPanelVisible(isVisible);
    }

    /// <summary>
    /// Shows the pre-start screen where the learner chooses an instruction and begins the walkthrough.
    /// </summary>
    public void ShowBeforeStart(
        LessonMode lessonMode,
        InstructionDefinition currentInstruction,
        string startButtonLabel,
        bool showInstructionDropdown,
        bool canStartSelectedMode)
    {
        Debug.Log(
            $"{k_LogPrefix} ShowBeforeStart | mode={lessonMode} showInstructionDropdown={showInstructionDropdown} canStart={canStartSelectedMode} instruction={(currentInstruction != null ? currentInstruction.displayName : "<null>")} frame={Time.frameCount}",
            this);
        SetVisible(true);
        SetTextField(m_Header, "Lesson Introduction");
        SetTextField(m_Body, BuildBeforeStartBody(lessonMode, canStartSelectedMode));
        SetButtonState(m_ActionButton, m_ActionLabel, startButtonLabel, canStartSelectedMode);
        SetModeDropdownVisible(true);
        SetModeDropdownInteractable(true);
        SetInstructionDropdownInteractable(showInstructionDropdown);
        SetInstructionDropdownVisible(showInstructionDropdown);
        RefreshPanelLayout(m_ActionButton);
    }

    /// <summary>
    /// Shows the currently active fetch or recap step on the intro panel.
    /// </summary>
    public void ShowStep(
        CpuLessonFlow lessonFlow,
        InstructionFlowStep step,
        string continueButtonLabel,
        string goBackButtonLabel,
        string restartButtonLabel)
    {
        if (lessonFlow == null || step == null)
            return;

        Debug.Log(
            $"{k_LogPrefix} ShowStep | step={step.stepName} mode={lessonFlow.CurrentMode} hasStarted={lessonFlow.HasStarted} interaction={step.requiredInteraction} frame={Time.frameCount}",
            this);

        var isFetchStep = IsFetchStep(step);
        var buttonLabel = isFetchStep
            ? goBackButtonLabel
            : step.requiredInteraction == InstructionStepInteractionType.Completion
                ? restartButtonLabel
                : continueButtonLabel;
        var buttonEnabled = isFetchStep ||
            step.requiredInteraction == InstructionStepInteractionType.ContinueButton ||
            step.requiredInteraction == InstructionStepInteractionType.Completion;

        SetVisible(true);
        SetTextField(m_Header, lessonFlow.HasStarted ? "Instruction Fetch" : "Lesson Introduction");
        SetTextField(m_Body, BuildIntroBody(lessonFlow, step));
        SetButtonState(
            m_ActionButton,
            m_ActionLabel,
            buttonLabel,
            buttonEnabled);
        SetModeDropdownVisible(false);
        SetInstructionDropdownVisible(false);
        RefreshPanelLayout(m_ActionButton);
    }

    /// <summary>
    /// Hides the authored action button when another phase owns the learner's progression button.
    /// </summary>
    public void HideAction()
    {
        Debug.Log($"{k_LogPrefix} HideAction | frame={Time.frameCount}", this);
        SetButtonState(m_ActionButton, m_ActionLabel, string.Empty, false);
        RefreshPanelLayout(m_ActionButton);
    }

    /// <summary>
    /// Builds the learner-facing description for the current intro-owned lesson step.
    /// </summary>
    string BuildIntroBody(CpuLessonFlow lessonFlow, InstructionFlowStep step)
    {
        var instruction = lessonFlow.CurrentInstruction;
        if (instruction == null)
            return string.Empty;

        if (IsFetchStep(step))
        {
            if (LessonModePolicy.IsAssessmentMode(lessonFlow.CurrentMode) && lessonFlow.CurrentPracticeInstruction != null)
            {
                return
                    "Instruction fetch uses the Program Counter to locate the next instruction in memory.\n\n" +
                    $"{(lessonFlow.CurrentMode == LessonMode.Test ? "Random Encoded Instruction" : "Encoded Instruction")}: {lessonFlow.CurrentPracticeInstruction.GetHexInstructionText()}\n\n" +
                    "The selected encoded instruction has been uploaded to the fetch terminal. Pick up the module, carry it to the decode terminal, and dock it there to unlock Instruction Decode.";
            }

            return
                $"Instruction fetch uses the Program Counter to locate the next instruction in memory.\n\n" +
                $"Instruction: {instruction.displayName}\n" +
                $"Assembly: {instruction.assemblyInstructionText}\n\n" +
                "The selected instruction has been uploaded to the fetch terminal. Pick up the module, carry it to the decode terminal, and dock it there to unlock Instruction Decode.";
        }

        return
            $"Instruction: {instruction.displayName}\n" +
            $"Assembly: {instruction.assemblyInstructionText}\n\n" +
            $"{step.explanation}\n\n" +
            $"Next: {m_PhaseRouter.GetNextStageLabel(lessonFlow, step)}.";
    }

    static void SetDropdownGroupVisible(GameObject groupRoot, TMP_Dropdown dropdown, bool isVisible)
    {
        if (groupRoot != null)
        {
            groupRoot.SetActive(isVisible);
            return;
        }

        if (dropdown != null)
            dropdown.gameObject.SetActive(isVisible);
    }

    static bool IsFetchStep(InstructionFlowStep step)
    {
        return step != null &&
               step.stepName.IndexOf("Fetch", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    static string BuildBeforeStartBody(LessonMode lessonMode, bool canStartSelectedMode)
    {
        if (!canStartSelectedMode)
            return "The selected mode does not currently have a valid authored instruction pool. Assign at least one compatible instruction, then try again.";

        return lessonMode switch
        {
            LessonMode.Practice =>
                "Practice Mode will present an encoded instruction instead of its assembly form.\n\n" +
                "Decode it first, then complete the remaining datapath phases with limited guidance.",
            LessonMode.Test =>
                "Test Mode removes lesson guidance and hint support.\n\n" +
                "A random encoded instruction will be chosen for you. You will have one validation attempt and one scanner mistake per phase.",
            _ =>
                "Welcome to the MIPS Single-Cycle Datapath Virtual Reality Experience.\n\n" +
                "You are about to trace an instruction through the core stages of a single-cycle CPU and experience how each part of the datapath contributes to its completion.",
        };
    }
}

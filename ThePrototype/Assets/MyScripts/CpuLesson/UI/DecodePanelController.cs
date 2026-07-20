using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class DecodePanelController : LessonPanelBase
{
    [Header("Lesson Panel")]
    [SerializeField]
    DecodeLessonPanelRefs m_LessonPanel;

    [Header("Hint Panel")]
    [SerializeField]
    DecodeHintPanelRefs m_HintPanel;

    [Header("Interaction Panel / Learn")]
    [SerializeField]
    DecodeLearnInteractionRefs m_LearnInteraction;

    [Header("Interaction Panel / Practice")]
    [SerializeField]
    DecodePracticeInteractionRefs m_PracticeInteraction;

    [Header("Interaction Panel / Shared")]
    [SerializeField]
    DecodeSharedInteractionRefs m_SharedInteraction;

    LearnDecodeView m_LearningView;
    PracticeDecodeView m_PracticeView;
    bool m_IsShowingPracticeDecode;

    public void SetVisible(bool isVisible)
    {
        SetPanelVisible(isVisible);
    }

    public void HideAction()
    {
        SetButtonState(m_SharedInteraction.ActionButton, m_SharedInteraction.ActionLabel, string.Empty, false);
        RefreshPanelLayout(m_SharedInteraction.ActionButton);
    }

    public void PopulateDropdowns(
        IReadOnlyList<InstructionDefinition> availableInstructions,
        InstructionDefinition currentInstruction,
        ref bool isRefreshing)
    {
        EnsureLearningView();
        m_LearningView.PopulateDropdowns(availableInstructions, currentInstruction, ref isRefreshing);
    }

    public void ResetDropdowns(ref bool isRefreshing)
    {
        EnsureLearningView();
        m_LearningView.ResetDropdowns(ref isRefreshing);
    }

    public void ResetFunctDropdown(ref bool isRefreshing)
    {
        EnsureLearningView();
        m_LearningView.ResetFunctDropdown(ref isRefreshing);
    }

    public void PopulatePracticeControls(
        PracticeInstructionDefinition currentInstruction,
        bool opcodeConfirmed)
    {
        EnsurePracticeView();
        m_PracticeView.Refresh(currentInstruction, opcodeConfirmed, false);
    }

    public void ResetPracticeControls()
    {
        EnsurePracticeView();
        m_PracticeView.Reset();
    }

    public void Refresh(
        CpuLessonFlow lessonFlow,
        InstructionFlowStep step,
        IReadOnlyList<InstructionDefinition> availableInstructions,
        PracticeInstructionDefinition currentPracticeInstruction,
        DecodeSelectionMode selectionMode,
        PracticeDecodeFlow practiceDecodeFlow,
        string continueLabel,
        string restartLabel)
    {
        if (lessonFlow == null || step == null)
            return;

        SetVisible(true);

        m_IsShowingPracticeDecode = IsPracticeDecodeStep(lessonFlow, step);

        ApplyHintPanelMode(lessonFlow.CurrentMode);

        if (m_IsShowingPracticeDecode)
        {
            RefreshPracticeDecode(
                lessonFlow.CurrentMode,
                currentPracticeInstruction,
                practiceDecodeFlow,
                continueLabel,
                restartLabel);
            return;
        }

        RefreshLearningDecode(lessonFlow, step, availableInstructions, selectionMode, continueLabel, restartLabel);
    }

    public void SetFeedback(string message, bool isFailure, IReadOnlyList<InstructionDefinition> availableInstructions)
    {
        SetFeedbackField(m_SharedInteraction.Feedback, message, isFailure);

        if (!m_IsShowingPracticeDecode)
            RefreshHintText(availableInstructions);

        RefreshPanelLayout(m_SharedInteraction.ActionButton);
    }

    public void RefreshHintText(IReadOnlyList<InstructionDefinition> availableInstructions)
    {
        EnsureLearningView();
        m_LearningView.RefreshHintText(availableInstructions);
        RefreshPanelLayout(m_SharedInteraction.ActionButton);
    }

    public string GetSelectedOpcode()
    {
        EnsureLearningView();
        return m_LearningView.GetSelectedOpcode();
    }

    public string GetSelectedFunct()
    {
        EnsureLearningView();
        return m_LearningView.GetSelectedFunct();
    }

    internal PracticeDecodeInputState GetPracticeInputState()
    {
        EnsurePracticeView();
        return m_PracticeView.CaptureInputState();
    }

    public void SetPracticeHintText(string hintText)
    {
        EnsurePracticeView();
        m_PracticeView.SetHintText(hintText);
        RefreshPanelLayout(m_SharedInteraction.ActionButton);
    }

    void RefreshPracticeDecode(
        LessonMode lessonMode,
        PracticeInstructionDefinition currentPracticeInstruction,
        PracticeDecodeFlow practiceDecodeFlow,
        string continueLabel,
        string restartLabel)
    {
        EnsureLearningView();
        EnsurePracticeView();

        m_LearningView.HideAll();
        ApplyPracticeLessonText(practiceDecodeFlow != null && practiceDecodeFlow.IsOpcodeConfirmed, lessonMode);

        var opcodeConfirmed = practiceDecodeFlow != null && practiceDecodeFlow.IsOpcodeConfirmed;
        var isFailed = practiceDecodeFlow != null && practiceDecodeFlow.IsFailed;

        m_PracticeView.Refresh(currentPracticeInstruction, opcodeConfirmed, isFailed);
        m_PracticeView.SetStatusText(
            practiceDecodeFlow != null
                ? practiceDecodeFlow.GetDecodeStatusText(currentPracticeInstruction)
                : string.Empty);

        SetButtonState(
            m_SharedInteraction.ActionButton,
            m_SharedInteraction.ActionLabel,
            isFailed ? restartLabel : continueLabel,
            true);
        RefreshPanelLayout(m_SharedInteraction.ActionButton);
    }

    void RefreshLearningDecode(
        CpuLessonFlow lessonFlow,
        InstructionFlowStep step,
        IReadOnlyList<InstructionDefinition> availableInstructions,
        DecodeSelectionMode selectionMode,
        string continueLabel,
        string restartLabel)
    {
        EnsureLearningView();
        m_LearningView.Refresh(lessonFlow, step, availableInstructions, selectionMode);
        ApplyPracticeLessonText(false, lessonFlow.CurrentMode);

        if (m_PracticeInteraction.Root != null)
            m_PracticeInteraction.Root.SetActive(false);

        var isPracticeDecodeScannerFailure =
            LessonModePolicy.IsAssessmentMode(lessonFlow.CurrentMode) &&
            lessonFlow.IsPracticeDecodeScannerFailureAwaitingReset;

        SetButtonState(
            m_SharedInteraction.ActionButton,
            m_SharedInteraction.ActionLabel,
            isPracticeDecodeScannerFailure || step.requiredInteraction == InstructionStepInteractionType.Completion ? restartLabel : continueLabel,
            isPracticeDecodeScannerFailure || ShouldShowContinue(selectionMode, step, lessonFlow));
        RefreshPanelLayout(m_SharedInteraction.ActionButton);
    }

    void EnsureLearningView()
    {
        m_LearningView ??= new LearnDecodeView(
            m_LessonPanel.OpcodeText,
            m_LessonPanel.RegisterText,
            m_LessonPanel.FunctText,
            m_LearnInteraction.OpcodeGroupRoot,
            m_LearnInteraction.FunctGroupRoot,
            m_SharedInteraction.RegisterBodyText,
            m_LearnInteraction.OpcodeSelectionText,
            m_SharedInteraction.RegisterSelectionText,
            m_LearnInteraction.FunctSelectionText,
            m_LearnInteraction.OpcodeDropdown,
            m_LearnInteraction.FunctDropdown,
            m_HintPanel.InfoDropdown,
            m_HintPanel.InfoText);
    }

    void EnsurePracticeView()
    {
        m_PracticeView ??= new PracticeDecodeView(
            m_PracticeInteraction.Root,
            m_PracticeInteraction.BinaryText,
            m_PracticeInteraction.StatusText,
            m_PracticeInteraction.OpcodeGroupRoot,
            m_PracticeInteraction.OpcodeInputField,
            m_PracticeInteraction.RsGroupRoot,
            m_PracticeInteraction.RsInputField,
            m_PracticeInteraction.RtGroupRoot,
            m_PracticeInteraction.RtInputField,
            m_PracticeInteraction.RdGroupRoot,
            m_PracticeInteraction.RdToggle,
            m_PracticeInteraction.RdInputField,
            m_PracticeInteraction.ImmediateGroupRoot,
            m_PracticeInteraction.ImmediateToggle,
            m_PracticeInteraction.ImmediateInputField,
            m_PracticeInteraction.FunctGroupRoot,
            m_PracticeInteraction.FunctToggle,
            m_PracticeInteraction.FunctInputField,
            m_HintPanel.HintText);
    }

    void ApplyHintPanelMode(LessonMode lessonMode)
    {
        SetObjectActive(m_HintPanel.Root, LessonModePolicy.UsesHintPanel(lessonMode));
        SetObjectActive(m_HintPanel.InfoRoot, lessonMode == LessonMode.Learning);
        SetTextFieldActive(m_HintPanel.InfoText, lessonMode == LessonMode.Learning);

        if (m_HintPanel.HintButton != null)
            m_HintPanel.HintButton.gameObject.SetActive(lessonMode != LessonMode.Learning && LessonModePolicy.UsesHintPanel(lessonMode));

        SetTextFieldActive(
            m_HintPanel.HintText,
            lessonMode != LessonMode.Learning &&
            LessonModePolicy.UsesHintPanel(lessonMode) &&
            !string.IsNullOrWhiteSpace(m_HintPanel.HintText.text));
    }

    void ApplyPracticeLessonText(bool showDecodingLesson, LessonMode lessonMode)
    {
        SetObjectActive(m_LessonPanel.Root, LessonModePolicy.UsesLessonPanel(lessonMode));
        SetTextFieldActive(m_LessonPanel.OpcodeText, !showDecodingLesson);
        SetTextFieldActive(m_LessonPanel.PracticeDecodingText, showDecodingLesson);
        SetTextFieldActive(m_LessonPanel.FunctText, false);
        SetTextFieldActive(m_LessonPanel.RegisterText, false);
    }

    static bool IsPracticeDecodeStep(CpuLessonFlow lessonFlow, InstructionFlowStep step)
    {
        return LessonModePolicy.IsAssessmentMode(lessonFlow.CurrentMode) &&
               step.highlightedNode == DatapathNodeId.InstructionMemory &&
               step.requiredInteraction != InstructionStepInteractionType.RegisterSelection;
    }

    static bool ShouldShowContinue(DecodeSelectionMode selectionMode, InstructionFlowStep step, CpuLessonFlow lessonFlow)
    {
        var isSelectionStep = selectionMode == DecodeSelectionMode.Opcode || selectionMode == DecodeSelectionMode.Funct;
        return isSelectionStep ||
               step.requiredInteraction == InstructionStepInteractionType.ContinueButton ||
               step.requiredInteraction == InstructionStepInteractionType.Completion ||
               (step.requiredInteraction == InstructionStepInteractionType.RegisterSelection &&
                lessonFlow.RegisterSelectionReadyToContinue);
    }
}

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Presentation owner for the instruction-decode lesson panel.
/// </summary>
[DisallowMultipleComponent]
public sealed class DecodePanelController : LessonPanelBase
{
    [SerializeField]
    TMP_Text m_OpcodeLessonText;

    [SerializeField]
    TMP_Text m_RegisterLessonText;

    [SerializeField]
    TMP_Text m_FunctLessonText;

    [Header("Practice Decode")]
    [SerializeField]
    GameObject m_PracticeRoot;

    [SerializeField]
    TMP_Text m_PracticeBinaryText;

    [SerializeField]
    TMP_Text m_PracticeStatusText;

    [SerializeField]
    GameObject m_PracticeOpcodeGroupRoot;

    [SerializeField]
    TMP_Dropdown m_PracticeOpcodeDropdown;

    [SerializeField]
    GameObject m_PracticeRsGroupRoot;

    [SerializeField]
    TMP_Dropdown m_PracticeRsDropdown;

    [SerializeField]
    GameObject m_PracticeRtGroupRoot;

    [SerializeField]
    TMP_Dropdown m_PracticeRtDropdown;

    [SerializeField]
    GameObject m_PracticeImmediateGroupRoot;

    [SerializeField]
    Toggle m_PracticeImmediateToggle;

    [SerializeField]
    TMP_Dropdown m_PracticeImmediateDropdown;

    [SerializeField]
    GameObject m_PracticeFunctGroupRoot;

    [SerializeField]
    Toggle m_PracticeFunctToggle;

    [SerializeField]
    TMP_Dropdown m_PracticeFunctDropdown;

    [SerializeField]
    TMP_Text m_PracticeHintText;

    [SerializeField]
    GameObject m_OpcodeGroupRoot;

    [SerializeField]
    GameObject m_FunctGroupRoot;

    [SerializeField]
    TMP_Text m_RegisterBodyText;

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

    LearnDecodeView m_LearningView;
    PracticeDecodeView m_PracticeView;
    bool m_IsShowingPracticeDecode;

    /// <summary>
    /// Shows or hides the authored decode panel root.
    /// </summary>
    public void SetVisible(bool isVisible)
    {
        SetPanelVisible(isVisible);
    }

    /// <summary>
    /// Hides the authored decode action button when another phase owns progression.
    /// </summary>
    public void HideAction()
    {
        SetButtonState(m_ActionButton, m_ActionLabel, string.Empty, false);
        RefreshPanelLayout(m_ActionButton);
    }

    /// <summary>
    /// Rebuilds all decode dropdowns from the authored instruction catalog.
    /// </summary>
    public void PopulateDropdowns(IReadOnlyList<InstructionDefinition> availableInstructions, InstructionDefinition currentInstruction, ref bool isRefreshing)
    {
        EnsureLearningView();
        m_LearningView.PopulateDropdowns(availableInstructions, currentInstruction, ref isRefreshing);
    }

    /// <summary>
    /// Resets all decode dropdowns to their authored placeholder values.
    /// </summary>
    public void ResetDropdowns(ref bool isRefreshing)
    {
        EnsureLearningView();
        m_LearningView.ResetDropdowns(ref isRefreshing);
    }

    /// <summary>
    /// Resets the funct dropdown when opcode validation unlocks the R-type funct step.
    /// </summary>
    public void ResetFunctDropdown(ref bool isRefreshing)
    {
        EnsureLearningView();
        m_LearningView.ResetFunctDropdown(ref isRefreshing);
    }

    /// <summary>
    /// Rebuilds the authored Practice decode controls from the currently selected
    /// Practice instruction and staged opcode-confirmation state.
    /// </summary>
    public void PopulatePracticeControls(
        PracticeInstructionDefinition currentInstruction,
        bool opcodeConfirmed,
        ref bool isRefreshing)
    {
        EnsurePracticeView();
        m_PracticeView?.Refresh(currentInstruction, opcodeConfirmed, ref isRefreshing);
    }

    /// <summary>
    /// Resets the authored Practice decode controls back to their unsolved state.
    /// </summary>
    public void ResetPracticeControls(ref bool isRefreshing)
    {
        EnsurePracticeView();
        m_PracticeView?.Reset(ref isRefreshing);
    }

    /// <summary>
    /// Refreshes the authored decode panel for the currently active decode sub-step.
    /// </summary>
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

        m_IsShowingPracticeDecode =
            lessonFlow.CurrentMode == LessonMode.Practice &&
            step.highlightedNode == DatapathNodeId.InstructionMemory;

        if (m_IsShowingPracticeDecode)
        {
            EnsureLearningView();
            m_LearningView.HideAll();
            EnsurePracticeView();
            var isRefreshingPractice = false;
            m_PracticeView?.Refresh(
                currentPracticeInstruction,
                practiceDecodeFlow != null && practiceDecodeFlow.IsOpcodeConfirmed,
                ref isRefreshingPractice);
            m_PracticeView?.SetStatusText(
                practiceDecodeFlow != null
                    ? practiceDecodeFlow.GetDecodeStatusText(currentPracticeInstruction)
                    : string.Empty);

            SetButtonState(m_ActionButton, m_ActionLabel, continueLabel, true);
            RefreshPanelLayout(m_ActionButton);
            return;
        }

        EnsureLearningView();
        m_LearningView.Refresh(lessonFlow, step, availableInstructions, selectionMode);
        if (m_PracticeRoot != null)
            m_PracticeRoot.SetActive(false);

        SetButtonState(
            m_ActionButton,
            m_ActionLabel,
            step.requiredInteraction == InstructionStepInteractionType.Completion ? restartLabel : continueLabel,
            ShouldShowContinue(selectionMode, step, lessonFlow));

        RefreshPanelLayout(m_ActionButton);
    }

    /// <summary>
    /// Applies decode feedback styling and keeps the hint panel layout in sync.
    /// </summary>
    public void SetFeedback(string message, bool isFailure, IReadOnlyList<InstructionDefinition> availableInstructions)
    {
        SetFeedbackField(m_Feedback, message, isFailure);

        if (!m_IsShowingPracticeDecode)
            RefreshHintText(availableInstructions);

        RefreshPanelLayout(m_ActionButton);
    }

    /// <summary>
    /// Refreshes the currently selected hint text.
    /// </summary>
    public void RefreshHintText(IReadOnlyList<InstructionDefinition> availableInstructions)
    {
        EnsureLearningView();
        m_LearningView.RefreshHintText(availableInstructions);
        RefreshPanelLayout(m_ActionButton);
    }

    /// <summary>
    /// Returns the learner's currently selected opcode bits.
    /// </summary>
    public string GetSelectedOpcode()
    {
        EnsureLearningView();
        return m_LearningView.GetSelectedOpcode();
    }

    /// <summary>
    /// Returns the learner's currently selected funct bits.
    /// </summary>
    public string GetSelectedFunct()
    {
        EnsureLearningView();
        return m_LearningView.GetSelectedFunct();
    }

    public string GetSelectedPracticeOpcode()
    {
        EnsurePracticeView();
        return m_PracticeView != null ? m_PracticeView.GetSelectedOpcode() : string.Empty;
    }

    public string GetSelectedPracticeRs()
    {
        EnsurePracticeView();
        return m_PracticeView != null ? m_PracticeView.GetSelectedRs() : string.Empty;
    }

    public string GetSelectedPracticeRt()
    {
        EnsurePracticeView();
        return m_PracticeView != null ? m_PracticeView.GetSelectedRt() : string.Empty;
    }

    public bool GetPracticeImmediateToggleValue()
    {
        EnsurePracticeView();
        return m_PracticeView != null && m_PracticeView.GetImmediateToggleValue();
    }

    public string GetSelectedPracticeImmediate()
    {
        EnsurePracticeView();
        return m_PracticeView != null ? m_PracticeView.GetSelectedImmediate() : string.Empty;
    }

    public bool GetPracticeFunctToggleValue()
    {
        EnsurePracticeView();
        return m_PracticeView != null && m_PracticeView.GetFunctToggleValue();
    }

    public string GetSelectedPracticeFunct()
    {
        EnsurePracticeView();
        return m_PracticeView != null ? m_PracticeView.GetSelectedFunct() : string.Empty;
    }

    /// <summary>
    /// Updates the hint text shown by the authored Practice decode hint area.
    /// </summary>
    public void SetPracticeHintText(string hintText)
    {
        EnsurePracticeView();
        m_PracticeView?.SetHintText(hintText);
        RefreshPanelLayout(m_ActionButton);
    }

    void EnsureLearningView()
    {
        m_LearningView ??= new LearnDecodeView(
            m_OpcodeLessonText,
            m_RegisterLessonText,
            m_FunctLessonText,
            m_OpcodeGroupRoot,
            m_FunctGroupRoot,
            m_RegisterBodyText,
            m_OpcodeSelectionText,
            m_RegisterSelectionText,
            m_FunctSelectionText,
            m_OpcodeDropdown,
            m_FunctDropdown,
            m_HintDropdown,
            m_HintText);
    }

    void EnsurePracticeView()
    {
        m_PracticeView ??= new PracticeDecodeView(
            m_PracticeRoot,
            m_PracticeBinaryText,
            m_PracticeStatusText,
            m_PracticeOpcodeGroupRoot,
            m_PracticeOpcodeDropdown,
            m_PracticeRsGroupRoot,
            m_PracticeRsDropdown,
            m_PracticeRtGroupRoot,
            m_PracticeRtDropdown,
            m_PracticeImmediateGroupRoot,
            m_PracticeImmediateToggle,
            m_PracticeImmediateDropdown,
            m_PracticeFunctGroupRoot,
            m_PracticeFunctToggle,
            m_PracticeFunctDropdown,
            m_PracticeHintText);
    }

    /// <summary>
    /// Returns whether the current decode state should expose the continue button.
    /// </summary>
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

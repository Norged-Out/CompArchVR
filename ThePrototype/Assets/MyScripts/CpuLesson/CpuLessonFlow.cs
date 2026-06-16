using UnityEngine;

/// <summary>
/// Runtime orchestrator for the current CPU lesson slice.
/// It owns lesson state and delegates everything else to small helpers:
/// - <see cref="LessonSetup"/> prepares scene services
/// - <see cref="LessonUI"/> shows text feedback
/// - <see cref="RegisterBank"/> handles physical register buttons
/// - <see cref="LessonChecks"/> validates learner actions
/// - <see cref="CpuNodeSequenceController"/> highlights datapath nodes
/// </summary>
public class CpuLessonFlow : MonoBehaviour
{
    [SerializeField]
    InstructionDefinition m_CurrentInstruction;

    [SerializeField]
    InstructionRuntimeSelection m_RuntimeSelection = new();

    [SerializeField]
    CpuNodeSequenceController m_NodeHighlighter;

    [SerializeField]
    NodeMap m_NodeMap;

    [SerializeField]
    LessonSetup m_LessonSetup;

    [Header("Auto-Load")]

    [SerializeField]
    string m_DefaultInstructionResourcePath = "InstructionDefinitions/AddInstructionDefinition";

    [SerializeField]
    bool m_AutoLoadInstructionOnStart = true;

    [SerializeField]
    bool m_StartLessonOnPlay = true;

    int m_CurrentStepIndex = -1;
    int m_CurrentRegisterSelectionIndex;

    /// <summary>
    /// True when the lesson runtime is active and the scene advance button
    /// should route progression through this controller.
    /// </summary>
    public bool IsLessonModeActive => enabled && m_CurrentInstruction != null;

    public InstructionDefinition CurrentInstruction => m_CurrentInstruction;
    public int CurrentStepIndex => m_CurrentStepIndex;
    public InstructionRuntimeSelection RuntimeSelection => m_RuntimeSelection;

    void Awake()
    {
        m_NodeHighlighter ??= GetComponent<CpuNodeSequenceController>();
        m_NodeMap ??= GetComponent<NodeMap>();
        m_LessonSetup ??= GetComponent<LessonSetup>();

        if (m_NodeMap == null)
            m_NodeMap = gameObject.AddComponent<NodeMap>();

        if (m_LessonSetup == null)
            m_LessonSetup = gameObject.AddComponent<LessonSetup>();
    }

    void Start()
    {
        if (m_AutoLoadInstructionOnStart && m_CurrentInstruction == null)
            m_CurrentInstruction = LoadDefaultInstruction();

        if (m_CurrentInstruction == null)
            return;

        LoadInstruction(m_CurrentInstruction);

        if (m_StartLessonOnPlay)
            BeginLesson();
    }

    void OnDestroy()
    {
        if (m_LessonSetup?.registerBank != null)
            m_LessonSetup.registerBank.RegisterPressed -= HandleRegisterButtonPressed;
    }

    /// <summary>
    /// Loads a new instruction definition and rebuilds lesson-side runtime state.
    /// </summary>
    public void LoadInstruction(InstructionDefinition instruction)
    {
        m_CurrentInstruction = instruction;
        m_RuntimeSelection.definition = instruction;
        m_RuntimeSelection.ResetOperands();
        m_CurrentStepIndex = -1;
        m_CurrentRegisterSelectionIndex = 0;

        PrepareSceneForInstruction();
        ClearFeedback();
        SyncRegisterVisuals();
    }

    /// <summary>
    /// Public entry point for future instruction-selection UI.
    /// </summary>
    public void StartLesson()
    {
        BeginLesson();
    }

    /// <summary>
    /// Resets the current lesson back to the first step.
    /// </summary>
    [ContextMenu("Reset Lesson")]
    public void ResetInstructionFlow()
    {
        if (m_CurrentInstruction == null)
            m_CurrentInstruction = LoadDefaultInstruction();

        if (m_CurrentInstruction == null)
            return;

        m_RuntimeSelection.definition = m_CurrentInstruction;
        m_RuntimeSelection.ResetOperands();
        BeginLesson();
    }

    /// <summary>
    /// Triggered by the existing scene push button through
    /// <see cref="CpuNodeSequenceController.AdvanceHighlight"/>.
    /// </summary>
    public void HandleAdvanceButtonPressed()
    {
        if (!TryGetCurrentStep(out var step))
            return;

        if (!step.blockProgressUntilValidated)
        {
            AdvanceToNextStep();
            return;
        }

        switch (step.requiredInteraction)
        {
            case InstructionStepInteractionType.None:
            case InstructionStepInteractionType.ContinueButton:
                AdvanceToNextStep();
                break;

            case InstructionStepInteractionType.RegisterSelection:
                SetFeedback(GetRegisterSelectionPrompt(step), true);
                break;

            case InstructionStepInteractionType.WriteBackRegisterConfirmation:
                SetFeedback(
                    $"Use the register bank to confirm write-back to {m_CurrentInstruction.GetExpectedRegisterName(step.confirmationRegisterRole)}.",
                    true);
                break;

            case InstructionStepInteractionType.Completion:
                SetFeedback("Lesson complete. Use Reset to replay the walkthrough.", false);
                break;
        }
    }

    /// <summary>
    /// Called by <see cref="RegisterBank"/> when a register button is pressed.
    /// </summary>
    public void HandleRegisterButtonPressed(string registerName)
    {
        if (string.IsNullOrWhiteSpace(registerName) || !TryGetCurrentStep(out var step))
            return;

        switch (step.requiredInteraction)
        {
            case InstructionStepInteractionType.RegisterSelection:
                ProcessRegisterSelection(step, registerName);
                break;

            case InstructionStepInteractionType.WriteBackRegisterConfirmation:
                ProcessWriteBackConfirmation(step, registerName);
                break;

            default:
                SetFeedback("That register is not needed for the current stage.", true);
                m_LessonSetup?.registerBank?.FlashFailure(registerName);
                break;
        }
    }

    /// <summary>
    /// Called by <see cref="LessonSetup"/> when the reset button is pressed.
    /// </summary>
    public void HandleResetButtonPressed(RegisterButton _)
    {
        ResetInstructionFlow();
    }

    /// <summary>
    /// Returns the currently active flow step.
    /// </summary>
    public bool TryGetCurrentStep(out InstructionFlowStep step)
    {
        step = null;

        if (m_CurrentInstruction == null || m_CurrentInstruction.flowSteps == null)
            return false;

        if (m_CurrentStepIndex < 0 || m_CurrentStepIndex >= m_CurrentInstruction.flowSteps.Length)
            return false;

        step = m_CurrentInstruction.flowSteps[m_CurrentStepIndex];
        return step != null;
    }

    void PrepareSceneForInstruction()
    {
        if (m_CurrentInstruction == null)
            return;

        m_LessonSetup?.Prepare(this, m_CurrentInstruction);
        m_NodeMap?.RebuildRegistry();
        m_NodeHighlighter?.SetTrackedRenderers(m_NodeMap != null ? m_NodeMap.GetOrderedRenderers() : null);

        if (m_LessonSetup?.registerBank != null)
        {
            m_LessonSetup.registerBank.RegisterPressed -= HandleRegisterButtonPressed;
            m_LessonSetup.registerBank.RegisterPressed += HandleRegisterButtonPressed;
        }
    }

    void BeginLesson()
    {
        PrepareSceneForInstruction();

        if (m_CurrentInstruction == null || m_CurrentInstruction.flowSteps == null || m_CurrentInstruction.flowSteps.Length == 0)
            return;

        m_CurrentStepIndex = 0;
        m_CurrentRegisterSelectionIndex = 0;
        PresentCurrentStep();
    }

    void AdvanceToNextStep()
    {
        if (m_CurrentInstruction == null || m_CurrentInstruction.flowSteps == null)
            return;

        var nextIndex = m_CurrentStepIndex + 1;
        if (nextIndex >= m_CurrentInstruction.flowSteps.Length)
        {
            SetFeedback("Lesson complete. Use Reset to replay the walkthrough.", false);
            return;
        }

        m_CurrentStepIndex = nextIndex;
        m_CurrentRegisterSelectionIndex = 0;
        PresentCurrentStep();
    }

    void PresentCurrentStep()
    {
        if (!TryGetCurrentStep(out var step))
            return;

        if (step.advanceVisualHighlight)
            m_NodeHighlighter?.HighlightNode(step.highlightedNode);
        else
            m_NodeHighlighter?.ResetHighlight();

        m_LessonSetup?.lessonUi?.ShowStep(m_CurrentInstruction, step);
        SyncRegisterVisuals();
        ShowStepPrompt(step);
    }

    void ShowStepPrompt(InstructionFlowStep step)
    {
        switch (step.requiredInteraction)
        {
            case InstructionStepInteractionType.None:
                ClearFeedback();
                break;

            case InstructionStepInteractionType.ContinueButton:
                SetFeedback("Press the Advance button to continue.", false);
                break;

            case InstructionStepInteractionType.RegisterSelection:
                SetFeedback(GetRegisterSelectionPrompt(step), false);
                break;

            case InstructionStepInteractionType.WriteBackRegisterConfirmation:
                SetFeedback(
                    $"Press the correct register to confirm write-back to {m_CurrentInstruction.GetExpectedRegisterName(step.confirmationRegisterRole)}.",
                    false);
                break;

            case InstructionStepInteractionType.Completion:
                SetFeedback("You finished the add walkthrough. Use Reset to replay it.", false);
                break;
        }
    }

    string GetRegisterSelectionPrompt(InstructionFlowStep step)
    {
        var requiredRoles = LessonChecks.GetRequiredRoles(step);
        if (m_CurrentRegisterSelectionIndex < 0 || m_CurrentRegisterSelectionIndex >= requiredRoles.Length)
            return "Select the required registers using the physical register bank.";

        var nextRole = requiredRoles[m_CurrentRegisterSelectionIndex];
        var expectedRegister = m_CurrentInstruction.GetExpectedRegisterName(nextRole);
        return $"Use the physical register bank to choose {nextRole} ({expectedRegister}).";
    }

    void ProcessRegisterSelection(InstructionFlowStep step, string registerName)
    {
        var result = LessonChecks.ValidateRegisterSelection(
            m_CurrentInstruction,
            step,
            m_CurrentRegisterSelectionIndex,
            registerName);

        if (!result.isCorrect)
        {
            SetFeedback(
                $"Incorrect. {result.expectedRole} should be {result.expectedRegister}, not {registerName}.",
                true);
            m_LessonSetup?.registerBank?.FlashFailure(registerName);
            return;
        }

        m_RuntimeSelection.SetSelectedRegister(result.expectedRole, registerName);
        m_LessonSetup?.registerBank?.SetSelected(registerName);
        m_CurrentRegisterSelectionIndex++;

        if (result.completesStep)
        {
            SetFeedback("Correct. The register operands are ready. Moving on to the ALU stage.", false);
            AdvanceToNextStep();
            return;
        }

        SetFeedback($"Correct. Now choose {result.nextRole} ({result.nextRegister}).", false);
    }

    void ProcessWriteBackConfirmation(InstructionFlowStep step, string registerName)
    {
        var result = LessonChecks.ValidateWriteBack(m_CurrentInstruction, step, registerName);
        if (!result.isCorrect)
        {
            SetFeedback($"Incorrect. The ALU result should write back to {result.expectedRegister}.", true);
            m_LessonSetup?.registerBank?.FlashFailure(registerName);
            return;
        }

        m_RuntimeSelection.confirmedWriteBackRegister = registerName;
        m_LessonSetup?.registerBank?.SetSelected(registerName);
        SetFeedback("Correct. The result writes back to rd.", false);
        AdvanceToNextStep();
    }

    void SyncRegisterVisuals()
    {
        var bank = m_LessonSetup?.registerBank;
        if (bank == null)
            return;

        bank.ResetVisuals();

        // During operand selection, keep already-correct picks visible so the
        // learner can see progress instead of starting from a blank board.
        if (TryGetCurrentStep(out var step) &&
            step.requiredInteraction == InstructionStepInteractionType.RegisterSelection)
        {
            bank.SetSelected(m_RuntimeSelection.selectedRs);
            bank.SetSelected(m_RuntimeSelection.selectedRt);
            bank.SetSelected(m_RuntimeSelection.selectedRd);
            return;
        }

        if (!string.IsNullOrWhiteSpace(m_RuntimeSelection.confirmedWriteBackRegister))
            bank.SetSelected(m_RuntimeSelection.confirmedWriteBackRegister);
    }

    void SetFeedback(string message, bool isFailure)
    {
        m_LessonSetup?.lessonUi?.SetFeedback(message, isFailure);
    }

    void ClearFeedback()
    {
        m_LessonSetup?.lessonUi?.ClearFeedback();
    }

    InstructionDefinition LoadDefaultInstruction()
    {
        var loadedInstruction = Resources.Load<InstructionDefinition>(m_DefaultInstructionResourcePath);
        if (loadedInstruction != null)
            return loadedInstruction;

        return InstructionDefaults.CreateFallbackAdd();
    }
}

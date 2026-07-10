/// <summary>
/// Mutable runtime state for the current lesson run.
/// This keeps the transient flow state out of the scene-facing controller.
/// </summary>
sealed class LessonState
{
    /// <summary>
    /// Captures the runtime selection object that survives across lesson phases.
    /// </summary>
    public LessonState(InstructionRuntimeSelection runtimeSelection)
    {
        RuntimeSelection = runtimeSelection;
    }

    public InstructionRuntimeSelection RuntimeSelection { get; }
    public int CurrentStepIndex { get; private set; } = -1;
    public int CurrentRegisterSelectionIndex { get; private set; }
    public bool RegisterSelectionReadyToContinue { get; private set; }
    public int LastAdvanceFrame { get; private set; } = -1;
    public bool HasStarted => CurrentStepIndex >= 0;

    /// <summary>
    /// Initializes runtime state for a fresh lesson run.
    /// </summary>
    public void BeginLesson(InstructionDefinition instruction)
    {
        RuntimeSelection.definition = instruction;
        RuntimeSelection.ResetOperands();
        CurrentStepIndex = 0;
        CurrentRegisterSelectionIndex = 0;
        RegisterSelectionReadyToContinue = false;
        LastAdvanceFrame = -1;
    }

    /// <summary>
    /// Returns runtime state to its not-started values while keeping the active instruction selection.
    /// </summary>
    public void ResetLesson(InstructionDefinition instruction)
    {
        RuntimeSelection.definition = instruction;
        RuntimeSelection.ResetOperands();
        CurrentStepIndex = -1;
        CurrentRegisterSelectionIndex = 0;
        RegisterSelectionReadyToContinue = false;
        LastAdvanceFrame = -1;
    }

    /// <summary>
    /// Advances the decode register-selection pointer after a successful operand scan.
    /// </summary>
    public void AdvanceRegisterSelection()
    {
        CurrentRegisterSelectionIndex++;
    }

    /// <summary>
    /// Marks whether decode has collected every operand needed before continuing.
    /// </summary>
    public void MarkRegisterSelectionReady(bool isReady)
    {
        RegisterSelectionReadyToContinue = isReady;
    }

    /// <summary>
    /// Debounces lesson advancement so the same click cannot advance two authored steps in one frame.
    /// </summary>
    public bool TrySetAdvanceFrame(int frame)
    {
        if (LastAdvanceFrame == frame)
            return false;

        LastAdvanceFrame = frame;
        return true;
    }

    /// <summary>
    /// Moves to the next authored lesson step and clears per-step transient decode state.
    /// </summary>
    public void AdvanceStep()
    {
        CurrentStepIndex++;
        CurrentRegisterSelectionIndex = 0;
        RegisterSelectionReadyToContinue = false;
    }

    /// <summary>
    /// Jumps directly to a chosen authored step, usually after skip-rule evaluation.
    /// </summary>
    public void SkipToStep(int stepIndex)
    {
        CurrentStepIndex = stepIndex;
        CurrentRegisterSelectionIndex = 0;
        RegisterSelectionReadyToContinue = false;
    }
}

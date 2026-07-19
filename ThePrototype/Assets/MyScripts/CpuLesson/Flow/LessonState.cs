using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Mutable runtime state for the current lesson run.
/// This keeps the transient flow state out of the scene-facing controller.
/// </summary>
sealed class LessonState
{
    readonly HashSet<InstructionRegisterRole> m_SelectedRegisterRoles = new();

    /// <summary>
    /// Captures the runtime selection object that survives across lesson phases.
    /// </summary>
    public LessonState(InstructionRuntimeSelection runtimeSelection)
    {
        RuntimeSelection = runtimeSelection;
    }

    public InstructionRuntimeSelection RuntimeSelection { get; }
    public IReadOnlyCollection<InstructionRegisterRole> SelectedRegisterRoles => m_SelectedRegisterRoles;
    public int CurrentStepIndex { get; private set; } = -1;
    public int CurrentRegisterSelectionIndex => m_SelectedRegisterRoles.Count;
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
        ClearRegisterSelectionProgress();
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
        ClearRegisterSelectionProgress();
        LastAdvanceFrame = -1;
    }

    /// <summary>
    /// Marks one decode register role as satisfied.
    /// </summary>
    public void MarkRegisterRoleSelected(InstructionRegisterRole registerRole)
    {
        if (registerRole == InstructionRegisterRole.None)
            return;

        m_SelectedRegisterRoles.Add(registerRole);
    }

    /// <summary>
    /// Returns whether a specific decode register role is already satisfied.
    /// </summary>
    public bool IsRegisterRoleSelected(InstructionRegisterRole registerRole)
    {
        return m_SelectedRegisterRoles.Contains(registerRole);
    }

    /// <summary>
    /// Marks decode register collection as fully satisfied without requiring
    /// each authored scanner callback to fire one by one.
    /// </summary>
    public void ForceRegisterSelectionComplete(IEnumerable<InstructionRegisterRole> requiredRoles)
    {
        m_SelectedRegisterRoles.Clear();

        if (requiredRoles != null)
        {
            foreach (var requiredRole in requiredRoles)
            {
                if (requiredRole == InstructionRegisterRole.None)
                    continue;

                m_SelectedRegisterRoles.Add(requiredRole);
            }
        }

        RegisterSelectionReadyToContinue = true;
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
        ClearRegisterSelectionProgress();
    }

    /// <summary>
    /// Jumps directly to a chosen authored step, usually after skip-rule evaluation.
    /// </summary>
    public void SkipToStep(int stepIndex)
    {
        CurrentStepIndex = stepIndex;
        ClearRegisterSelectionProgress();
    }

    /// <summary>
    /// Clears any decode register progress for the next lesson state.
    /// </summary>
    void ClearRegisterSelectionProgress()
    {
        m_SelectedRegisterRoles.Clear();
        RegisterSelectionReadyToContinue = false;
    }
}

using System;

/// <summary>
/// Small validation helpers for lesson steps.
/// This keeps correctness checks out of the main lesson flow controller.
/// </summary>
public static class LessonChecks
{
    static readonly InstructionRegisterRole[] k_RegisterReadOrder =
    {
        InstructionRegisterRole.Rs,
        InstructionRegisterRole.Rt,
    };

    static readonly InstructionRegisterRole[] k_ImmediateRegisterReadOrder =
    {
        InstructionRegisterRole.Rs,
    };

    public readonly struct RegisterSelectionResult
    {
        public readonly bool isCorrect;
        public readonly bool completesStep;
        public readonly InstructionRegisterRole expectedRole;
        public readonly string expectedRegister;
        public readonly InstructionRegisterRole nextRole;
        public readonly string nextRegister;

        public RegisterSelectionResult(
            bool isCorrect,
            bool completesStep,
            InstructionRegisterRole expectedRole,
            string expectedRegister,
            InstructionRegisterRole nextRole,
            string nextRegister)
        {
            this.isCorrect = isCorrect;
            this.completesStep = completesStep;
            this.expectedRole = expectedRole;
            this.expectedRegister = expectedRegister;
            this.nextRole = nextRole;
            this.nextRegister = nextRegister;
        }
    }

    public readonly struct WriteBackResult
    {
        public readonly bool isCorrect;
        public readonly string expectedRegister;

        public WriteBackResult(bool isCorrect, string expectedRegister)
        {
            this.isCorrect = isCorrect;
            this.expectedRegister = expectedRegister;
        }
    }

    /// <summary>
    /// Returns the logical register order a step expects.
    /// </summary>
    public static InstructionRegisterRole[] GetRequiredRoles(InstructionDefinition instruction, InstructionFlowStep step)
    {
        if (step.requiredRegisterSelections != null && step.requiredRegisterSelections.Length > 0)
            return step.requiredRegisterSelections;

        if (instruction != null && instruction.usesImmediate)
            return k_ImmediateRegisterReadOrder;

        return k_RegisterReadOrder;
    }

    /// <summary>
    /// Validates the next register press for a register-selection step.
    /// </summary>
    public static RegisterSelectionResult ValidateRegisterSelection(
        InstructionDefinition instruction,
        InstructionFlowStep step,
        int currentSelectionIndex,
        string registerName)
    {
        var requiredRoles = GetRequiredRoles(instruction, step);
        if (currentSelectionIndex < 0 || currentSelectionIndex >= requiredRoles.Length)
        {
            return new RegisterSelectionResult(
                isCorrect: false,
                completesStep: false,
                expectedRole: InstructionRegisterRole.None,
                expectedRegister: string.Empty,
                nextRole: InstructionRegisterRole.None,
                nextRegister: string.Empty);
        }

        var expectedRole = requiredRoles[currentSelectionIndex];
        var expectedRegister = instruction.GetExpectedRegisterName(expectedRole);
        var isCorrect = string.Equals(expectedRegister, registerName, StringComparison.OrdinalIgnoreCase);

        if (!isCorrect)
        {
            return new RegisterSelectionResult(
                isCorrect: false,
                completesStep: false,
                expectedRole: expectedRole,
                expectedRegister: expectedRegister,
                nextRole: expectedRole,
                nextRegister: expectedRegister);
        }

        var nextIndex = currentSelectionIndex + 1;
        var completesStep = nextIndex >= requiredRoles.Length;
        var nextRole = completesStep ? InstructionRegisterRole.None : requiredRoles[nextIndex];
        var nextRegister = completesStep ? string.Empty : instruction.GetExpectedRegisterName(nextRole);

        return new RegisterSelectionResult(
            isCorrect: true,
            completesStep: completesStep,
            expectedRole: expectedRole,
            expectedRegister: expectedRegister,
            nextRole: nextRole,
            nextRegister: nextRegister);
    }

    /// <summary>
    /// Validates the final write-back confirmation register.
    /// </summary>
    public static WriteBackResult ValidateWriteBack(
        InstructionDefinition instruction,
        InstructionFlowStep step,
        string registerName)
    {
        var expectedRegister = instruction.GetExpectedRegisterName(step.confirmationRegisterRole);
        var isCorrect = string.Equals(expectedRegister, registerName, StringComparison.OrdinalIgnoreCase);
        return new WriteBackResult(isCorrect, expectedRegister);
    }
}

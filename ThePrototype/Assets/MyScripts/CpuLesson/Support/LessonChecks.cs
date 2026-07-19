using System;
using System.Collections.Generic;

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
        public readonly InstructionRegisterRole matchedRole;
        public readonly string matchedRegister;
        public readonly InstructionRegisterRole nextRole;
        public readonly string nextRegister;

        /// <summary>
        /// Captures both the validation result and the next scanner target for the decode phase.
        /// </summary>
        public RegisterSelectionResult(
            bool isCorrect,
            bool completesStep,
            InstructionRegisterRole matchedRole,
            string matchedRegister,
            InstructionRegisterRole nextRole,
            string nextRegister)
        {
            this.isCorrect = isCorrect;
            this.completesStep = completesStep;
            this.matchedRole = matchedRole;
            this.matchedRegister = matchedRegister;
            this.nextRole = nextRole;
            this.nextRegister = nextRegister;
        }
    }

    /// <summary>
    /// Returns the logical register order a step expects.
    /// </summary>
    public static InstructionRegisterRole[] GetRequiredRoles(InstructionDefinition instruction, InstructionFlowStep step)
    {
        if (step.requiredRegisterSelections != null && step.requiredRegisterSelections.Length > 0)
            return step.requiredRegisterSelections;

        if (instruction != null && instruction.usesImmediate && instruction.mnemonic != InstructionMnemonic.Sw)
            return k_ImmediateRegisterReadOrder;

        return k_RegisterReadOrder;
    }

    /// <summary>
    /// Validates the next register press for a register-selection step.
    /// </summary>
    public static RegisterSelectionResult ValidateRegisterSelection(
        InstructionDefinition instruction,
        InstructionFlowStep step,
        IReadOnlyCollection<InstructionRegisterRole> completedRoles,
        InstructionRegisterRole scannedRole,
        string registerName)
    {
        var requiredRoles = GetRequiredRoles(instruction, step);
        if (instruction == null || requiredRoles == null || requiredRoles.Length == 0)
        {
            return new RegisterSelectionResult(
                isCorrect: false,
                completesStep: false,
                matchedRole: InstructionRegisterRole.None,
                matchedRegister: string.Empty,
                nextRole: InstructionRegisterRole.None,
                nextRegister: string.Empty);
        }

        var remainingRoles = new List<InstructionRegisterRole>();
        foreach (var requiredRole in requiredRoles)
        {
            if (ContainsRole(completedRoles, requiredRole))
                continue;

            remainingRoles.Add(requiredRole);
        }

        if (remainingRoles.Count == 0)
        {
            return new RegisterSelectionResult(
                isCorrect: false,
                completesStep: true,
                matchedRole: InstructionRegisterRole.None,
                matchedRegister: string.Empty,
                nextRole: InstructionRegisterRole.None,
                nextRegister: string.Empty);
        }

        var matchedRole = ResolveMatchedRole(instruction, remainingRoles, scannedRole, registerName);
        if (matchedRole == InstructionRegisterRole.None)
        {
            var fallbackRole = scannedRole != InstructionRegisterRole.None && Array.IndexOf(requiredRoles, scannedRole) >= 0
                ? scannedRole
                : remainingRoles[0];

            return new RegisterSelectionResult(
                isCorrect: false,
                completesStep: false,
                matchedRole: fallbackRole,
                matchedRegister: instruction.GetExpectedRegisterName(fallbackRole),
                nextRole: remainingRoles[0],
                nextRegister: instruction.GetExpectedRegisterName(remainingRoles[0]));
        }

        var nextRole = InstructionRegisterRole.None;
        var nextRegister = string.Empty;
        foreach (var remainingRole in remainingRoles)
        {
            if (remainingRole == matchedRole)
                continue;

            nextRole = remainingRole;
            nextRegister = instruction.GetExpectedRegisterName(remainingRole);
            break;
        }

        var completesStep = remainingRoles.Count == 1;

        return new RegisterSelectionResult(
            isCorrect: true,
            completesStep: completesStep,
            matchedRole: matchedRole,
            matchedRegister: instruction.GetExpectedRegisterName(matchedRole),
            nextRole: nextRole,
            nextRegister: nextRegister);
    }

    static InstructionRegisterRole ResolveMatchedRole(
        InstructionDefinition instruction,
        IReadOnlyList<InstructionRegisterRole> remainingRoles,
        InstructionRegisterRole scannedRole,
        string registerName)
    {
        if (instruction == null || remainingRoles == null || string.IsNullOrWhiteSpace(registerName))
            return InstructionRegisterRole.None;

        if (scannedRole != InstructionRegisterRole.None)
        {
            if (!ContainsRole(remainingRoles, scannedRole))
                return InstructionRegisterRole.None;

            var expectedRegister = instruction.GetExpectedRegisterName(scannedRole);
            return string.Equals(expectedRegister, registerName, StringComparison.OrdinalIgnoreCase)
                ? scannedRole
                : InstructionRegisterRole.None;
        }

        foreach (var remainingRole in remainingRoles)
        {
            var expectedRegister = instruction.GetExpectedRegisterName(remainingRole);
            if (string.Equals(expectedRegister, registerName, StringComparison.OrdinalIgnoreCase))
                return remainingRole;
        }

        return InstructionRegisterRole.None;
    }

    static bool ContainsRole(IEnumerable<InstructionRegisterRole> roles, InstructionRegisterRole targetRole)
    {
        if (roles == null)
            return false;

        foreach (var role in roles)
        {
            if (role == targetRole)
                return true;
        }

        return false;
    }
}

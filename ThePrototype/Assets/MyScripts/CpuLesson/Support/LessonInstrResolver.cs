using UnityEngine;

/// <summary>
/// Centralizes lesson-mode instruction policy so the scene-facing flow root does
/// not keep accumulating mode-specific fetch/display/start rules.
/// </summary>
public static class LessonInstrResolver
{
    /// <summary>
    /// Returns whether the current mode should expose the second intro dropdown.
    /// </summary>
    public static bool UsesInstructionSelection(LessonMode mode)
    {
        return LessonModePolicy.UsesInstructionSelection(mode);
    }

    /// <summary>
    /// Returns whether the currently selected mode has enough authored data to
    /// begin a lesson safely.
    /// </summary>
    public static bool CanStart(
        LessonMode mode,
        InstructionDefinition learningInstruction,
        PracticeInstructionDefinition practiceInstruction,
        InstructionCatalog instructionCatalog)
    {
        return mode switch
        {
            LessonMode.Learning => learningInstruction != null ||
                                   instructionCatalog != null && instructionCatalog.GetDefaultLearningInstruction() != null,
            LessonMode.Practice => HasRuntimeTemplate(practiceInstruction) ||
                                   HasRuntimeTemplate(instructionCatalog != null ? instructionCatalog.GetDefaultPracticeInstruction(LessonMode.Practice) : null),
            LessonMode.Test => HasRuntimeTemplate(instructionCatalog != null ? instructionCatalog.GetDefaultPracticeInstruction(LessonMode.Test) : null),
            _ => false,
        };
    }

    /// <summary>
    /// Resolves which guided instruction should actually drive the shared lesson
    /// runtime for the currently selected top-level mode.
    /// </summary>
    public static InstructionDefinition ResolveRuntimeInstruction(
        LessonMode mode,
        InstructionDefinition learningInstruction,
        PracticeInstructionDefinition practiceInstruction)
    {
        return LessonModePolicy.IsAssessmentMode(mode)
            ? practiceInstruction != null ? practiceInstruction.CreateRuntimeInstruction() : null
            : learningInstruction;
    }

    /// <summary>
    /// Chooses the text that should appear on the physical instruction module
    /// during fetch.
    /// </summary>
    public static string GetFetchDisplayText(
        LessonMode mode,
        InstructionDefinition learningInstruction,
        PracticeInstructionDefinition practiceInstruction)
    {
        if (LessonModePolicy.IsAssessmentMode(mode) && practiceInstruction != null)
            return practiceInstruction.GetHexInstructionText();

        return learningInstruction != null ? learningInstruction.assemblyInstructionText : string.Empty;
    }

    static bool HasRuntimeTemplate(PracticeInstructionDefinition instruction)
    {
        return instruction != null && instruction.learningModeInstruction != null;
    }
}

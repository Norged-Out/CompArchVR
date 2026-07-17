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
        return mode == LessonMode.Learning || mode == LessonMode.Practice;
    }

    /// <summary>
    /// Returns whether the currently selected mode has enough authored data to
    /// begin a lesson safely.
    /// </summary>
    public static bool CanStart(
        LessonMode mode,
        InstructionDefinition learningInstruction,
        PracticeInstructionDefinition practiceInstruction)
    {
        return mode switch
        {
            LessonMode.Learning => learningInstruction != null,
            LessonMode.Practice => practiceInstruction != null && practiceInstruction.learningModeInstruction != null,
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
        return mode == LessonMode.Practice
            ? practiceInstruction != null ? practiceInstruction.learningModeInstruction : null
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
        if (mode == LessonMode.Practice && practiceInstruction != null)
            return practiceInstruction.GetHexInstructionText();

        return learningInstruction != null ? learningInstruction.assemblyInstructionText : string.Empty;
    }

    /// <summary>
    /// Loads the default guided instruction from Resources.
    /// </summary>
    public static InstructionDefinition LoadDefaultLearning(string resourcesPath)
    {
        var loadedInstruction = Resources.Load<InstructionDefinition>(resourcesPath);
        return loadedInstruction != null ? loadedInstruction : InstructionDefaults.CreateFallbackAdd();
    }

    /// <summary>
    /// Loads the default practice instruction from Resources.
    /// </summary>
    public static PracticeInstructionDefinition LoadDefaultPractice(string resourcesPath)
    {
        return Resources.Load<PracticeInstructionDefinition>(resourcesPath);
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Loads and sorts the authored instruction definition assets used by the lesson UI.
/// </summary>
public sealed class InstructionCatalog
{
    const string k_LearningInstructionResourcesPath = "InstructionDefinitions";
    const string k_PracticeInstructionResourcesPath = "PracticeInstructionDefinitions";

    /// <summary>
    /// Builds the visible Learning instruction list from the Resources folder and
    /// keeps the currently selected instruction available even if the asset list
    /// is empty.
    /// </summary>
    public List<InstructionDefinition> LoadAvailable(LessonMode mode, InstructionDefinition currentInstruction)
    {
        if (mode != LessonMode.Learning)
            return new List<InstructionDefinition>();

        var availableInstructions = new List<InstructionDefinition>();
        var loadedInstructions = Resources.LoadAll<InstructionDefinition>(k_LearningInstructionResourcesPath);

        if (loadedInstructions != null && loadedInstructions.Length > 0)
        {
            availableInstructions.AddRange(loadedInstructions);
            availableInstructions.Sort(CompareInstructionsByDisplayName);
        }

        if (availableInstructions.Count == 0 && currentInstruction != null)
            availableInstructions.Add(currentInstruction);

        return availableInstructions;
    }

    /// <summary>
    /// Builds the visible Practice instruction list from its separate Resources
    /// folder so encoded-instruction assets do not pollute guided lesson content.
    /// </summary>
    public List<PracticeInstructionDefinition> LoadPracticeAvailable(PracticeInstructionDefinition currentInstruction)
    {
        var availableInstructions = new List<PracticeInstructionDefinition>();
        var loadedInstructions = Resources.LoadAll<PracticeInstructionDefinition>(k_PracticeInstructionResourcesPath);

        if (loadedInstructions != null && loadedInstructions.Length > 0)
        {
            availableInstructions.AddRange(loadedInstructions);
            availableInstructions.Sort(ComparePracticeInstructionsByDisplayName);
        }

        if (availableInstructions.Count == 0 && currentInstruction != null)
            availableInstructions.Add(currentInstruction);

        return availableInstructions;
    }

    /// <summary>
    /// Keeps dropdown ordering human-readable and deterministic.
    /// </summary>
    static int CompareInstructionsByDisplayName(InstructionDefinition left, InstructionDefinition right)
    {
        return string.Compare(
            left != null ? left.displayName : string.Empty,
            right != null ? right.displayName : string.Empty,
            StringComparison.OrdinalIgnoreCase);
    }

    static int ComparePracticeInstructionsByDisplayName(PracticeInstructionDefinition left, PracticeInstructionDefinition right)
    {
        return string.Compare(
            left != null ? left.displayName : string.Empty,
            right != null ? right.displayName : string.Empty,
            StringComparison.OrdinalIgnoreCase);
    }
}

/// <summary>
/// Describes which authored decode sub-step is currently active inside the shared
/// decode panel.
/// </summary>
public enum DecodeSelectionMode
{
    None,
    Opcode,
    Funct,
    Registers,
}

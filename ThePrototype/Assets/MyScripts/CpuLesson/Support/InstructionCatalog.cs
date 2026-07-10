using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Loads and sorts the authored instruction definition assets used by the lesson UI.
/// </summary>
public sealed class InstructionCatalog
{
    const string k_InstructionResourcesPath = "InstructionDefinitions";

    /// <summary>
    /// Builds the visible instruction list from the Resources folder and keeps the
    /// currently selected instruction available even if the asset list is empty.
    /// </summary>
    public List<InstructionDefinition> LoadAvailable(InstructionDefinition currentInstruction)
    {
        var availableInstructions = new List<InstructionDefinition>();
        var loadedInstructions = Resources.LoadAll<InstructionDefinition>(k_InstructionResourcesPath);

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
    /// Keeps dropdown ordering human-readable and deterministic.
    /// </summary>
    static int CompareInstructionsByDisplayName(InstructionDefinition left, InstructionDefinition right)
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

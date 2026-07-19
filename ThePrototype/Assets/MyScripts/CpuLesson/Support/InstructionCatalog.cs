using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Authored bookkeeping asset for every instruction the lesson flow can expose.
/// This replaces folder-wide resource discovery with one explicit source of truth
/// that controls ordering and future mode expansion.
/// </summary>
[CreateAssetMenu(fileName = "LessonInstructionCatalog", menuName = "CPU Lesson/Instruction Catalog")]
public sealed class InstructionCatalog : ScriptableObject
{
    const string k_DefaultCatalogResourcesPath = "LessonInstructionCatalog";

    [SerializeField]
    List<InstructionDefinition> m_LearningInstructions = new();

    [SerializeField]
    List<PracticeInstructionDefinition> m_PracticeInstructions = new();

    [SerializeField]
    List<PracticeInstructionDefinition> m_TestInstructions = new();

    /// <summary>
    /// Loads the default catalog asset when a scene has not assigned one explicitly.
    /// </summary>
    public static InstructionCatalog LoadDefaultCatalog()
    {
        return Resources.Load<InstructionCatalog>(k_DefaultCatalogResourcesPath);
    }

    /// <summary>
    /// Returns the authored Learning instruction bank in editor-defined order.
    /// </summary>
    public List<InstructionDefinition> GetLearningInstructions(InstructionDefinition currentInstruction)
    {
        return BuildInstructionList(m_LearningInstructions, currentInstruction);
    }

    /// <summary>
    /// Returns the authored Practice or Test instruction bank. Test falls back
    /// to the Practice pool until it gains its own dedicated authored list.
    /// </summary>
    public List<PracticeInstructionDefinition> GetModePracticeInstructions(LessonMode mode, PracticeInstructionDefinition currentInstruction)
    {
        var sourceList = mode == LessonMode.Test && HasAuthoredEntries(m_TestInstructions)
            ? m_TestInstructions
            : m_PracticeInstructions;

        return BuildInstructionList(sourceList, currentInstruction);
    }

    /// <summary>
    /// Returns the first valid Learning instruction in the authored bank.
    /// </summary>
    public InstructionDefinition GetDefaultLearningInstruction()
    {
        var availableInstructions = GetLearningInstructions(null);
        return availableInstructions.Count > 0 ? availableInstructions[0] : InstructionDefaults.CreateFallbackAdd();
    }

    /// <summary>
    /// Returns the first valid Practice/Test instruction in the authored bank.
    /// </summary>
    public PracticeInstructionDefinition GetDefaultPracticeInstruction(LessonMode mode)
    {
        var availableInstructions = GetModePracticeInstructions(mode, null);
        return availableInstructions.Count > 0 ? availableInstructions[0] : null;
    }

    static List<TInstruction> BuildInstructionList<TInstruction>(
        IReadOnlyList<TInstruction> sourceList,
        TInstruction currentInstruction)
        where TInstruction : Object
    {
        var availableInstructions = new List<TInstruction>();

        if (sourceList != null)
        {
            for (var index = 0; index < sourceList.Count; index++)
            {
                var instruction = sourceList[index];
                if (instruction == null || availableInstructions.Contains(instruction))
                    continue;

                availableInstructions.Add(instruction);
            }
        }

        if (availableInstructions.Count == 0 && currentInstruction != null)
            availableInstructions.Add(currentInstruction);

        return availableInstructions;
    }

    static bool HasAuthoredEntries<TInstruction>(IReadOnlyList<TInstruction> sourceList)
        where TInstruction : Object
    {
        if (sourceList == null)
            return false;

        for (var index = 0; index < sourceList.Count; index++)
        {
            if (sourceList[index] != null)
                return true;
        }

        return false;
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

using System.Collections.Generic;

/// <summary>
/// Owns the selectable lesson-content lists shown on the intro panel so the
/// guide controller can stay focused on scene orchestration.
/// </summary>
public sealed class GuideSelState
{
    readonly InstructionCatalog m_Catalog = new();
    readonly List<InstructionDefinition> m_LearningInstructions = new();
    readonly List<PracticeInstructionDefinition> m_PracticeInstructions = new();
    readonly List<string> m_IntroLabels = new();

    public IReadOnlyList<InstructionDefinition> LearningInstructions => m_LearningInstructions;
    public IReadOnlyList<PracticeInstructionDefinition> PracticeInstructions => m_PracticeInstructions;
    public IReadOnlyList<string> IntroLabels => m_IntroLabels;
    public int CurrentIntroSelectionIndex { get; private set; }

    /// <summary>
    /// Rebuilds the scene-visible content lists from the currently selected mode.
    /// </summary>
    public void Refresh(CpuLessonFlow lessonFlow)
    {
        if (lessonFlow == null)
            return;

        m_LearningInstructions.Clear();
        m_LearningInstructions.AddRange(m_Catalog.LoadAvailable(LessonMode.Learning, lessonFlow.CurrentInstruction));

        m_PracticeInstructions.Clear();
        m_PracticeInstructions.AddRange(m_Catalog.LoadPracticeAvailable(lessonFlow.CurrentPracticeInstruction));

        BuildIntroLabels(lessonFlow);
    }

    /// <summary>
    /// Applies the learner's current intro-dropdown choice back into the flow.
    /// </summary>
    public bool TryApplySelection(CpuLessonFlow lessonFlow, int selectedIndex)
    {
        if (lessonFlow == null)
            return false;

        switch (lessonFlow.CurrentMode)
        {
            case LessonMode.Learning:
                if (selectedIndex < 0 || selectedIndex >= m_LearningInstructions.Count)
                    return false;

                lessonFlow.SetCurrentInstruction(m_LearningInstructions[selectedIndex]);
                return true;

            case LessonMode.Practice:
            case LessonMode.Test:
                if (selectedIndex < 0 || selectedIndex >= m_PracticeInstructions.Count)
                    return false;

                lessonFlow.SetCurrentPracticeInstruction(m_PracticeInstructions[selectedIndex]);
                return true;

            default:
                return false;
        }
    }

    void BuildIntroLabels(CpuLessonFlow lessonFlow)
    {
        m_IntroLabels.Clear();
        CurrentIntroSelectionIndex = 0;

        if (lessonFlow.CurrentMode == LessonMode.Learning)
        {
            for (var index = 0; index < m_LearningInstructions.Count; index++)
            {
                var instruction = m_LearningInstructions[index];
                m_IntroLabels.Add(instruction != null ? instruction.displayName : "Instruction");

                if (instruction == lessonFlow.CurrentInstruction)
                    CurrentIntroSelectionIndex = index;
            }

            return;
        }

        for (var index = 0; index < m_PracticeInstructions.Count; index++)
        {
            var instruction = m_PracticeInstructions[index];
            m_IntroLabels.Add(instruction != null ? instruction.displayName : "Practice Instruction");

            if (instruction == lessonFlow.CurrentPracticeInstruction)
                CurrentIntroSelectionIndex = index;
        }
    }
}

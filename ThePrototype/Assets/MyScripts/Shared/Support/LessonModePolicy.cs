/// <summary>
/// Shared lesson-mode policy so controllers can ask simple questions about
/// behavior instead of duplicating mode checks throughout the project.
/// </summary>
public static class LessonModePolicy
{
    public static bool UsesInstructionSelection(LessonMode mode)
    {
        return mode == LessonMode.Learning || mode == LessonMode.Practice;
    }

    public static bool IsAssessmentMode(LessonMode mode)
    {
        return mode == LessonMode.Practice || mode == LessonMode.Test;
    }

    public static bool UsesLessonPanel(LessonMode mode)
    {
        return mode != LessonMode.Test;
    }

    public static bool UsesHintPanel(LessonMode mode)
    {
        return mode != LessonMode.Test;
    }

    public static int ResolveValidationAttempts(LessonMode mode, int practiceAttempts)
    {
        return mode == LessonMode.Test ? 1 : practiceAttempts;
    }

    public static int ResolveScannerAttempts(LessonMode mode, int practiceAttempts)
    {
        return mode == LessonMode.Test ? 1 : practiceAttempts;
    }

    public static int ResolveHintAttempts(LessonMode mode, int practiceHints)
    {
        return mode == LessonMode.Test ? 0 : practiceHints;
    }
}

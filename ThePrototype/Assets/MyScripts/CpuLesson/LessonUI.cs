using TMPro;
using UnityEngine;

/// <summary>
/// Thin presentation wrapper for the lesson's world-space text UI.
/// Scene-authored references are preferred; runtime fallback references can be
/// bound by <see cref="LessonSetup"/> when the scene has no authored UI yet.
/// </summary>
public class LessonUI : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI m_InstructionNameText;

    [SerializeField]
    TextMeshProUGUI m_StageNameText;

    [SerializeField]
    TextMeshProUGUI m_ExplanationText;

    [SerializeField]
    TextMeshProUGUI m_FeedbackText;

    /// <summary>
    /// True when the UI already has the minimum text references it needs.
    /// </summary>
    public bool HasTextReferences =>
        m_InstructionNameText != null &&
        m_StageNameText != null &&
        m_ExplanationText != null &&
        m_FeedbackText != null;

    /// <summary>
    /// Binds scene-created or runtime-created text references.
    /// </summary>
    public void Bind(
        TextMeshProUGUI instructionNameText,
        TextMeshProUGUI stageNameText,
        TextMeshProUGUI explanationText,
        TextMeshProUGUI feedbackText)
    {
        m_InstructionNameText = instructionNameText;
        m_StageNameText = stageNameText;
        m_ExplanationText = explanationText;
        m_FeedbackText = feedbackText;
    }

    /// <summary>
    /// Shows the currently active instruction and step.
    /// </summary>
    public void ShowStep(InstructionDefinition instruction, InstructionFlowStep step)
    {
        if (!HasTextReferences || instruction == null || step == null)
            return;

        m_InstructionNameText.text = $"Instruction: {instruction.assemblyInstructionText}";
        m_StageNameText.text = $"Stage: {step.stepName}";
        m_ExplanationText.text = BuildExplanationText(instruction, step);
    }

    /// <summary>
    /// Updates the feedback text with a success or failure color.
    /// </summary>
    public void SetFeedback(string message, bool isFailure)
    {
        if (m_FeedbackText == null)
            return;

        m_FeedbackText.text = message;
        m_FeedbackText.color = isFailure
            ? new Color(1f, 0.55f, 0.55f, 1f)
            : new Color(0.78f, 0.96f, 0.82f, 1f);
    }

    /// <summary>
    /// Clears the feedback message.
    /// </summary>
    public void ClearFeedback()
    {
        if (m_FeedbackText == null)
            return;

        m_FeedbackText.text = string.Empty;
        m_FeedbackText.color = Color.white;
    }

    static string BuildExplanationText(InstructionDefinition instruction, InstructionFlowStep step)
    {
        var explanation = step.explanation ?? string.Empty;

        if (step.highlightedNode == DatapathNodeId.InstructionMemory &&
            !string.IsNullOrWhiteSpace(instruction.fieldBreakdownText))
        {
            explanation += $"\n\nInstruction fields:\n{instruction.fieldBreakdownText}";
        }

        return explanation;
    }
}

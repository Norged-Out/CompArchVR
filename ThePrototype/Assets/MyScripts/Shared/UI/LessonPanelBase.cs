using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Shared UI helpers for authored lesson panels.
/// Each panel binds its authored scroll views and root rects through arrays so
/// both single-panel and multi-panel lesson layouts can be handled uniformly.
/// </summary>
public abstract class LessonPanelBase : MonoBehaviour
{
    const float k_ActionButtonHeight = 56f;

    [Header("Audio")]
    [SerializeField]
    LessonUiAudioCueSet m_LessonAudioCues = new();

    [Header("Shared Layout")]
    [SerializeField]
    ScrollRect[] m_ScrollRects;

    [SerializeField]
    RectTransform[] m_RootRects;

    /// <summary>
    /// Shows or hides the entire authored panel root.
    /// </summary>
    protected void SetPanelVisible(bool isVisible)
    {
        gameObject.SetActive(isVisible);
    }

    /// <summary>
    /// Writes text into a field and hides the field when the content is empty.
    /// </summary>
    protected static void SetTextField(TMP_Text target, string text)
    {
        if (target == null)
            return;

        target.text = text;
        target.gameObject.SetActive(!string.IsNullOrWhiteSpace(text));
    }

    /// <summary>
    /// Shows or hides a text field without modifying its current content.
    /// </summary>
    protected static void SetTextFieldActive(TMP_Text target, bool isActive)
    {
        if (target == null)
            return;

        target.gameObject.SetActive(isActive);
    }

    /// <summary>
    /// Shows or hides a authored scene object without changing any child content.
    /// </summary>
    protected static void SetObjectActive(GameObject target, bool isActive)
    {
        if (target == null)
            return;

        target.SetActive(isActive);
    }

    /// <summary>
    /// Applies the shared lesson feedback palette and visibility rules.
    /// </summary>
    protected static void SetFeedbackField(TMP_Text target, string message, bool isFailure)
    {
        if (target == null)
            return;

        target.text = message;
        target.color = isFailure
            ? new Color(1f, 0.55f, 0.55f, 1f)
            : new Color(0.78f, 0.96f, 0.82f, 1f);
        target.gameObject.SetActive(!string.IsNullOrWhiteSpace(message));
    }

    /// <summary>
    /// Updates button visibility, interactability, and authored label text together.
    /// </summary>
    protected static void SetButtonState(Button button, TMP_Text label, string labelText, bool visibleAndEnabled)
    {
        if (button == null)
            return;

        button.gameObject.SetActive(visibleAndEnabled);
        button.interactable = visibleAndEnabled;

        if (label != null)
            label.text = labelText;
    }

    /// <summary>
    /// Forces all authored scroll contents to rebuild after runtime text changes.
    /// </summary>
    protected void RefreshPanelLayout(Button primaryActionButton)
    {
        if (!gameObject.activeInHierarchy)
            return;

        EnsureButtonLayout(primaryActionButton);
        Canvas.ForceUpdateCanvases();

        RefreshScrollLayouts();

        RefreshRootLayouts();

        Canvas.ForceUpdateCanvases();
    }

    /// <summary>
    /// Plays the authored phase-entered cue from this panel's local audio source.
    /// </summary>
    public void PlayPhaseActivatedCue()
    {
        m_LessonAudioCues.PlayPhaseActivatedCue();
    }

    /// <summary>
    /// Plays the authored phase-cleared cue from this panel's local audio source.
    /// </summary>
    public void PlayPhaseCompletedCue()
    {
        m_LessonAudioCues.PlayPhaseCompletedCue();
    }

    /// <summary>
    /// Plays the authored incorrect-action cue from this panel's local audio source.
    /// </summary>
    public void PlayIncorrectCue()
    {
        m_LessonAudioCues.PlayIncorrectCue();
    }

    /// <summary>
    /// Plays the authored lesson-finished cue from this panel's local audio source.
    /// </summary>
    public void PlayLessonCompletedCue()
    {
        m_LessonAudioCues.PlayLessonCompletedCue();
    }

    /// <summary>
    /// Ensures authored action buttons reserve enough height for their runtime label.
    /// </summary>
    static void EnsureButtonLayout(Button button)
    {
        if (button == null)
            return;

        var layoutElement = button.GetComponent<LayoutElement>();
        if (layoutElement == null)
            layoutElement = button.gameObject.AddComponent<LayoutElement>();

        if (layoutElement.preferredHeight <= 0f)
            layoutElement.preferredHeight = k_ActionButtonHeight;

        if (layoutElement.minHeight <= 0f)
            layoutElement.minHeight = k_ActionButtonHeight;
    }

    /// <summary>
    /// Rebuilds every authored scroll view assigned to this panel.
    /// </summary>
    void RefreshScrollLayouts()
    {
        var seen = new HashSet<ScrollRect>();
        if (m_ScrollRects == null)
            return;

        foreach (var scrollRect in m_ScrollRects)
            RefreshScrollLayout(scrollRect, seen);
    }

    /// <summary>
    /// Rebuilds one authored scroll view if it has not already been processed.
    /// </summary>
    static void RefreshScrollLayout(ScrollRect scrollRect, HashSet<ScrollRect> seen)
    {
        if (scrollRect == null || !seen.Add(scrollRect))
            return;

        if (scrollRect.content != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content);

        if (scrollRect.viewport != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.viewport);
    }

    /// <summary>
    /// Rebuilds every authored root rect assigned to this panel.
    /// </summary>
    void RefreshRootLayouts()
    {
        var seen = new HashSet<RectTransform>();
        if (m_RootRects == null)
            return;

        foreach (var rootRect in m_RootRects)
            RefreshRootLayout(rootRect, seen);
    }

    /// <summary>
    /// Rebuilds one authored root rect if it has not already been processed.
    /// </summary>
    static void RefreshRootLayout(RectTransform rootRect, HashSet<RectTransform> seen)
    {
        if (rootRect == null || !seen.Add(rootRect))
            return;

        LayoutRebuilder.ForceRebuildLayoutImmediate(rootRect);
    }
}

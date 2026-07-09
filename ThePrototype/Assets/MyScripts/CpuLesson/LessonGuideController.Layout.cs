using TMPro;
using UnityEngine;
using UnityEngine.UI;

public partial class LessonGuideController
{
    static void SetText(TMP_Text target, string text)
    {
        if (target == null)
            return;

        target.text = text;
        target.gameObject.SetActive(!string.IsNullOrWhiteSpace(text));
    }

    static void SetActive(TMP_Text target, bool isActive)
    {
        if (target == null)
            return;

        target.gameObject.SetActive(isActive);
    }

    static void SetButtonState(Button button, TMP_Text label, string labelText, bool visibleAndEnabled)
    {
        if (button == null)
            return;

        button.gameObject.SetActive(visibleAndEnabled);
        button.interactable = visibleAndEnabled;

        if (label != null)
            label.text = labelText;
    }

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

    static void RefreshLayout(GameObject root, TMP_Text body, TMP_Text feedback, Button actionButton)
    {
        if (root == null || !root.activeInHierarchy)
            return;

        foreach (var textMesh in root.GetComponentsInChildren<TMP_Text>(true))
            textMesh?.ForceMeshUpdate();

        EnsureButtonLayout(actionButton);
        Canvas.ForceUpdateCanvases();

        // Each lesson panel is authored in-scene. Runtime only refreshes layout after
        // text/button changes so scroll content stays readable and doesn't overlap.
        var scrollRect = root.GetComponentInChildren<ScrollRect>(true);
        if (scrollRect != null && scrollRect.content != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content);

            if (scrollRect.viewport != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.viewport);
        }

        var rootRect = root.GetComponent<RectTransform>();
        if (rootRect != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(rootRect);

        Canvas.ForceUpdateCanvases();
    }
}

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Presentation-only helper for the authored Mem UI.
/// </summary>
public static class MemoryPresentation
{
    /// <summary>
    /// Rebuilds all memory-phase UI text and hint visibility.
    /// </summary>
    public static void Refresh(MemoryController controller, MemoryTransferService transferService)
    {
        if (controller == null || transferService == null)
            return;

        transferService.RefreshExpectedTargets(controller);

        SetObjectActive(controller.LessonPanelRoot, LessonModePolicy.UsesLessonPanel(controller.CurrentMode));
        SetObjectActive(controller.HintPanelRoot, LessonModePolicy.UsesHintPanel(controller.CurrentMode));

        if (controller.LessonRuntimeText != null)
        {
            if (controller.IsAssessmentMode)
                controller.LessonRuntimeText.text = "Assessment Mode\nComplete the Memory phase using the instruction you decoded earlier.";
            else
            {
                var instructionName = controller.CurrentInstruction != null ? controller.CurrentInstruction.displayName : "instruction";
                var assembly = controller.CurrentInstruction != null ? controller.CurrentInstruction.assemblyInstructionText : "lw t1, 8(t0)";
                controller.LessonRuntimeText.text = $"Instruction: {instructionName}\nAssembly: {assembly}";
            }
        }

        if (controller.LoadLessonText != null)
            controller.LoadLessonText.gameObject.SetActive(!controller.IsAssessmentMode && transferService.IsLoadInstruction(controller.CurrentInstruction));

        if (controller.StoreLessonText != null)
            controller.StoreLessonText.gameObject.SetActive(!controller.IsAssessmentMode && transferService.IsStoreInstruction(controller.CurrentInstruction));

        if (controller.MemReadStatusText != null)
            controller.MemReadStatusText.text = $"MemRead: {controller.MemReadValue}";

        if (controller.MemWriteStatusText != null)
            controller.MemWriteStatusText.text = $"MemWrite: {controller.MemWriteValue}";

        if (controller.AddressStatusText != null)
            controller.AddressStatusText.text = BuildAddressStatusText(controller, transferService);

        if (controller.DataStatusText != null)
            controller.DataStatusText.text = BuildDataStatusText(controller, transferService);

        if (controller.ActionButtonLabel != null)
        {
            controller.ActionButtonLabel.text = controller.IsPracticeAwaitingReset
                ? "Restart"
                : transferService.UsesInteractiveMemory(controller.CurrentInstruction)
                ? controller.IsAwaitingContinue ? controller.ContinueButtonText : controller.ExecuteButtonText
                : controller.ContinueButtonText;
        }

        if (controller.ActionButton != null)
        {
            controller.ActionButton.gameObject.SetActive(controller.IsPhaseActive);
            controller.ActionButton.interactable = controller.IsPhaseActive && controller.ExecutionRoutine == null;
        }

        RefreshHintBlocks(controller);
        RefreshLayout(controller);
    }

    /// <summary>
    /// Updates the shared memory feedback field color and visibility.
    /// </summary>
    public static void SetFeedback(TMP_Text feedbackText, string message, bool isFailure, Color successColor, Color failureColor)
    {
        if (feedbackText == null)
            return;

        feedbackText.text = message;
        feedbackText.color = isFailure ? failureColor : successColor;
        feedbackText.gameObject.SetActive(!string.IsNullOrWhiteSpace(message));
    }

    /// <summary>
    /// Rebuilds the memory hint dropdown in deterministic authored order.
    /// </summary>
    public static void PopulateHintDropdown(TMP_Dropdown hintDropdown)
    {
        if (hintDropdown == null)
            return;

        var selectedValue = Mathf.Clamp(hintDropdown.value, 0, 2);
        InfoCatalog.Load().PopulateDropdown(PhaseInfoTopicGroup.Memory, hintDropdown, selectedValue);
    }

    static string BuildAddressStatusText(MemoryController controller, MemoryTransferService transferService)
    {
        if (!transferService.UsesInteractiveMemory(controller.CurrentInstruction))
            return controller.IsAssessmentMode ? "Waiting" : "Address: memory path skipped";

        if (controller.AddressScanner == null || controller.AddressScanner.AcceptedPacket == null)
            return controller.IsAssessmentMode ? "Waiting" : "Address: waiting for ALU Result";

        return controller.IsAssessmentMode
            ? MemoryTransferService.FormatAddress(controller.AddressScanner.AcceptedPacket.Value)
            : $"Address: {MemoryTransferService.FormatAddress(controller.AddressScanner.AcceptedPacket.Value)}";
    }

    static string BuildDataStatusText(MemoryController controller, MemoryTransferService transferService)
    {
        if (!transferService.UsesInteractiveMemory(controller.CurrentInstruction))
            return controller.IsAssessmentMode ? "Waiting" : "Data: not used in this phase";

        if (transferService.IsLoadInstruction(controller.CurrentInstruction))
        {
            if (controller.HasCompletedMemoryAccess)
                return controller.IsAssessmentMode ? controller.LastLoadedValue.ToString() : $"Value: {controller.LastLoadedValue}";

            return controller.IsAssessmentMode ? "Waiting" : "Value: waiting for Execute Memory";
        }

        if (controller.DataScanner == null || controller.DataScanner.AcceptedPacket == null)
            return controller.IsAssessmentMode ? "Waiting" : "Value: waiting for store packet";

        return controller.IsAssessmentMode ? controller.DataScanner.AcceptedPacket.Value.ToString() : $"Value: {controller.DataScanner.AcceptedPacket.Value}";
    }

    static void RefreshHintBlocks(MemoryController controller)
    {
        var usesAssessmentHints = controller.IsAssessmentMode && LessonModePolicy.UsesHintPanel(controller.CurrentMode);
        SetObjectActive(controller.HintPanel.InfoRoot, !usesAssessmentHints);

        if (controller.PracticeHintButton != null)
            controller.PracticeHintButton.gameObject.SetActive(usesAssessmentHints);

        SetHintBlockActive(
            controller.PracticeHintText,
            usesAssessmentHints && !string.IsNullOrWhiteSpace(controller.PracticeHintText != null ? controller.PracticeHintText.text : string.Empty));

        if (usesAssessmentHints)
        {
            SetHintBlockActive(controller.HintMemReadText, false);
            SetHintBlockActive(controller.HintMemWriteText, false);
            return;
        }

        var selectedHint = controller.HintDropdown != null ? controller.HintDropdown.value : 0;
        SetHintBlockActive(controller.HintMemReadText, selectedHint == 1);
        SetHintBlockActive(controller.HintMemWriteText, selectedHint == 2);
    }

    static void RefreshLayout(MemoryController controller)
    {
        if (controller == null || controller.MemUiRoot == null)
            return;

        var uiRoot = controller.MemUiRoot;
        if (uiRoot == null || !uiRoot.activeInHierarchy)
            return;

        foreach (var textMesh in uiRoot.GetComponentsInChildren<TMP_Text>(true))
            textMesh?.ForceMeshUpdate();

        Canvas.ForceUpdateCanvases();

        foreach (var scrollRect in uiRoot.GetComponentsInChildren<ScrollRect>(true))
        {
            if (scrollRect.content != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content);

            if (scrollRect.viewport != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.viewport);
        }

        var rootRect = uiRoot.GetComponent<RectTransform>();
        if (rootRect != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(rootRect);

        Canvas.ForceUpdateCanvases();
    }

    static void SetHintBlockActive(TMP_Text textBlock, bool isActive)
    {
        if (textBlock != null)
            textBlock.gameObject.SetActive(isActive);
    }

    static void SetObjectActive(GameObject targetObject, bool isActive)
    {
        if (targetObject != null)
            targetObject.SetActive(isActive);
    }
}

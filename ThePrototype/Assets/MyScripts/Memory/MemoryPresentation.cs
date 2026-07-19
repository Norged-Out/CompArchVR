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

        if (controller.LessonRuntimeText != null)
        {
            if (controller.IsPracticeMode)
                controller.LessonRuntimeText.text = "Practice Mode\nComplete the Memory phase using the instruction you decoded earlier.";
            else
            {
                var instructionName = controller.CurrentInstruction != null ? controller.CurrentInstruction.displayName : "instruction";
                var assembly = controller.CurrentInstruction != null ? controller.CurrentInstruction.assemblyInstructionText : "lw t1, 8(t0)";
                controller.LessonRuntimeText.text = $"Instruction: {instructionName}\nAssembly: {assembly}";
            }
        }

        if (controller.LoadLessonText != null)
            controller.LoadLessonText.gameObject.SetActive(!controller.IsPracticeMode && transferService.IsLoadInstruction(controller.CurrentInstruction));

        if (controller.StoreLessonText != null)
            controller.StoreLessonText.gameObject.SetActive(!controller.IsPracticeMode && transferService.IsStoreInstruction(controller.CurrentInstruction));

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
            return controller.IsPracticeMode ? "Waiting" : "Address: memory path skipped";

        if (controller.AddressScanner == null || controller.AddressScanner.AcceptedPacket == null)
            return controller.IsPracticeMode ? "Waiting" : "Address: waiting for ALU Result";

        return controller.IsPracticeMode
            ? MemoryTransferService.FormatAddress(controller.AddressScanner.AcceptedPacket.Value)
            : $"Address: {MemoryTransferService.FormatAddress(controller.AddressScanner.AcceptedPacket.Value)}";
    }

    static string BuildDataStatusText(MemoryController controller, MemoryTransferService transferService)
    {
        if (!transferService.UsesInteractiveMemory(controller.CurrentInstruction))
            return controller.IsPracticeMode ? "Waiting" : "Data: not used in this phase";

        if (transferService.IsLoadInstruction(controller.CurrentInstruction))
        {
            if (controller.HasCompletedMemoryAccess)
                return controller.IsPracticeMode ? controller.LastLoadedValue.ToString() : $"Value: {controller.LastLoadedValue}";

            return controller.IsPracticeMode ? "Waiting" : "Value: waiting for Execute Memory";
        }

        if (controller.DataScanner == null || controller.DataScanner.AcceptedPacket == null)
            return controller.IsPracticeMode ? "Waiting" : "Value: waiting for store packet";

        return controller.IsPracticeMode ? controller.DataScanner.AcceptedPacket.Value.ToString() : $"Value: {controller.DataScanner.AcceptedPacket.Value}";
    }

    static void RefreshHintBlocks(MemoryController controller)
    {
        var isPracticeMode = controller.IsPracticeMode;
        SetObjectActive(controller.HintPanel.InfoRoot, !isPracticeMode);

        if (controller.PracticeHintButton != null)
            controller.PracticeHintButton.gameObject.SetActive(isPracticeMode);

        SetHintBlockActive(
            controller.PracticeHintText,
            isPracticeMode && !string.IsNullOrWhiteSpace(controller.PracticeHintText != null ? controller.PracticeHintText.text : string.Empty));

        if (isPracticeMode)
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

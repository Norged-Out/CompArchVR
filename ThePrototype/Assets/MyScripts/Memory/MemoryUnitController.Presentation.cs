using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Presentation helpers for the memory phase UI and hint panel.
/// </summary>
public partial class MemoryUnitController
{
    void RefreshPresentation()
    {
        CacheReferences();
        RefreshExpectedTargets();

        if (m_LessonRuntimeText != null)
            m_LessonRuntimeText.text = BuildLessonRuntimeText();

        if (m_LoadLessonText != null)
            m_LoadLessonText.gameObject.SetActive(IsLoadInstruction());

        if (m_StoreLessonText != null)
            m_StoreLessonText.gameObject.SetActive(IsStoreInstruction());

        if (m_MemReadStatusText != null)
            m_MemReadStatusText.text = $"MemRead: {m_MemReadValue}";

        if (m_MemWriteStatusText != null)
            m_MemWriteStatusText.text = $"MemWrite: {m_MemWriteValue}";

        if (m_AddressStatusText != null)
            m_AddressStatusText.text = BuildAddressStatusText();

        if (m_DataStatusText != null)
            m_DataStatusText.text = BuildDataStatusText();

        if (m_ActionButtonLabel != null)
        {
            m_ActionButtonLabel.text = UsesInteractiveMemory()
                ? m_IsAwaitingContinue ? m_ContinueButtonText : m_ExecuteButtonText
                : m_ContinueButtonText;
        }

        if (m_ActionButton != null)
        {
            m_ActionButton.gameObject.SetActive(m_IsPhaseActive);
            m_ActionButton.interactable = m_IsPhaseActive && m_ExecutionRoutine == null;
        }

        RefreshHintBlocks();
        RefreshLayout();
    }

    string BuildLessonRuntimeText()
    {
        var instructionName = m_CurrentInstruction != null ? m_CurrentInstruction.displayName : "instruction";
        var assembly = m_CurrentInstruction != null ? m_CurrentInstruction.assemblyInstructionText : "lw t1, 8(t0)";

        return $"Instruction: {instructionName}\nAssembly: {assembly}";
    }

    string BuildAddressStatusText()
    {
        if (!UsesInteractiveMemory())
            return "Address: memory path skipped";

        if (m_AddressScanner == null || m_AddressScanner.AcceptedPacket == null)
            return "Address: waiting for ALU Result";

        return $"Address: {FormatAddress(m_AddressScanner.AcceptedPacket.Value)} (ALU Result)";
    }

    string BuildDataStatusText()
    {
        if (!UsesInteractiveMemory())
            return "Data: not used in this phase";

        if (IsLoadInstruction())
        {
            if (m_HasCompletedMemoryAccess)
                return $"Value: Memory Data = {m_LastLoadedValue}";

            return "Value: waiting for Execute Memory";
        }

        if (m_DataScanner == null || m_DataScanner.AcceptedPacket == null)
            return "Value: waiting for store packet";

        return $"Value: {m_DataScanner.AcceptedPacket.Value} ({GetPacketRoleLabel(m_DataScanner.AcceptedPacket.PacketRole)})";
    }

    void SetFeedback(string message, bool isFailure)
    {
        if (m_FeedbackText == null)
            return;

        m_FeedbackText.text = message;
        m_FeedbackText.color = isFailure ? m_FailureFeedbackColor : m_SuccessFeedbackColor;
        m_FeedbackText.gameObject.SetActive(!string.IsNullOrWhiteSpace(message));
    }

    void RefreshHintBlocks()
    {
        var selectedHint = m_HintDropdown != null ? m_HintDropdown.value : 0;

        SetHintBlockActive(m_HintMemReadText, selectedHint == 1);
        SetHintBlockActive(m_HintMemWriteText, selectedHint == 2);
    }

    void PopulateHintDropdown()
    {
        if (m_HintDropdown == null)
            return;

        var selectedValue = Mathf.Clamp(m_HintDropdown.value, 0, 2);
        m_HintDropdown.ClearOptions();
        m_HintDropdown.AddOptions(new System.Collections.Generic.List<string>
        {
            "Choose Option",
            "MemRead",
            "MemWrite",
        });
        m_HintDropdown.SetValueWithoutNotify(selectedValue);
    }

    void RefreshLayout()
    {
        if (m_MemUiRoot == null || !m_MemUiRoot.activeInHierarchy)
            return;

        foreach (var textMesh in m_MemUiRoot.GetComponentsInChildren<TMP_Text>(true))
            textMesh?.ForceMeshUpdate();
        Canvas.ForceUpdateCanvases();

        var scrollRect = m_MemUiRoot.GetComponentInChildren<ScrollRect>(true);
        if (scrollRect != null && scrollRect.content != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content);

            if (scrollRect.viewport != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.viewport);
        }

        var rootRect = m_MemUiRoot.GetComponent<RectTransform>();
        if (rootRect != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(rootRect);

        Canvas.ForceUpdateCanvases();
    }

    static string GetPacketRoleLabel(DataPacketRole packetRole)
    {
        return packetRole switch
        {
            DataPacketRole.ReadData1 => "Read Data 1",
            DataPacketRole.ReadData2 => "Read Data 2",
            DataPacketRole.Immediate => "Immediate",
            DataPacketRole.AluResult => "ALU Result",
            DataPacketRole.MemoryData => "Memory Data",
            _ => "Packet",
        };
    }

    static string FormatAddress(int address)
    {
        return $"0x{address:X8}";
    }

    static void SetHintBlockActive(TMP_Text textBlock, bool isActive)
    {
        if (textBlock == null)
            return;

        textBlock.gameObject.SetActive(isActive);
    }
}

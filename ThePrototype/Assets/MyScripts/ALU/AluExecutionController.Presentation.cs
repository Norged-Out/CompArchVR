using TMPro;
using UnityEngine;

/// <summary>
/// Presentation helpers for ALU UI text, labels, and hint visibility.
/// </summary>
public partial class AluExecutionController
{
    void RefreshAllPresentation()
    {
        RefreshExpectedInputRoles();
        RefreshPrefabLabels();
        RefreshUiTexts();
    }

    void RefreshPrefabLabels()
    {
        if (m_OperationLabelText != null)
            m_OperationLabelText.text = GetOperationDisplayName();
    }

    void RefreshUiTexts()
    {
        if (m_LessonRuntimeText != null)
            m_LessonRuntimeText.text = BuildLessonRuntimeText();

        if (m_AluOpStatusText != null)
            m_AluOpStatusText.text = $"ALUOp: {m_CurrentAluOpValue}";

        if (m_AluSrcStatusText != null)
            m_AluSrcStatusText.text = $"ALUSrc: {m_CurrentAluSrcValue}";

        if (m_FunctDropdown != null)
        {
            var showFunctDropdown = m_CurrentAluOpValue == "10";
            m_FunctDropdown.gameObject.SetActive(showFunctDropdown);
            m_FunctDropdown.interactable = showFunctDropdown && !m_HasProducedResult;
            if (showFunctDropdown && m_HasExplicitFunctSelection)
                SyncDropdownToCurrentOperation();
        }

        RefreshHintBlocks();

        if (m_Input1StatusText != null)
            m_Input1StatusText.text = BuildInputStatusText("Input 1", DataPacketRole.ReadData1, m_InputA);

        if (m_Input2StatusText != null)
            m_Input2StatusText.text = BuildInputStatusText("Input 2", GetExpectedInput2Role(), m_InputB);

        if (m_ExecuteButtonLabel != null)
            m_ExecuteButtonLabel.text = m_HasProducedResult ? m_ResultReadyButtonText : m_ExecuteButtonText;

        if (m_ExecuteButton != null)
            m_ExecuteButton.interactable = m_IsPhaseActive && m_ComputeRoutine == null;
    }

    string BuildLessonRuntimeText()
    {
        var instructionName = m_CurrentInstruction != null ? m_CurrentInstruction.displayName : "instruction";
        var assembly = m_CurrentInstruction != null ? m_CurrentInstruction.assemblyInstructionText : "add t2, t0, t1";

        return $"Instruction: {instructionName}\nAssembly: {assembly}";
    }

    string BuildInputStatusText(string inputLabel, DataPacketRole expectedRole, AluInputScanner scanner)
    {
        if (scanner == null)
            return $"{inputLabel}: waiting for {GetRoleDisplayName(expectedRole)}";

        if (scanner.AcceptedPacket == null)
        {
            if (scanner.CurrentIssue == AluInputScanner.PacketIssue.ImmediateNotSignExtended)
                return $"{inputLabel}: Immediate detected (not sign-extended)";

            return $"{inputLabel}: waiting for {GetRoleDisplayName(expectedRole)}";
        }

        var signExtensionSuffix = scanner.AcceptedPacket.PacketRole == DataPacketRole.Immediate
            ? scanner.AcceptedPacket.IsSignExtended ? " (sign-extended)" : " (not sign-extended)"
            : string.Empty;
        return $"{inputLabel}: {GetRoleDisplayName(scanner.AcceptedPacket.PacketRole)} = {scanner.AcceptedValue}{signExtensionSuffix}";
    }

    string GetOperationDisplayName()
    {
        if (m_CurrentAluOpValue == "10" && !m_HasExplicitFunctSelection)
            return "None";

        return GetOperationDisplayName(ResolveCurrentOperation());
    }

    string GetOperationDisplayName(AluOperation operation)
    {
        return operation switch
        {
            AluOperation.Subtract => "Sub",
            AluOperation.And => "And",
            AluOperation.Or => "Or",
            AluOperation.SetOnLessThan => "Slt",
            _ => "Add",
        };
    }

    string GetPostExecuteFeedback(int resultValue)
    {
        if (m_CurrentInstruction == null)
            return $"ALU result ready: {resultValue}. Click Continue.";

        if (m_CurrentInstruction.UsesBranchDecision())
            return $"Zero result ready: {resultValue}. Click Continue to proceed to Program Counter Update.";

        if (m_CurrentInstruction.UsesInteractiveMemoryPhase())
            return $"ALU result ready: {resultValue}. Click Continue to proceed to Memory Access.";

        if (m_CurrentInstruction.UsesWriteBackPhase())
            return $"ALU result ready: {resultValue}. Memory Access is skipped for this instruction. Click Continue to proceed to Write Back.";

        return $"ALU result ready: {resultValue}. Click Continue to proceed to Program Counter Update.";
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

        SetHintBlockActive(m_HintAluOpText, selectedHint == 1);
        SetHintBlockActive(m_HintAluSrcText, selectedHint == 2);
        SetHintBlockActive(m_HintAluControlText, selectedHint == 3);
    }

    void PopulateHintDropdown()
    {
        if (m_HintDropdown == null)
            return;

        var selectedValue = Mathf.Clamp(m_HintDropdown.value, 0, 3);
        m_HintDropdown.ClearOptions();
        m_HintDropdown.AddOptions(new System.Collections.Generic.List<string>
        {
            "Choose Option",
            "ALUOp",
            "ALUSrc",
            "ALU Control",
        });
        m_HintDropdown.SetValueWithoutNotify(selectedValue);
    }

    static void SetHintBlockActive(TMP_Text textBlock, bool isActive)
    {
        if (textBlock == null)
            return;

        textBlock.gameObject.SetActive(isActive);
    }
}

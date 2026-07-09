using System.Collections.Generic;

public partial class LessonGuideController
{
    string BuildDecodeOpcodeSelectionText(InstructionFlowStep step)
    {
        var instruction = m_LessonFlow.CurrentInstruction;
        if (instruction == null)
            return string.Empty;

        if (IsDecodeOpcodeSelectionStep())
            return $"Assembly: {instruction.assemblyInstructionText}";

        return string.Empty;
    }

    string BuildDecodeRegisterSelectionText(InstructionFlowStep step)
    {
        var instruction = m_LessonFlow.CurrentInstruction;
        if (instruction == null)
            return string.Empty;

        if (step.requiredInteraction == InstructionStepInteractionType.RegisterSelection)
        {
            var lines = new List<string>();
            var requiredRoles = LessonChecks.GetRequiredRoles(instruction, step);

            for (var index = 0; index < requiredRoles.Length; index++)
            {
                var role = requiredRoles[index];
                var registerName = instruction.GetExpectedRegisterName(role);
                var scannerName = GetScannerLabel(role);
                var status = index < m_LessonFlow.CurrentRegisterSelectionIndex ? "done" : "pending";
                lines.Add($"{scannerName}: {registerName} [{status}]");
            }

            if (instruction.usesImmediate)
            {
                var immediateStatus = m_LessonFlow.RegisterSelectionReadyToContinue ? "ready to generate" : "locked";
                lines.Add($"Immediate packet: {instruction.expectedImmediateValue} [{immediateStatus}]");
            }

            var nextAction = m_LessonFlow.RegisterSelectionReadyToContinue
                ? instruction.usesImmediate
                    ? "Press Continue to generate the immediate packet and proceed to Execution."
                    : "Press Continue to proceed to Execution."
                : $"Current target: {GetCurrentDecodeTargetLabel(instruction, step)}.";

            return $"{string.Join("\n", lines)}\n\n{nextAction}";
        }

        return step.explanation;
    }

    string BuildDecodeFunctSelectionText(InstructionFlowStep step)
    {
        var instruction = m_LessonFlow.CurrentInstruction;
        if (instruction == null)
            return string.Empty;

        if (IsDecodeFunctSelectionStep())
            return $"Assembly: {instruction.assemblyInstructionText}";

        return string.Empty;
    }

    void RefreshDecodeTextBlocks(InstructionFlowStep step)
    {
        var isOpcodeStep = IsDecodeOpcodeSelectionStep();
        var isFunctStep = IsDecodeFunctSelectionStep();
        var isRegisterStep = step != null && step.requiredInteraction == InstructionStepInteractionType.RegisterSelection;

        // Decode uses authored text blocks so scene layout stays stable and runtime
        // only swaps the parts that truly depend on instruction/selection state.
        SetActive(m_IDOpcodeLessonText, isOpcodeStep);
        SetActive(m_IDFunctLessonText, isFunctStep);
        SetActive(m_IDRegisterLessonText, isRegisterStep);
        SetActive(m_IDOpcodeBodyText, isOpcodeStep);
        SetActive(m_IDFunctBodyText, isFunctStep);
        SetActive(m_IDRegisterBodyText, isRegisterStep);
        SetActive(m_IDOpcodeSelectionText, isOpcodeStep);
        SetActive(m_IDFunctSelectionText, isFunctStep);
        SetActive(m_IDRegisterSelectionText, isRegisterStep);

        SetText(m_IDOpcodeSelectionText, isOpcodeStep ? BuildDecodeOpcodeSelectionText(step) : string.Empty);
        SetText(m_IDFunctSelectionText, isFunctStep ? BuildDecodeFunctSelectionText(step) : string.Empty);
        SetText(m_IDRegisterSelectionText, isRegisterStep ? BuildDecodeRegisterSelectionText(step) : string.Empty);
    }

    void PopulateDecodeDropdowns()
    {
        PopulateDecodeOpcodeDropdown();
        PopulateDecodeFunctDropdown();
        PopulateDecodeHintDropdown();
    }

    void PopulateDecodeOpcodeDropdown()
    {
        if (m_IDOpcodeDropdown == null)
            return;

        m_DecodeOpcodeOptions.Clear();

        var optionLabels = new List<string> { "Choose Opcode" };
        foreach (var instruction in m_AvailableInstructions)
        {
            if (instruction == null || string.IsNullOrWhiteSpace(instruction.opcodeBits))
                continue;

            var opcode = instruction.opcodeBits.Trim();
            if (m_DecodeOpcodeOptions.Contains(opcode))
                continue;

            m_DecodeOpcodeOptions.Add(opcode);
            optionLabels.Add(opcode);
        }

        if (m_LessonFlow != null &&
            m_LessonFlow.CurrentInstruction != null &&
            !string.IsNullOrWhiteSpace(m_LessonFlow.CurrentInstruction.opcodeBits))
        {
            var currentOpcode = m_LessonFlow.CurrentInstruction.opcodeBits.Trim();
            if (!m_DecodeOpcodeOptions.Contains(currentOpcode))
            {
                m_DecodeOpcodeOptions.Add(currentOpcode);
                optionLabels.Add(currentOpcode);
            }
        }

        m_IsRefreshingDecodeDropdowns = true;
        m_IDOpcodeDropdown.ClearOptions();
        m_IDOpcodeDropdown.AddOptions(optionLabels);
        m_IDOpcodeDropdown.SetValueWithoutNotify(0);
        m_IsRefreshingDecodeDropdowns = false;
    }

    void PopulateDecodeFunctDropdown()
    {
        if (m_IDFunctDropdown == null)
            return;

        m_DecodeFunctOptions.Clear();

        var optionLabels = new List<string> { "Choose Funct" };
        foreach (var instruction in m_AvailableInstructions)
        {
            if (instruction == null || string.IsNullOrWhiteSpace(instruction.functBits))
                continue;

            var funct = instruction.functBits.Trim();
            if (m_DecodeFunctOptions.Contains(funct))
                continue;

            m_DecodeFunctOptions.Add(funct);
            optionLabels.Add(funct);
        }

        if (m_LessonFlow != null &&
            m_LessonFlow.CurrentInstruction != null &&
            !string.IsNullOrWhiteSpace(m_LessonFlow.CurrentInstruction.functBits))
        {
            var currentFunct = m_LessonFlow.CurrentInstruction.functBits.Trim();
            if (!m_DecodeFunctOptions.Contains(currentFunct))
            {
                m_DecodeFunctOptions.Add(currentFunct);
                optionLabels.Add(currentFunct);
            }
        }

        m_IsRefreshingDecodeDropdowns = true;
        m_IDFunctDropdown.ClearOptions();
        m_IDFunctDropdown.AddOptions(optionLabels);
        m_IDFunctDropdown.SetValueWithoutNotify(0);
        m_IsRefreshingDecodeDropdowns = false;
    }

    void PopulateDecodeHintDropdown()
    {
        if (m_IDHintDropdown == null)
            return;

        m_IsRefreshingDecodeDropdowns = true;
        m_IDHintDropdown.ClearOptions();
        m_IDHintDropdown.AddOptions(new List<string> { "Choose Option", "Opcode", "Funct" });
        m_IDHintDropdown.SetValueWithoutNotify(0);
        m_IsRefreshingDecodeDropdowns = false;
    }

    void ResetDecodeDropdowns()
    {
        m_IsRefreshingDecodeDropdowns = true;

        if (m_IDOpcodeDropdown != null)
            m_IDOpcodeDropdown.SetValueWithoutNotify(0);

        if (m_IDFunctDropdown != null)
            m_IDFunctDropdown.SetValueWithoutNotify(0);

        if (m_IDHintDropdown != null)
            m_IDHintDropdown.SetValueWithoutNotify(0);

        m_IsRefreshingDecodeDropdowns = false;
        m_IsDecodeFunctStepActive = false;
        SetText(m_IDHintText, string.Empty);
    }

    void RefreshDecodeDropdownState(InstructionFlowStep step)
    {
        var showOpcodeDropdown = IsDecodeOpcodeSelectionStep();
        var showFunctDropdown = IsDecodeFunctSelectionStep();

        if (m_IDOpcodeDropdown != null)
        {
            m_IDOpcodeDropdown.gameObject.SetActive(showOpcodeDropdown);
            m_IDOpcodeDropdown.interactable = showOpcodeDropdown;
        }

        if (m_IDFunctDropdown != null)
        {
            m_IDFunctDropdown.gameObject.SetActive(showFunctDropdown);
            m_IDFunctDropdown.interactable = showFunctDropdown;
        }

        if (m_IDHintDropdown != null)
            m_IDHintDropdown.gameObject.SetActive(true);

        if (m_IDHintText != null)
            m_IDHintText.gameObject.SetActive(m_IDHintDropdown != null && m_IDHintDropdown.value > 0);
    }

    void RefreshDecodeHintText()
    {
        if (m_IDHintDropdown == null)
            return;

        string hintText;
        switch (m_IDHintDropdown.value)
        {
            case 1:
                hintText = BuildOpcodeHintText();
                break;
            case 2:
                hintText = BuildFunctHintText();
                break;
            default:
                hintText = string.Empty;
                break;
        }

        SetText(m_IDHintText, hintText);
    }

    string BuildOpcodeHintText()
    {
        var lines = new List<string>();
        foreach (var instruction in m_AvailableInstructions)
        {
            if (instruction == null || string.IsNullOrWhiteSpace(instruction.opcodeBits))
                continue;

            var line = $"{instruction.displayName} -> {instruction.opcodeBits.Trim()}";
            if (!lines.Contains(line))
                lines.Add(line);
        }

        return lines.Count == 0
            ? "No opcode reference available."
            : "Opcode reference\n\n" + string.Join("\n", lines);
    }

    string BuildFunctHintText()
    {
        var lines = new List<string>();
        foreach (var instruction in m_AvailableInstructions)
        {
            if (instruction == null || string.IsNullOrWhiteSpace(instruction.functBits))
                continue;

            var line = $"{instruction.displayName} -> {instruction.functBits.Trim()}";
            if (!lines.Contains(line))
                lines.Add(line);
        }

        return lines.Count == 0
            ? "No funct reference available."
            : "Funct reference\n\n" + string.Join("\n", lines);
    }

    void HandleDecodeOpcodeContinue()
    {
        if (m_LessonFlow == null || m_LessonFlow.CurrentInstruction == null)
            return;

        var selectedOpcode = GetSelectedDecodeOpcode();
        if (string.IsNullOrWhiteSpace(selectedOpcode))
        {
            HandleFeedbackChanged("Select an opcode first.", true);
            return;
        }

        var expectedOpcode = m_LessonFlow.CurrentInstruction.opcodeBits != null
            ? m_LessonFlow.CurrentInstruction.opcodeBits.Trim()
            : string.Empty;

        if (!string.Equals(selectedOpcode, expectedOpcode, System.StringComparison.Ordinal))
        {
            HandleFeedbackChanged("That opcode does not match the selected instruction.", true);
            return;
        }

        if (InstructionUsesDecodeFunct(m_LessonFlow.CurrentInstruction))
        {
            m_IsDecodeFunctStepActive = true;
            if (m_IDFunctDropdown != null)
                m_IDFunctDropdown.SetValueWithoutNotify(0);

            HandleFeedbackChanged("Opcode confirmed. Now identify the funct field.", false);
            RefreshView();
            return;
        }

        HandleFeedbackChanged("Opcode confirmed. Continue into operand setup.", false);
        m_LessonFlow.Advance();
    }

    string GetSelectedDecodeOpcode()
    {
        if (m_IDOpcodeDropdown == null ||
            m_IDOpcodeDropdown.options == null ||
            m_IDOpcodeDropdown.value <= 0 ||
            m_IDOpcodeDropdown.value >= m_IDOpcodeDropdown.options.Count)
        {
            return string.Empty;
        }

        return m_IDOpcodeDropdown.options[m_IDOpcodeDropdown.value].text.Trim();
    }

    void HandleDecodeFunctContinue()
    {
        if (m_LessonFlow == null || m_LessonFlow.CurrentInstruction == null)
            return;

        var selectedFunct = GetSelectedDecodeFunct();
        if (string.IsNullOrWhiteSpace(selectedFunct))
        {
            HandleFeedbackChanged("Select a funct value first.", true);
            return;
        }

        var expectedFunct = m_LessonFlow.CurrentInstruction.functBits != null
            ? m_LessonFlow.CurrentInstruction.functBits.Trim()
            : string.Empty;

        if (!string.Equals(selectedFunct, expectedFunct, System.StringComparison.Ordinal))
        {
            HandleFeedbackChanged("That funct value does not match the selected instruction.", true);
            return;
        }

        m_IsDecodeFunctStepActive = false;
        HandleFeedbackChanged("Funct confirmed. Continue into operand setup.", false);
        m_LessonFlow.Advance();
    }

    string GetSelectedDecodeFunct()
    {
        if (m_IDFunctDropdown == null ||
            m_IDFunctDropdown.options == null ||
            m_IDFunctDropdown.value <= 0 ||
            m_IDFunctDropdown.value >= m_IDFunctDropdown.options.Count)
        {
            return string.Empty;
        }

        return m_IDFunctDropdown.options[m_IDFunctDropdown.value].text.Trim();
    }

    bool IsDecodeOpcodeSelectionStep()
    {
        var step = m_LessonFlow != null ? m_LessonFlow.CurrentStep : null;
        return step != null &&
               step.highlightedNode == DatapathNodeId.InstructionMemory &&
               !m_IsDecodeFunctStepActive;
    }

    bool IsDecodeFunctSelectionStep()
    {
        var step = m_LessonFlow != null ? m_LessonFlow.CurrentStep : null;
        return step != null &&
               step.highlightedNode == DatapathNodeId.InstructionMemory &&
               m_IsDecodeFunctStepActive;
    }

    static bool InstructionUsesDecodeFunct(InstructionDefinition instruction)
    {
        return instruction != null &&
               !string.IsNullOrWhiteSpace(instruction.functBits) &&
               string.Equals(instruction.opcodeBits != null ? instruction.opcodeBits.Trim() : string.Empty, "000000", System.StringComparison.Ordinal);
    }

    string GetCurrentDecodeTargetLabel(InstructionDefinition instruction, InstructionFlowStep step)
    {
        var requiredRoles = LessonChecks.GetRequiredRoles(instruction, step);
        var currentIndex = m_LessonFlow != null ? m_LessonFlow.CurrentRegisterSelectionIndex : 0;
        if (currentIndex < 0 || currentIndex >= requiredRoles.Length)
            return "Place the required register";

        var role = requiredRoles[currentIndex];
        return $"{instruction.GetExpectedRegisterName(role)} on {GetScannerLabel(role)}";
    }

    static string GetScannerLabel(InstructionRegisterRole registerRole)
    {
        return registerRole switch
        {
            InstructionRegisterRole.Rs => "Read Register 1",
            InstructionRegisterRole.Rt => "Read Register 2",
            InstructionRegisterRole.Rd => "Write Register",
            _ => "the correct scanner",
        };
    }
}

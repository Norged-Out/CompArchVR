using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine.Events;

/// <summary>
/// Owns opcode, funct, and hint dropdown behavior for the decode panel.
/// This keeps option rebuilding, hint routing, and learner selection lookup
/// separate from the panel controller's text/presentation concerns.
/// </summary>
sealed class DecodeDropdownView
{
    enum DecodeHintTopic
    {
        None = 0,
        Opcode = 1,
        Funct = 2,
    }

    readonly TMP_Dropdown m_OpcodeDropdown;
    readonly TMP_Dropdown m_FunctDropdown;
    readonly TMP_Dropdown m_HintDropdown;
    readonly TMP_Text m_HintText;
    readonly DecodeHintBuilder m_HintBuilder = new();
    readonly List<string> m_OpcodeOptions = new();
    readonly List<string> m_FunctOptions = new();

    /// <summary>
    /// Captures the authored decode dropdowns and hint field managed by this helper.
    /// </summary>
    public DecodeDropdownView(
        TMP_Dropdown opcodeDropdown,
        TMP_Dropdown functDropdown,
        TMP_Dropdown hintDropdown,
        TMP_Text hintText)
    {
        m_OpcodeDropdown = opcodeDropdown;
        m_FunctDropdown = functDropdown;
        m_HintDropdown = hintDropdown;
        m_HintText = hintText;
    }

    /// <summary>
    /// Rebuilds the opcode, funct, and hint dropdowns from the authored instruction catalog.
    /// </summary>
    public void Populate(IReadOnlyList<InstructionDefinition> availableInstructions, InstructionDefinition currentInstruction, ref bool isRefreshing)
    {
        PopulateBitDropdown(
            m_OpcodeDropdown,
            m_OpcodeOptions,
            availableInstructions,
            currentInstruction,
            instruction => instruction != null ? instruction.opcodeBits : null,
            "Choose Opcode",
            ref isRefreshing);
        PopulateBitDropdown(
            m_FunctDropdown,
            m_FunctOptions,
            availableInstructions,
            currentInstruction,
            instruction => instruction != null ? instruction.functBits : null,
            "Choose Funct",
            ref isRefreshing);
        PopulateHintDropdown(ref isRefreshing);
    }

    /// <summary>
    /// Resets every decode dropdown back to its placeholder row.
    /// </summary>
    public void Reset(ref bool isRefreshing)
    {
        isRefreshing = true;

        if (m_OpcodeDropdown != null)
            m_OpcodeDropdown.SetValueWithoutNotify(0);

        if (m_FunctDropdown != null)
            m_FunctDropdown.SetValueWithoutNotify(0);

        if (m_HintDropdown != null)
            m_HintDropdown.SetValueWithoutNotify(0);

        isRefreshing = false;
        SetHintText(string.Empty);
    }

    /// <summary>
    /// Clears only the funct dropdown when opcode validation unlocks the R-type funct step.
    /// </summary>
    public void ResetFunct(ref bool isRefreshing)
    {
        if (m_FunctDropdown == null)
            return;

        isRefreshing = true;
        m_FunctDropdown.SetValueWithoutNotify(0);
        isRefreshing = false;
    }

    /// <summary>
    /// Rebinds opcode and funct dropdowns to the shared decode-selection callback.
    /// </summary>
    public void BindSelectionDropdowns(UnityAction<int> listener)
    {
        BindDropdown(m_OpcodeDropdown, listener);
        BindDropdown(m_FunctDropdown, listener);
    }

    /// <summary>
    /// Rebinds the decode hint dropdown to the supplied callback.
    /// </summary>
    public void BindHintDropdown(UnityAction<int> listener)
    {
        BindDropdown(m_HintDropdown, listener);
    }

    /// <summary>
    /// Shows only the dropdowns that are valid for the active decode sub-step.
    /// </summary>
    public void RefreshVisibleDropdowns(bool showOpcodeDropdown, bool showFunctDropdown)
    {
        SetDropdownState(m_OpcodeDropdown, showOpcodeDropdown);
        SetDropdownState(m_FunctDropdown, showFunctDropdown);

        if (m_HintDropdown != null)
            m_HintDropdown.gameObject.SetActive(true);

        if (m_HintText != null)
            m_HintText.gameObject.SetActive(m_HintDropdown != null && m_HintDropdown.value > 0);
    }

    /// <summary>
    /// Rebuilds the visible hint text from the currently selected help topic.
    /// </summary>
    public void RefreshHintText(IReadOnlyList<InstructionDefinition> availableInstructions)
    {
        SetHintText(BuildHintText(GetSelectedHintTopic(), availableInstructions));
    }

    /// <summary>
    /// Returns the learner's currently selected opcode bits.
    /// </summary>
    public string GetSelectedOpcode()
    {
        return GetSelectedDropdownValue(m_OpcodeDropdown);
    }

    /// <summary>
    /// Returns the learner's currently selected funct bits.
    /// </summary>
    public string GetSelectedFunct()
    {
        return GetSelectedDropdownValue(m_FunctDropdown);
    }

    /// <summary>
    /// Populates a dropdown with unique bit patterns drawn from the authored instruction list.
    /// </summary>
    static void PopulateBitDropdown(
        TMP_Dropdown dropdown,
        List<string> cache,
        IReadOnlyList<InstructionDefinition> availableInstructions,
        InstructionDefinition currentInstruction,
        Func<InstructionDefinition, string> selector,
        string placeholderLabel,
        ref bool isRefreshing)
    {
        if (dropdown == null)
            return;

        cache.Clear();
        var optionLabels = new List<string> { placeholderLabel };
        foreach (var instruction in availableInstructions)
        {
            var bits = selector(instruction);
            if (instruction == null || string.IsNullOrWhiteSpace(bits))
                continue;

            var trimmedBits = bits.Trim();
            if (cache.Contains(trimmedBits))
                continue;

            cache.Add(trimmedBits);
            optionLabels.Add(trimmedBits);
        }

        var currentBits = selector(currentInstruction);
        if (!string.IsNullOrWhiteSpace(currentBits))
        {
            var trimmedCurrentBits = currentBits.Trim();
            if (!cache.Contains(trimmedCurrentBits))
            {
                cache.Add(trimmedCurrentBits);
                optionLabels.Add(trimmedCurrentBits);
            }
        }

        isRefreshing = true;
        dropdown.ClearOptions();
        dropdown.AddOptions(optionLabels);
        dropdown.SetValueWithoutNotify(0);
        isRefreshing = false;
    }

    /// <summary>
    /// Populates the decode hint dropdown with the authored help topics.
    /// </summary>
    void PopulateHintDropdown(ref bool isRefreshing)
    {
        if (m_HintDropdown == null)
            return;

        isRefreshing = true;
        m_HintDropdown.ClearOptions();
        m_HintDropdown.AddOptions(new List<string> { "Choose Option", "Opcode", "Funct" });
        m_HintDropdown.SetValueWithoutNotify(0);
        isRefreshing = false;
    }

    /// <summary>
    /// Applies visibility and interactability together for a lesson dropdown.
    /// </summary>
    static void SetDropdownState(TMP_Dropdown dropdown, bool isActive)
    {
        if (dropdown == null)
            return;

        dropdown.gameObject.SetActive(isActive);
        dropdown.interactable = isActive;
    }

    /// <summary>
    /// Converts the active hint topic into learner-facing decode help text.
    /// </summary>
    string BuildHintText(DecodeHintTopic hintTopic, IReadOnlyList<InstructionDefinition> availableInstructions)
    {
        return hintTopic switch
        {
            DecodeHintTopic.Opcode => m_HintBuilder.BuildOpcodeHintText(availableInstructions),
            DecodeHintTopic.Funct => m_HintBuilder.BuildFunctHintText(availableInstructions),
            _ => string.Empty,
        };
    }

    /// <summary>
    /// Maps the hint dropdown selection onto the decode help topics supported by this panel.
    /// </summary>
    DecodeHintTopic GetSelectedHintTopic()
    {
        return m_HintDropdown == null ? DecodeHintTopic.None : (DecodeHintTopic)m_HintDropdown.value;
    }

    /// <summary>
    /// Returns the currently selected value for a decode dropdown, excluding placeholder rows.
    /// </summary>
    static string GetSelectedDropdownValue(TMP_Dropdown dropdown)
    {
        if (dropdown == null ||
            dropdown.options == null ||
            dropdown.value <= 0 ||
            dropdown.value >= dropdown.options.Count)
        {
            return string.Empty;
        }

        return dropdown.options[dropdown.value].text.Trim();
    }

    /// <summary>
    /// Applies the same rebinding behavior to any authored decode dropdown.
    /// </summary>
    static void BindDropdown(TMP_Dropdown dropdown, UnityAction<int> listener)
    {
        if (dropdown == null)
            return;

        dropdown.onValueChanged.RemoveAllListeners();
        if (listener != null)
            dropdown.onValueChanged.AddListener(listener);
    }

    /// <summary>
    /// Writes hint text and toggles the authored hint field based on content.
    /// </summary>
    void SetHintText(string text)
    {
        if (m_HintText == null)
            return;

        m_HintText.text = text;
        m_HintText.gameObject.SetActive(!string.IsNullOrWhiteSpace(text));
    }
}

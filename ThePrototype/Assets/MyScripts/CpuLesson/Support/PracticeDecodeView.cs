using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Owns the authored Practice-mode decode controls so the main decode panel can
/// reuse one panel root without absorbing every dropdown/toggle detail itself.
/// </summary>
sealed class PracticeDecodeView
{
    static readonly List<string> s_OpcodeOptions = new()
    {
        "Choose Opcode",
        "000000",
        "000100",
        "000101",
        "001000",
        "100011",
        "101011",
    };

    static readonly List<string> s_FunctOptions = new()
    {
        "Choose Funct",
        "100000",
        "100010",
        "100100",
        "100101",
        "101010",
    };

    readonly GameObject m_PracticeRoot;
    readonly TMP_Text m_BinaryText;
    readonly TMP_Text m_StatusText;
    readonly GameObject m_OpcodeGroupRoot;
    readonly TMP_Dropdown m_OpcodeDropdown;
    readonly GameObject m_RsGroupRoot;
    readonly TMP_Dropdown m_RsDropdown;
    readonly GameObject m_RtGroupRoot;
    readonly TMP_Dropdown m_RtDropdown;
    readonly GameObject m_ImmediateGroupRoot;
    readonly Toggle m_ImmediateToggle;
    readonly TMP_Dropdown m_ImmediateDropdown;
    readonly GameObject m_FunctGroupRoot;
    readonly Toggle m_FunctToggle;
    readonly TMP_Dropdown m_FunctDropdown;
    readonly TMP_Text m_HintText;

    readonly List<string> m_RegisterOptions = new();
    readonly List<string> m_ImmediateOptions = new();

    public PracticeDecodeView(
        GameObject practiceRoot,
        TMP_Text binaryText,
        TMP_Text statusText,
        GameObject opcodeGroupRoot,
        TMP_Dropdown opcodeDropdown,
        GameObject rsGroupRoot,
        TMP_Dropdown rsDropdown,
        GameObject rtGroupRoot,
        TMP_Dropdown rtDropdown,
        GameObject immediateGroupRoot,
        Toggle immediateToggle,
        TMP_Dropdown immediateDropdown,
        GameObject functGroupRoot,
        Toggle functToggle,
        TMP_Dropdown functDropdown,
        TMP_Text hintText)
    {
        m_PracticeRoot = practiceRoot;
        m_BinaryText = binaryText;
        m_StatusText = statusText;
        m_OpcodeGroupRoot = opcodeGroupRoot;
        m_OpcodeDropdown = opcodeDropdown;
        m_RsGroupRoot = rsGroupRoot;
        m_RsDropdown = rsDropdown;
        m_RtGroupRoot = rtGroupRoot;
        m_RtDropdown = rtDropdown;
        m_ImmediateGroupRoot = immediateGroupRoot;
        m_ImmediateToggle = immediateToggle;
        m_ImmediateDropdown = immediateDropdown;
        m_FunctGroupRoot = functGroupRoot;
        m_FunctToggle = functToggle;
        m_FunctDropdown = functDropdown;
        m_HintText = hintText;
    }

    /// <summary>
    /// Rebuilds the authored Practice decode controls and reveals only the
    /// groups that belong to the current staged sub-step.
    /// </summary>
    public void Refresh(PracticeInstructionDefinition instruction, bool opcodeConfirmed, ref bool isRefreshing)
    {
        SetVisible(true);
        PopulateStaticOptions(ref isRefreshing, instruction);

        SetGroupState(m_OpcodeGroupRoot, m_OpcodeDropdown, true);
        SetGroupState(m_RsGroupRoot, m_RsDropdown, opcodeConfirmed);
        SetGroupState(m_RtGroupRoot, m_RtDropdown, opcodeConfirmed);
        SetGroupState(m_ImmediateGroupRoot, m_ImmediateDropdown, opcodeConfirmed);
        SetGroupState(m_FunctGroupRoot, m_FunctDropdown, opcodeConfirmed);

        if (m_ImmediateToggle != null)
        {
            m_ImmediateToggle.gameObject.SetActive(opcodeConfirmed);

            if (m_ImmediateDropdown != null)
                m_ImmediateDropdown.interactable = opcodeConfirmed && m_ImmediateToggle.isOn;
        }

        if (m_FunctToggle != null)
        {
            m_FunctToggle.gameObject.SetActive(opcodeConfirmed);

            if (m_FunctDropdown != null)
                m_FunctDropdown.interactable = opcodeConfirmed && m_FunctToggle.isOn;
        }

        SetTextField(m_BinaryText, instruction != null ? instruction.GetNormalizedBinaryInstruction() : string.Empty);
    }

    /// <summary>
    /// Restores the authored Practice decode controls to a fresh, unsolved state.
    /// </summary>
    public void Reset(ref bool isRefreshing)
    {
        isRefreshing = true;

        ResetDropdown(m_OpcodeDropdown);
        ResetDropdown(m_RsDropdown);
        ResetDropdown(m_RtDropdown);
        ResetDropdown(m_ImmediateDropdown);
        ResetDropdown(m_FunctDropdown);

        if (m_ImmediateToggle != null)
            m_ImmediateToggle.SetIsOnWithoutNotify(false);

        if (m_FunctToggle != null)
            m_FunctToggle.SetIsOnWithoutNotify(false);

        isRefreshing = false;
        SetHintText(string.Empty);
        SetStatusText(string.Empty);
    }

    public void SetVisible(bool isVisible)
    {
        if (m_PracticeRoot != null)
            m_PracticeRoot.SetActive(isVisible);
    }

    public void SetStatusText(string statusText)
    {
        SetTextField(m_StatusText, statusText);
    }

    public void SetHintText(string hintText)
    {
        SetTextField(m_HintText, hintText);
    }

    public string GetSelectedOpcode()
    {
        return GetSelectedDropdownValue(m_OpcodeDropdown);
    }

    public string GetSelectedRs()
    {
        return GetSelectedDropdownValue(m_RsDropdown);
    }

    public string GetSelectedRt()
    {
        return GetSelectedDropdownValue(m_RtDropdown);
    }

    public bool GetImmediateToggleValue()
    {
        return m_ImmediateToggle != null && m_ImmediateToggle.isOn;
    }

    public string GetSelectedImmediate()
    {
        return GetSelectedDropdownValue(m_ImmediateDropdown);
    }

    public bool GetFunctToggleValue()
    {
        return m_FunctToggle != null && m_FunctToggle.isOn;
    }

    public string GetSelectedFunct()
    {
        return GetSelectedDropdownValue(m_FunctDropdown);
    }

    void PopulateStaticOptions(ref bool isRefreshing, PracticeInstructionDefinition instruction)
    {
        PopulateOptions(m_OpcodeDropdown, s_OpcodeOptions, ref isRefreshing);

        if (m_RegisterOptions.Count == 0)
        {
            m_RegisterOptions.Add("Choose Register");
            for (var registerIndex = 0; registerIndex < 32; registerIndex++)
                m_RegisterOptions.Add(System.Convert.ToString(registerIndex, 2).PadLeft(5, '0'));
        }

        PopulateOptions(m_RsDropdown, m_RegisterOptions, ref isRefreshing);
        PopulateOptions(m_RtDropdown, m_RegisterOptions, ref isRefreshing);

        PopulateImmediateOptions(instruction, ref isRefreshing);
        PopulateOptions(m_FunctDropdown, s_FunctOptions, ref isRefreshing);
    }

    void PopulateImmediateOptions(PracticeInstructionDefinition instruction, ref bool isRefreshing)
    {
        m_ImmediateOptions.Clear();
        m_ImmediateOptions.Add("Choose Immediate");

        var expectedImmediateBits = instruction != null ? instruction.expectedImmediateBits : string.Empty;
        if (!string.IsNullOrWhiteSpace(expectedImmediateBits))
            m_ImmediateOptions.Add(expectedImmediateBits.Trim());

        PopulateOptions(m_ImmediateDropdown, m_ImmediateOptions, ref isRefreshing);
    }

    static void PopulateOptions(TMP_Dropdown dropdown, IReadOnlyList<string> options, ref bool isRefreshing)
    {
        if (dropdown == null)
            return;

        if (!ShouldRepopulate(dropdown, options))
            return;

        isRefreshing = true;
        dropdown.ClearOptions();
        dropdown.AddOptions(new List<string>(options));
        dropdown.SetValueWithoutNotify(0);
        isRefreshing = false;
    }

    static void ResetDropdown(TMP_Dropdown dropdown)
    {
        if (dropdown != null)
            dropdown.SetValueWithoutNotify(0);
    }

    static void SetGroupState(GameObject groupRoot, TMP_Dropdown dropdown, bool isActive)
    {
        if (groupRoot != null)
            groupRoot.SetActive(isActive);
        else if (dropdown != null)
            dropdown.gameObject.SetActive(isActive);

        if (dropdown != null)
            dropdown.interactable = isActive;
    }

    static bool ShouldRepopulate(TMP_Dropdown dropdown, IReadOnlyList<string> options)
    {
        if (dropdown == null || dropdown.options == null || dropdown.options.Count != options.Count)
            return true;

        for (var optionIndex = 0; optionIndex < options.Count; optionIndex++)
        {
            if (dropdown.options[optionIndex].text != options[optionIndex])
                return true;
        }

        return false;
    }

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

    static void SetTextField(TMP_Text target, string text)
    {
        if (target == null)
            return;

        target.text = text;
        target.gameObject.SetActive(!string.IsNullOrWhiteSpace(text));
    }
}

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

sealed class PracticeDecodeDropdownField
{
    readonly GameObject m_GroupRoot;
    readonly TMP_Dropdown m_Dropdown;

    public PracticeDecodeDropdownField(GameObject groupRoot, TMP_Dropdown dropdown)
    {
        m_GroupRoot = groupRoot;
        m_Dropdown = dropdown;
    }

    public void SetVisible(bool isVisible)
    {
        if (m_GroupRoot != null)
            m_GroupRoot.SetActive(isVisible);
        else if (m_Dropdown != null)
            m_Dropdown.gameObject.SetActive(isVisible);

        if (m_Dropdown != null)
            m_Dropdown.interactable = isVisible;
    }

    public void Populate(IReadOnlyList<string> options, ref bool isRefreshing)
    {
        if (m_Dropdown == null || !ShouldRepopulate(options))
            return;

        isRefreshing = true;
        m_Dropdown.ClearOptions();
        m_Dropdown.AddOptions(new List<string>(options));
        m_Dropdown.SetValueWithoutNotify(0);
        isRefreshing = false;
    }

    public void Reset()
    {
        if (m_Dropdown != null)
            m_Dropdown.SetValueWithoutNotify(0);
    }

    public string GetSelectedValue()
    {
        if (m_Dropdown == null ||
            m_Dropdown.options == null ||
            m_Dropdown.value <= 0 ||
            m_Dropdown.value >= m_Dropdown.options.Count)
        {
            return string.Empty;
        }

        return m_Dropdown.options[m_Dropdown.value].text.Trim();
    }

    bool ShouldRepopulate(IReadOnlyList<string> options)
    {
        if (m_Dropdown.options == null || m_Dropdown.options.Count != options.Count)
            return true;

        for (var index = 0; index < options.Count; index++)
        {
            if (m_Dropdown.options[index].text != options[index])
                return true;
        }

        return false;
    }
}

sealed class PracticeDecodeOptionalField
{
    readonly GameObject m_GroupRoot;
    readonly Toggle m_Toggle;
    readonly TMP_Dropdown m_Dropdown;

    public PracticeDecodeOptionalField(GameObject groupRoot, Toggle toggle, TMP_Dropdown dropdown)
    {
        m_GroupRoot = groupRoot;
        m_Toggle = toggle;
        m_Dropdown = dropdown;
    }

    public bool IsEnabled => m_Toggle != null && m_Toggle.isOn;
    public string SelectedValue => GetSelectedValue();

    public void RefreshVisibility(bool isVisible)
    {
        if (m_GroupRoot != null)
            m_GroupRoot.SetActive(isVisible);

        if (m_Toggle != null)
            m_Toggle.gameObject.SetActive(isVisible);

        var showDropdown = isVisible && IsEnabled;
        if (m_Dropdown != null)
        {
            m_Dropdown.gameObject.SetActive(showDropdown);
            m_Dropdown.interactable = showDropdown;
        }
    }

    public void Populate(IReadOnlyList<string> options, ref bool isRefreshing)
    {
        if (m_Dropdown == null || !ShouldRepopulate(options))
            return;

        isRefreshing = true;
        m_Dropdown.ClearOptions();
        m_Dropdown.AddOptions(new List<string>(options));
        m_Dropdown.SetValueWithoutNotify(0);
        isRefreshing = false;
    }

    public void Reset()
    {
        if (m_Dropdown != null)
            m_Dropdown.SetValueWithoutNotify(0);

        if (m_Toggle != null)
            m_Toggle.SetIsOnWithoutNotify(false);
    }

    string GetSelectedValue()
    {
        if (m_Dropdown == null ||
            m_Dropdown.options == null ||
            m_Dropdown.value <= 0 ||
            m_Dropdown.value >= m_Dropdown.options.Count)
        {
            return string.Empty;
        }

        return m_Dropdown.options[m_Dropdown.value].text.Trim();
    }

    bool ShouldRepopulate(IReadOnlyList<string> options)
    {
        if (m_Dropdown.options == null || m_Dropdown.options.Count != options.Count)
            return true;

        for (var index = 0; index < options.Count; index++)
        {
            if (m_Dropdown.options[index].text != options[index])
                return true;
        }

        return false;
    }
}

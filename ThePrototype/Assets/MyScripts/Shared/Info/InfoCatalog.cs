using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Shared source of dropdown labels for authored lesson info panels.
/// A Resources asset can override these defaults later, but the runtime
/// fallback keeps the current scene working without extra hookups.
/// </summary>
[CreateAssetMenu(fileName = "InfoCatalog", menuName = "CPU Lesson/Info Catalog")]
public sealed class InfoCatalog : ScriptableObject
{
    const string k_ResourcesPath = "InfoCatalog";

    static InfoCatalog s_RuntimeCatalog;

    [SerializeField]
    PhaseInfoGroup[] m_Groups = Array.Empty<PhaseInfoGroup>();

    /// <summary>
    /// Loads the authored catalog if one exists, otherwise falls back to the
    /// baked-in option labels that match the current project setup.
    /// </summary>
    public static InfoCatalog Load()
    {
        if (s_RuntimeCatalog != null)
            return s_RuntimeCatalog;

        s_RuntimeCatalog = Resources.Load<InfoCatalog>(k_ResourcesPath);
        if (s_RuntimeCatalog != null)
            return s_RuntimeCatalog;

        s_RuntimeCatalog = CreateInstance<InfoCatalog>();
        s_RuntimeCatalog.hideFlags = HideFlags.DontSave;
        s_RuntimeCatalog.m_Groups = BuildDefaultGroups();
        return s_RuntimeCatalog;
    }

    /// <summary>
    /// Rebuilds one dropdown from the requested authored option group.
    /// </summary>
    public void PopulateDropdown(PhaseInfoTopicGroup topicGroup, TMP_Dropdown dropdown, int selectedValue = 0)
    {
        if (dropdown == null)
            return;

        var optionLabels = GetOptionLabels(topicGroup);
        dropdown.ClearOptions();
        dropdown.AddOptions(optionLabels);
        dropdown.SetValueWithoutNotify(Mathf.Clamp(selectedValue, 0, optionLabels.Count - 1));
    }

    List<string> GetOptionLabels(PhaseInfoTopicGroup topicGroup)
    {
        foreach (var group in m_Groups)
        {
            if (group.TopicGroup != topicGroup)
                continue;

            return group.BuildOptions();
        }

        return new List<string> { "Choose Option" };
    }

    static PhaseInfoGroup[] BuildDefaultGroups()
    {
        return new[]
        {
            new PhaseInfoGroup(PhaseInfoTopicGroup.Decode, "Choose Option", "Opcode", "Funct"),
            new PhaseInfoGroup(PhaseInfoTopicGroup.Alu, "Choose Option", "ALUOp", "ALUSrc", "ALU Control"),
            new PhaseInfoGroup(PhaseInfoTopicGroup.Memory, "Choose Option", "MemRead", "MemWrite"),
            new PhaseInfoGroup(PhaseInfoTopicGroup.WriteBack, "Choose Option", "RegDst", "RegWrite", "MemToReg"),
            new PhaseInfoGroup(PhaseInfoTopicGroup.PcUpdate, "Choose Option", "PC", "PCSrc", "Branch", "Jump", "Shift Left 2", "Zero"),
        };
    }

    [Serializable]
    struct PhaseInfoGroup
    {
        [SerializeField]
        PhaseInfoTopicGroup m_TopicGroup;

        [SerializeField]
        string[] m_Options;

        public PhaseInfoGroup(PhaseInfoTopicGroup topicGroup, params string[] options)
        {
            m_TopicGroup = topicGroup;
            m_Options = options ?? Array.Empty<string>();
        }

        public PhaseInfoTopicGroup TopicGroup => m_TopicGroup;

        public List<string> BuildOptions()
        {
            var optionLabels = new List<string>();
            if (m_Options == null || m_Options.Length == 0)
            {
                optionLabels.Add("Choose Option");
                return optionLabels;
            }

            optionLabels.AddRange(m_Options);
            return optionLabels;
        }
    }
}

/// <summary>
/// Groups dropdown-backed info topics by lesson phase.
/// </summary>
public enum PhaseInfoTopicGroup
{
    Decode,
    Alu,
    Memory,
    WriteBack,
    PcUpdate,
}

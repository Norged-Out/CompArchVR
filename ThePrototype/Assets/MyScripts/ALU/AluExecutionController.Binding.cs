using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// Prefab-local wiring helpers for the ALU controller.
/// This partial only owns event subscriptions and child lookups.
/// </summary>
public partial class AluExecutionController
{
    void HookButtons()
    {
        HookPhysicalButton(m_AluOpButtonRoot, HandleAluOpPressed, true);
        HookPhysicalButton(m_AluSrcButtonRoot, HandleAluSrcPressed, true);

        if (m_ExecuteButton != null)
        {
            m_ExecuteButton.onClick.RemoveListener(HandleExecutePressed);
            m_ExecuteButton.onClick.AddListener(HandleExecutePressed);
        }
    }

    void UnhookButtons()
    {
        HookPhysicalButton(m_AluOpButtonRoot, HandleAluOpPressed, false);
        HookPhysicalButton(m_AluSrcButtonRoot, HandleAluSrcPressed, false);

        if (m_ExecuteButton != null)
            m_ExecuteButton.onClick.RemoveListener(HandleExecutePressed);
    }

    void HookDropdown()
    {
        if (m_FunctDropdown == null)
            return;

        m_FunctDropdown.onValueChanged.RemoveListener(HandleFunctDropdownChanged);
        m_FunctDropdown.onValueChanged.AddListener(HandleFunctDropdownChanged);
    }

    void UnhookDropdown()
    {
        if (m_FunctDropdown == null)
            return;

        m_FunctDropdown.onValueChanged.RemoveListener(HandleFunctDropdownChanged);
    }

    void HookHintDropdown()
    {
        if (m_HintDropdown == null)
            return;

        m_HintDropdown.onValueChanged.RemoveListener(HandleHintDropdownChanged);
        m_HintDropdown.onValueChanged.AddListener(HandleHintDropdownChanged);
    }

    void UnhookHintDropdown()
    {
        if (m_HintDropdown == null)
            return;

        m_HintDropdown.onValueChanged.RemoveListener(HandleHintDropdownChanged);
    }

    void HookInputEvents(bool subscribe)
    {
        HookInputEvent(m_InputA, subscribe);
        HookInputEvent(m_InputB, subscribe);
    }

    void HookInputEvent(AluInputScanner inputScanner, bool subscribe)
    {
        if (inputScanner == null)
            return;

        if (subscribe)
        {
            inputScanner.PacketAccepted -= HandlePacketAccepted;
            inputScanner.PacketAccepted += HandlePacketAccepted;
        }
        else
        {
            inputScanner.PacketAccepted -= HandlePacketAccepted;
        }
    }

    void CacheReferences()
    {
        // Only local prefab references are resolved here. Lesson/UI objects are
        // authored in-scene and should be wired explicitly through the Inspector.
        m_InputA ??= FindChildComponent<AluInputScanner>("Input 1");
        m_InputB ??= FindChildComponent<AluInputScanner>("Input 2");
        m_OperationLabelText ??= FindChildText("Screen Canvas/Operation Label");
        m_ResultSpawnTransform ??= transform.Find("Data Packet Spawn");
        m_ComputeParticles ??= GetComponentInChildren<ParticleSystem>(true);
        m_AluOpButtonRoot ??= transform.Find("ALUOp Button");
        m_AluSrcButtonRoot ??= transform.Find("ALUSrc Button");
    }

    static void HookPhysicalButton(
        Transform buttonRoot,
        UnityEngine.Events.UnityAction<SelectEnterEventArgs> handler,
        bool subscribe)
    {
        var button = buttonRoot != null ? buttonRoot.GetComponent<XRSimpleInteractable>() : null;
        if (button == null)
            return;

        if (subscribe)
        {
            button.firstSelectEntered.RemoveListener(handler);
            button.firstSelectEntered.AddListener(handler);
        }
        else
        {
            button.firstSelectEntered.RemoveListener(handler);
        }
    }

    T FindChildComponent<T>(string childName) where T : Component
    {
        var childTransform = transform.Find($"Visuals/{childName}");
        if (childTransform == null)
            childTransform = transform.Find(childName);

        return childTransform != null ? childTransform.GetComponent<T>() : null;
    }

    TMP_Text FindChildText(string childPath)
    {
        var childTransform = transform.Find(childPath);
        return childTransform != null ? childTransform.GetComponent<TMP_Text>() : null;
    }
}

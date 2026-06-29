using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// Owns the authored write-back prefab and WB UI.
/// Responsibilities:
/// - watch the RegDst / RegWrite / MemToReg physical buttons
/// - keep the register and data pedestals synced to those control settings
/// - validate the final write-back transfer
/// - run the authored pipe / particle sequence
/// - notify the lesson flow when write-back has been applied and when the user
///   is ready to continue to recap
/// </summary>
[DisallowMultipleComponent]
public class WriteBackController : MonoBehaviour
{
    [Header("Write-Back Prefab")]
    [SerializeField]
    WriteBackRegisterScanner m_RegisterScanner;

    [SerializeField]
    WriteBackPacketScanner m_PacketScanner;

    [SerializeField]
    ParticleSystem m_TransferParticles;

    [SerializeField]
    Transform m_RegDstButtonRoot;

    [SerializeField]
    Transform m_RegWriteButtonRoot;

    [SerializeField]
    Transform m_MemToRegButtonRoot;

    [SerializeField]
    Renderer[] m_PipeRenderers;

    [Header("WB UI")]
    [SerializeField]
    GameObject m_WbUiRoot;

    [SerializeField]
    TMP_Text m_BodyText;

    [SerializeField]
    TMP_Text m_RegDstStatusText;

    [SerializeField]
    TMP_Text m_RegWriteStatusText;

    [SerializeField]
    TMP_Text m_MemToRegStatusText;

    [SerializeField]
    TMP_Text m_RegisterStatusText;

    [SerializeField]
    TMP_Text m_DataStatusText;

    [SerializeField]
    TMP_Text m_FeedbackText;

    [SerializeField]
    Button m_ActionButton;

    [SerializeField]
    TMP_Text m_ActionButtonLabel;

    [Header("Timing")]
    [SerializeField]
    float m_PipeStepDelaySeconds = 0.3f;

    [SerializeField]
    float m_ParticleLeadTimeSeconds = 0.75f;

    [SerializeField]
    string m_ExecuteButtonText = "Execute Write Back";

    [SerializeField]
    string m_ContinueButtonText = "Continue";

    [SerializeField]
    Color m_SuccessFeedbackColor = new(0.78f, 0.96f, 0.82f, 1f);

    [SerializeField]
    Color m_FailureFeedbackColor = new(1f, 0.55f, 0.55f, 1f);

    InstructionDefinition m_CurrentInstruction;
    RegisterBank m_RegisterBank;
    Coroutine m_TransferRoutine;
    readonly Dictionary<Renderer, Material> m_OriginalPipeMaterials = new();
    bool m_IsPhaseActive;
    bool m_IsAwaitingContinue;
    bool m_HasAppliedWriteBack;
    int m_LastTransferredValue;
    string m_LastTargetRegister = string.Empty;
    DataPacketRole m_LastTransferredPacketRole = DataPacketRole.None;
    string m_RegDstValue = "0";
    string m_RegWriteValue = "0";
    string m_MemToRegValue = "0";

    public event System.Action<string, int> WriteBackApplied;
    public event System.Action ContinueRequested;

    void Awake()
    {
        CacheReferences();
        HookButtons();
        CachePipeMaterials();
        ResetPipeMaterials();
        RefreshPresentation();
        SetFeedback(string.Empty, false);
    }

    void OnEnable()
    {
        CacheReferences();
        HookButtons();
        HookScannerEvents(true);
        CachePipeMaterials();
        RefreshPresentation();
    }

    void OnDisable()
    {
        HookScannerEvents(false);
        UnhookButtons();
    }

    public void SetPhaseState(bool isActive, InstructionDefinition instruction, RegisterBank registerBank)
    {
        CacheReferences();

        var instructionChanged = instruction != null && instruction != m_CurrentInstruction;
        var isEnteringPhase = isActive && !m_IsPhaseActive;

        m_IsPhaseActive = isActive;
        m_CurrentInstruction = instruction != null ? instruction : InstructionDefaults.CreateFallbackAdd();
        m_RegisterBank = registerBank != null ? registerBank : m_RegisterBank;

        if (isEnteringPhase || instructionChanged)
            PrepareForWriteBackStep();

        if (m_WbUiRoot != null)
            m_WbUiRoot.SetActive(isActive);

        m_RegisterScanner?.SetActive(isActive);
        m_PacketScanner?.SetActive(isActive);
        RefreshPresentation();
    }

    public void ResetWriteBackState()
    {
        if (m_TransferRoutine != null)
        {
            StopCoroutine(m_TransferRoutine);
            m_TransferRoutine = null;
        }

        if (m_TransferParticles != null)
            m_TransferParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        m_CurrentInstruction = null;
        m_IsPhaseActive = false;
        m_IsAwaitingContinue = false;
        m_HasAppliedWriteBack = false;
        m_LastTransferredValue = 0;
        m_LastTargetRegister = string.Empty;
        m_LastTransferredPacketRole = DataPacketRole.None;
        m_RegDstValue = "0";
        m_RegWriteValue = "0";
        m_MemToRegValue = "0";

        m_RegisterScanner?.ResetScanner();
        m_PacketScanner?.ResetScanner();
        ResetPipeMaterials();
        SetFeedback(string.Empty, false);
        RefreshPresentation();

        if (m_WbUiRoot != null)
            m_WbUiRoot.SetActive(false);
    }

    public void HandleActionPressed()
    {
        if (!m_IsPhaseActive || m_TransferRoutine != null)
            return;

        if (m_IsAwaitingContinue)
        {
            m_IsAwaitingContinue = false;
            ContinueRequested?.Invoke();
            return;
        }

        if (!TryValidateSetup(out var validationMessage))
        {
            SetFeedback(validationMessage, true);
            RefreshPresentation();
            return;
        }

        SetFeedback("Write-back confirmed. Transferring value into the destination register...", false);
        RefreshPresentation();
        m_TransferRoutine = StartCoroutine(ApplyWriteBackRoutine());
    }

    void PrepareForWriteBackStep()
    {
        if (m_TransferRoutine != null)
        {
            StopCoroutine(m_TransferRoutine);
            m_TransferRoutine = null;
        }

        if (m_TransferParticles != null)
            m_TransferParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        m_RegDstValue = "0";
        m_RegWriteValue = "0";
        m_MemToRegValue = "0";
        m_IsAwaitingContinue = false;
        m_HasAppliedWriteBack = false;
        m_LastTransferredValue = 0;
        m_LastTargetRegister = string.Empty;
        m_LastTransferredPacketRole = DataPacketRole.None;

        m_RegisterScanner?.ResetScanner();
        m_PacketScanner?.ResetScanner();
        ResetPipeMaterials();
        SetFeedback(string.Empty, false);
        RefreshExpectedTargets();
        RefreshPresentation();
    }

    IEnumerator ApplyWriteBackRoutine()
    {
        var destinationRegister = m_RegisterScanner != null && m_RegisterScanner.AcceptedRegister != null
            ? m_RegisterScanner.AcceptedRegister.RegisterId
            : string.Empty;
        var packet = m_PacketScanner != null ? m_PacketScanner.AcceptedPacket : null;
        var packetValue = packet != null ? packet.Value : 0;
        var packetRole = packet != null ? packet.PacketRole : DataPacketRole.None;

        foreach (var pipeRenderer in m_PipeRenderers)
        {
            if (pipeRenderer == null)
                continue;

            if (m_PacketScanner != null && m_PacketScanner.SuccessMaterial != null)
                pipeRenderer.sharedMaterial = m_PacketScanner.SuccessMaterial;

            yield return new WaitForSeconds(m_PipeStepDelaySeconds);
        }

        if (m_TransferParticles != null)
            m_TransferParticles.Play();

        yield return new WaitForSeconds(m_ParticleLeadTimeSeconds);

        if (m_RegisterBank != null && !string.IsNullOrWhiteSpace(destinationRegister))
            m_RegisterBank.SetRegisterValue(destinationRegister, packetValue);

        if (packet != null)
        {
            if (Application.isPlaying)
                Destroy(packet.gameObject);
            else
                DestroyImmediate(packet.gameObject);
        }

        m_PacketScanner?.ConsumeAcceptedPacket();

        m_LastTargetRegister = destinationRegister;
        m_LastTransferredValue = packetValue;
        m_LastTransferredPacketRole = packetRole;
        m_HasAppliedWriteBack = true;
        m_IsAwaitingContinue = true;
        m_TransferRoutine = null;

        WriteBackApplied?.Invoke(destinationRegister, packetValue);
        SetFeedback(
            $"Write-back complete. {destinationRegister} now stores {packetValue}. Click Continue to close the phase.",
            false);
        RefreshPresentation();
    }

    bool TryValidateSetup(out string validationMessage)
    {
        validationMessage = string.Empty;

        if (m_CurrentInstruction == null)
        {
            validationMessage = "No instruction is loaded for write-back.";
            return false;
        }

        var expectedRegWrite = m_CurrentInstruction.GetExpectedRegWriteControlValue();
        if (m_RegWriteValue != expectedRegWrite)
        {
            validationMessage = $"RegWrite is {m_RegWriteValue}, but {m_CurrentInstruction.displayName} needs {expectedRegWrite}.";
            return false;
        }

        var expectedRegDst = m_CurrentInstruction.GetExpectedRegDstControlValue();
        if (m_RegDstValue != expectedRegDst)
        {
            validationMessage = $"RegDst is {m_RegDstValue}, but this instruction needs {expectedRegDst}.";
            return false;
        }

        var expectedMemToReg = m_CurrentInstruction.GetExpectedMemToRegControlValue();
        if (m_MemToRegValue != expectedMemToReg)
        {
            validationMessage = $"MemToReg is {m_MemToRegValue}, but this instruction needs {expectedMemToReg}.";
            return false;
        }

        if (m_RegisterScanner == null || m_RegisterScanner.AcceptedRegister == null)
        {
            validationMessage = $"Register input is still waiting for {GetExpectedRegisterIdFromControlState()}.";
            return false;
        }

        if (m_PacketScanner == null || m_PacketScanner.AcceptedPacket == null)
        {
            validationMessage = $"Data input is still waiting for {GetPacketRoleDisplayName(GetExpectedPacketRoleFromControlState())}.";
            return false;
        }

        return true;
    }

    void HandleRegDstPressed(SelectEnterEventArgs _)
    {
        if (!m_IsPhaseActive || m_HasAppliedWriteBack)
            return;

        m_RegDstValue = m_RegDstValue == "1" ? "0" : "1";
        RefreshExpectedTargets();
        SetFeedback(string.Empty, false);
        RefreshPresentation();
    }

    void HandleRegWritePressed(SelectEnterEventArgs _)
    {
        if (!m_IsPhaseActive || m_HasAppliedWriteBack)
            return;

        m_RegWriteValue = m_RegWriteValue == "1" ? "0" : "1";
        SetFeedback(string.Empty, false);
        RefreshPresentation();
    }

    void HandleMemToRegPressed(SelectEnterEventArgs _)
    {
        if (!m_IsPhaseActive || m_HasAppliedWriteBack)
            return;

        m_MemToRegValue = m_MemToRegValue == "1" ? "0" : "1";
        RefreshExpectedTargets();
        SetFeedback(string.Empty, false);
        RefreshPresentation();
    }

    void HandleRegisterAccepted(WriteBackRegisterScanner _, RegisterToken __)
    {
        SetFeedback(string.Empty, false);
        RefreshPresentation();
    }

    void HandlePacketAccepted(WriteBackPacketScanner _, DataPacketToken __)
    {
        SetFeedback(string.Empty, false);
        RefreshPresentation();
    }

    void RefreshExpectedTargets()
    {
        m_RegisterScanner?.SetExpectedRegisterId(GetExpectedRegisterIdFromControlState());
        m_PacketScanner?.SetExpectedPacketRole(GetExpectedPacketRoleFromControlState());
    }

    string GetExpectedRegisterIdFromControlState()
    {
        if (m_CurrentInstruction == null)
            return string.Empty;

        return m_RegDstValue == "1"
            ? m_CurrentInstruction.expectedRd
            : m_CurrentInstruction.expectedRt;
    }

    DataPacketRole GetExpectedPacketRoleFromControlState()
    {
        if (m_CurrentInstruction == null)
            return DataPacketRole.None;

        return m_MemToRegValue == "1"
            ? DataPacketRole.MemoryData
            : DataPacketRole.AluResult;
    }

    void RefreshPresentation()
    {
        CacheReferences();
        RefreshExpectedTargets();

        if (m_BodyText != null)
        {
            var instructionName = m_CurrentInstruction != null ? m_CurrentInstruction.displayName : "instruction";
            var assembly = m_CurrentInstruction != null ? m_CurrentInstruction.assemblyInstructionText : "add t2, t0, t1";
            var targetRegister = GetExpectedRegisterIdFromControlState();
            var expectedPacket = GetPacketRoleDisplayName(GetExpectedPacketRoleFromControlState());
            m_BodyText.text =
                "Write Back\n\n" +
                $"Instruction: {instructionName}\n\n" +
                $"Assembly: {assembly}\n\n" +
                "1. Set RegWrite, RegDst, and MemToReg for this instruction.\n" +
                $"2. The currently selected destination register is {targetRegister}.\n" +
                $"3. The currently selected write-back source is {expectedPacket}.\n" +
                "4. Place the destination register on the register input.\n" +
                "5. Place the final datapath value on the data input.\n" +
                "6. Execute the transfer to update the register file.";
        }

        if (m_RegWriteStatusText != null)
            m_RegWriteStatusText.text = $"RegWrite: {m_RegWriteValue}";

        if (m_RegDstStatusText != null)
            m_RegDstStatusText.text = $"RegDst: {m_RegDstValue}";

        if (m_MemToRegStatusText != null)
            m_MemToRegStatusText.text = $"MemToReg: {m_MemToRegValue}";

        if (m_RegisterStatusText != null)
        {
            if (m_HasAppliedWriteBack)
            {
                m_RegisterStatusText.text = $"Register Target: {m_LastTargetRegister}";
            }
            else if (m_RegisterScanner == null || m_RegisterScanner.AcceptedRegister == null)
            {
                m_RegisterStatusText.text = $"Register Target: waiting for {GetExpectedRegisterIdFromControlState()}";
            }
            else
            {
                m_RegisterStatusText.text = $"Register Target: {m_RegisterScanner.AcceptedRegister.RegisterId}";
            }
        }

        if (m_DataStatusText != null)
        {
            if (m_HasAppliedWriteBack)
            {
                m_DataStatusText.text = $"Write Data: {GetPacketRoleDisplayName(m_LastTransferredPacketRole)} = {m_LastTransferredValue}";
            }
            else if (m_PacketScanner == null || m_PacketScanner.AcceptedPacket == null)
            {
                m_DataStatusText.text = $"Write Data: waiting for {GetPacketRoleDisplayName(GetExpectedPacketRoleFromControlState())}";
            }
            else
            {
                m_DataStatusText.text =
                    $"Write Data: {GetPacketRoleDisplayName(m_PacketScanner.AcceptedPacket.PacketRole)} = {m_PacketScanner.AcceptedPacket.Value}";
            }
        }

        if (m_ActionButtonLabel != null)
            m_ActionButtonLabel.text = m_IsAwaitingContinue ? m_ContinueButtonText : m_ExecuteButtonText;

        if (m_ActionButton != null)
            m_ActionButton.interactable = m_IsPhaseActive && m_TransferRoutine == null;
    }

    void SetFeedback(string message, bool isFailure)
    {
        if (m_FeedbackText == null)
            return;

        m_FeedbackText.text = message;
        m_FeedbackText.color = isFailure ? m_FailureFeedbackColor : m_SuccessFeedbackColor;
        m_FeedbackText.gameObject.SetActive(!string.IsNullOrWhiteSpace(message));
    }

    void HookButtons()
    {
        HookPhysicalButton(m_RegDstButtonRoot, HandleRegDstPressed, true);
        HookPhysicalButton(m_RegWriteButtonRoot, HandleRegWritePressed, true);
        HookPhysicalButton(m_MemToRegButtonRoot, HandleMemToRegPressed, true);

        if (m_ActionButton != null)
        {
            m_ActionButton.onClick.RemoveListener(HandleActionPressed);
            m_ActionButton.onClick.AddListener(HandleActionPressed);
        }
    }

    void UnhookButtons()
    {
        HookPhysicalButton(m_RegDstButtonRoot, HandleRegDstPressed, false);
        HookPhysicalButton(m_RegWriteButtonRoot, HandleRegWritePressed, false);
        HookPhysicalButton(m_MemToRegButtonRoot, HandleMemToRegPressed, false);

        if (m_ActionButton != null)
            m_ActionButton.onClick.RemoveListener(HandleActionPressed);
    }

    void HookScannerEvents(bool subscribe)
    {
        HookScannerEvent(m_RegisterScanner, subscribe);
        HookPacketEvent(m_PacketScanner, subscribe);
    }

    void HookScannerEvent(WriteBackRegisterScanner scanner, bool subscribe)
    {
        if (scanner == null)
            return;

        if (subscribe)
        {
            scanner.RegisterAccepted -= HandleRegisterAccepted;
            scanner.RegisterAccepted += HandleRegisterAccepted;
        }
        else
        {
            scanner.RegisterAccepted -= HandleRegisterAccepted;
        }
    }

    void HookPacketEvent(WriteBackPacketScanner scanner, bool subscribe)
    {
        if (scanner == null)
            return;

        if (subscribe)
        {
            scanner.PacketAccepted -= HandlePacketAccepted;
            scanner.PacketAccepted += HandlePacketAccepted;
        }
        else
        {
            scanner.PacketAccepted -= HandlePacketAccepted;
        }
    }

    void CacheReferences()
    {
        m_RegisterScanner ??= FindChildComponent<WriteBackRegisterScanner>("Reg Input");
        m_PacketScanner ??= FindChildComponent<WriteBackPacketScanner>("Data Input");
        m_TransferParticles ??= GetComponentInChildren<ParticleSystem>(true);
        m_RegDstButtonRoot ??= transform.Find("RegDst Button");
        m_RegWriteButtonRoot ??= transform.Find("RegWrite Button");
        m_MemToRegButtonRoot ??= transform.Find("MemToReg Button");

        if (m_PipeRenderers == null || m_PipeRenderers.Length == 0)
        {
            var renderers = new List<Renderer>();
            AddPipeRendererIfFound(renderers, "DataPipe");
            AddPipeRendererIfFound(renderers, "Pipe 1");
            AddPipeRendererIfFound(renderers, "Pipe 2");
            AddPipeRendererIfFound(renderers, "Pipe 3");
            AddPipeRendererIfFound(renderers, "RegPipe");
            m_PipeRenderers = renderers.ToArray();
        }

        if (m_WbUiRoot == null)
        {
            var wbUiTransform = FindSceneTransformByName("WB UI");
            m_WbUiRoot = wbUiTransform != null ? wbUiTransform.gameObject : null;
        }

        if (m_WbUiRoot != null)
        {
            m_BodyText ??= FindNamedText(m_WbUiRoot.transform, "Text Body");
            m_RegDstStatusText ??= FindNamedText(m_WbUiRoot.transform, "Text RegDst");
            m_RegWriteStatusText ??= FindNamedText(m_WbUiRoot.transform, "Text RegWrite");
            m_MemToRegStatusText ??= FindNamedText(m_WbUiRoot.transform, "Text MemToReg");
            m_RegisterStatusText ??= FindNamedText(m_WbUiRoot.transform, "Text Input 1");
            m_DataStatusText ??= FindNamedText(m_WbUiRoot.transform, "Text Input 2");
            m_FeedbackText ??= FindNamedText(m_WbUiRoot.transform, "Text Feedback");
            m_ActionButton ??= m_WbUiRoot.GetComponentInChildren<Button>(true);
            m_ActionButtonLabel ??= m_ActionButton != null
                ? m_ActionButton.GetComponentInChildren<TMP_Text>(true)
                : null;
        }
    }

    void CachePipeMaterials()
    {
        m_OriginalPipeMaterials.Clear();

        if (m_PipeRenderers == null)
            return;

        foreach (var pipeRenderer in m_PipeRenderers)
        {
            if (pipeRenderer != null && pipeRenderer.sharedMaterial != null)
                m_OriginalPipeMaterials[pipeRenderer] = pipeRenderer.sharedMaterial;
        }
    }

    void ResetPipeMaterials()
    {
        foreach (var pipeMaterialPair in m_OriginalPipeMaterials)
        {
            if (pipeMaterialPair.Key != null)
                pipeMaterialPair.Key.sharedMaterial = pipeMaterialPair.Value;
        }
    }

    void AddPipeRendererIfFound(List<Renderer> renderers, string objectName)
    {
        var pipeTransform = FindChildRecursive(transform, objectName);
        var pipeRenderer = pipeTransform != null ? pipeTransform.GetComponent<Renderer>() : null;
        if (pipeRenderer != null)
            renderers.Add(pipeRenderer);
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

    static string GetPacketRoleDisplayName(DataPacketRole packetRole)
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

    T FindChildComponent<T>(string childName) where T : Component
    {
        var childTransform = FindChildRecursive(transform, childName);
        return childTransform != null ? childTransform.GetComponent<T>() : null;
    }

    static Transform FindChildRecursive(Transform root, string childName)
    {
        if (root == null)
            return null;

        foreach (var childTransform in root.GetComponentsInChildren<Transform>(true))
        {
            if (childTransform != null && childTransform.name == childName)
                return childTransform;
        }

        return null;
    }

    static TMP_Text FindNamedText(Transform root, string objectName)
    {
        if (root == null)
            return null;

        foreach (var textMesh in root.GetComponentsInChildren<TMP_Text>(true))
        {
            if (textMesh != null && textMesh.name == objectName)
                return textMesh;
        }

        return null;
    }

    static Transform FindSceneTransformByName(string objectName)
    {
        foreach (var sceneTransform in Resources.FindObjectsOfTypeAll<Transform>())
        {
            if (sceneTransform == null || sceneTransform.name != objectName)
                continue;

            if (!sceneTransform.gameObject.scene.IsValid() || !sceneTransform.gameObject.scene.isLoaded)
                continue;

            return sceneTransform;
        }

        return null;
    }
}

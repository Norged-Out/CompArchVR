using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// Owns the authored Memory Unit prefab and the Mem UI.
/// For the current vertical slice it primarily handles:
/// - `lw` memory reads with ALU-result address input
/// - `sw` memory writes with ALU-result address input plus store-data packet
/// - MemRead / MemWrite control validation
/// - highlighting the addressed word in the Data Memory bank
/// - spawning the Memory Data packet used by write-back
/// </summary>
[DisallowMultipleComponent]
public class MemoryUnitController : MonoBehaviour
{
    [Header("Memory Unit Prefab")]
    [SerializeField]
    MemoryAddressScanner m_AddressScanner;

    [SerializeField]
    MemoryPacketScanner m_DataScanner;

    [SerializeField]
    Transform m_MemoryDataSpawnTransform;

    [SerializeField]
    DataPacketToken m_MemoryDataPacketPrefab;

    [SerializeField]
    DataMemoryBank m_MemoryBank;

    [Header("Physical Buttons")]
    [SerializeField]
    Transform m_MemReadButtonRoot;

    [SerializeField]
    Transform m_MemWriteButtonRoot;

    [Header("Mem UI")]
    [SerializeField]
    GameObject m_MemUiRoot;

    [SerializeField]
    TMP_Text m_LessonRuntimeText;

    [SerializeField]
    TMP_Text m_LoadLessonText;

    [SerializeField]
    TMP_Text m_StoreLessonText;

    [SerializeField]
    TMP_Text m_MemReadStatusText;

    [SerializeField]
    TMP_Text m_MemWriteStatusText;

    [SerializeField]
    TMP_Text m_AddressStatusText;

    [SerializeField]
    TMP_Text m_DataStatusText;

    [SerializeField]
    TMP_Text m_FeedbackText;

    [SerializeField]
    Button m_ActionButton;

    [SerializeField]
    TMP_Text m_ActionButtonLabel;

    [SerializeField]
    TMP_Dropdown m_HintDropdown;

    [SerializeField]
    TMP_Text m_HintMemReadText;

    [SerializeField]
    TMP_Text m_HintMemWriteText;

    [Header("Timing")]
    [SerializeField]
    float m_DataSpawnDelaySeconds = 0.75f;

    [SerializeField]
    string m_ExecuteButtonText = "Execute Memory";

    [SerializeField]
    string m_ContinueButtonText = "Continue";

    [SerializeField]
    Color m_SuccessFeedbackColor = new(0.78f, 0.96f, 0.82f, 1f);

    [SerializeField]
    Color m_FailureFeedbackColor = new(1f, 0.55f, 0.55f, 1f);

    InstructionDefinition m_CurrentInstruction;
    Coroutine m_ExecutionRoutine;
    DataPacketToken m_SpawnedMemoryPacket;
    bool m_IsPhaseActive;
    bool m_IsAwaitingContinue;
    bool m_HasCompletedMemoryAccess;
    int m_LastAddress;
    int m_LastLoadedValue;
    string m_MemReadValue = "0";
    string m_MemWriteValue = "0";

    public event System.Action ContinueRequested;

    void Awake()
    {
        CacheReferences();
        PopulateHintDropdown();
        HookActionButton(true);
        HookButtons();
        HookHintDropdown(true);
        HookScannerEvents(true);
        RefreshPresentation();
        SetFeedback(string.Empty, false);
    }

    void OnEnable()
    {
        CacheReferences();
        PopulateHintDropdown();
        HookActionButton(true);
        HookButtons();
        HookHintDropdown(true);
        HookScannerEvents(true);
        RefreshPresentation();
    }

    void OnDisable()
    {
        HookActionButton(false);
        HookHintDropdown(false);
        HookScannerEvents(false);
        UnhookButtons();
    }

    public void SetPhaseState(bool isActive, InstructionDefinition instruction)
    {
        CacheReferences();

        var instructionChanged = instruction != null && instruction != m_CurrentInstruction;
        var isEnteringPhase = isActive && !m_IsPhaseActive;

        m_IsPhaseActive = isActive;
        m_CurrentInstruction = instruction != null ? instruction : InstructionDefaults.CreateFallbackAdd();

        if (isEnteringPhase || instructionChanged)
            PrepareForMemoryStep();

        if (m_MemUiRoot != null)
            m_MemUiRoot.SetActive(isActive);

        var usesInteractiveMemory = isActive && UsesInteractiveMemory();
        m_AddressScanner?.SetActive(usesInteractiveMemory);
        m_DataScanner?.SetActive(usesInteractiveMemory && RequiresDataInput());
        m_MemoryBank?.SetPhaseState(isActive, usesInteractiveMemory);
        RefreshPresentation();
    }

    public void ResetMemoryState()
    {
        if (m_ExecutionRoutine != null)
        {
            StopCoroutine(m_ExecutionRoutine);
            m_ExecutionRoutine = null;
        }

        m_CurrentInstruction = null;
        m_IsPhaseActive = false;
        m_IsAwaitingContinue = false;
        m_HasCompletedMemoryAccess = false;
        m_LastAddress = 0;
        m_LastLoadedValue = 0;
        m_MemReadValue = "0";
        m_MemWriteValue = "0";

        m_AddressScanner?.ResetScanner();
        m_DataScanner?.ResetScanner();
        m_MemoryBank?.StopAllAnimations();
        m_MemoryBank?.SetPhaseState(false, false);
        SetFeedback(string.Empty, false);
        RefreshPresentation();

        if (m_MemUiRoot != null)
            m_MemUiRoot.SetActive(false);
    }

    public void HandleActionPressed()
    {
        if (!m_IsPhaseActive || m_ExecutionRoutine != null)
            return;

        if (!UsesInteractiveMemory())
        {
            ContinueRequested?.Invoke();
            return;
        }

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

        SetFeedback("Memory access confirmed. Performing the transfer...", false);
        RefreshPresentation();
        m_ExecutionRoutine = StartCoroutine(ExecuteMemoryRoutine());
    }

    void PrepareForMemoryStep()
    {
        if (m_ExecutionRoutine != null)
        {
            StopCoroutine(m_ExecutionRoutine);
            m_ExecutionRoutine = null;
        }

        m_IsAwaitingContinue = false;
        m_HasCompletedMemoryAccess = false;
        m_LastAddress = 0;
        m_LastLoadedValue = 0;
        m_MemReadValue = "0";
        m_MemWriteValue = "0";

        m_AddressScanner?.ResetScanner();
        m_DataScanner?.ResetScanner();
        ClearSpawnedMemoryPacket();
        RefreshExpectedTargets();
        SetFeedback(string.Empty, false);
        RefreshPresentation();
    }

    IEnumerator ExecuteMemoryRoutine()
    {
        var addressPacket = m_AddressScanner != null ? m_AddressScanner.AcceptedPacket : null;
        var addressValue = addressPacket != null ? addressPacket.Value : 0;

        if (IsLoadInstruction())
        {
            var transferFinished = m_MemoryBank == null;
            m_MemoryBank?.PlayTransferSequence(true, () => transferFinished = true);
            while (!transferFinished)
                yield return null;

            yield return new WaitForSeconds(m_DataSpawnDelaySeconds);
            if (m_MemoryBank != null && m_MemoryBank.TryReadWord(addressValue, out var loadedValue, out _))
            {
                SpawnMemoryDataPacket(addressValue, loadedValue);
                m_LastLoadedValue = loadedValue;
                m_LastAddress = addressValue;
                SetFeedback($"Memory data ready: loaded {loadedValue} from {FormatAddress(addressValue)}. Click Continue to proceed to Write Back.", false);
            }
        }
        else if (IsStoreInstruction())
        {
            var sourcePacket = m_DataScanner != null ? m_DataScanner.AcceptedPacket : null;
            var transferFinished = m_MemoryBank == null;
            m_MemoryBank?.PlayTransferSequence(false, () => transferFinished = true);
            while (!transferFinished)
                yield return null;

            if (sourcePacket != null && m_MemoryBank != null && m_MemoryBank.TryWriteWord(addressValue, sourcePacket.Value, out _))
            {
                m_LastLoadedValue = sourcePacket.Value;
                m_LastAddress = addressValue;
                SetFeedback($"Stored {sourcePacket.Value} into {FormatAddress(addressValue)}. Click Continue to proceed to Program Counter Update.", false);
            }
        }

        m_HasCompletedMemoryAccess = true;
        m_IsAwaitingContinue = true;
        m_ExecutionRoutine = null;

        m_AddressScanner?.ConsumeAcceptedPacket();

        if (m_DataScanner != null && IsStoreInstruction())
            m_DataScanner.ConsumeAcceptedPacket();

        RefreshPresentation();
    }

    bool TryValidateSetup(out string validationMessage)
    {
        validationMessage = string.Empty;

        if (!UsesInteractiveMemory())
            return true;

        var expectedMemRead = IsLoadInstruction() ? "1" : "0";
        if (m_MemReadValue != expectedMemRead)
        {
            validationMessage = "MemRead is not set for the required memory behavior.";
            return false;
        }

        var expectedMemWrite = IsStoreInstruction() ? "1" : "0";
        if (m_MemWriteValue != expectedMemWrite)
        {
            validationMessage = "MemWrite is not set for the required memory behavior.";
            return false;
        }

        if (m_AddressScanner == null || m_AddressScanner.AcceptedPacket == null)
        {
            validationMessage = "The address input is still missing its packet.";
            return false;
        }

        var addressValue = m_AddressScanner.AcceptedPacket.Value;
        if (m_MemoryBank == null || !m_MemoryBank.TryReadWord(addressValue, out _, out _))
        {
            validationMessage = "That address does not map to a valid memory word in this lesson.";
            return false;
        }

        if (IsStoreInstruction())
        {
            if (m_DataScanner == null || m_DataScanner.AcceptedPacket == null)
            {
                validationMessage = "The data input is still missing its packet.";
                return false;
            }
        }

        return true;
    }

    void HandleMemReadPressed(SelectEnterEventArgs _)
    {
        if (!m_IsPhaseActive || m_HasCompletedMemoryAccess || !UsesInteractiveMemory())
            return;

        m_MemReadValue = m_MemReadValue == "1" ? "0" : "1";
        ResetScannersIfSignalStateIsInvalid();
        SetFeedback(string.Empty, false);
        RefreshPresentation();
    }

    void HandleMemWritePressed(SelectEnterEventArgs _)
    {
        if (!m_IsPhaseActive || m_HasCompletedMemoryAccess || !UsesInteractiveMemory())
            return;

        m_MemWriteValue = m_MemWriteValue == "1" ? "0" : "1";
        ResetScannersIfSignalStateIsInvalid();
        SetFeedback(string.Empty, false);
        RefreshPresentation();
    }

    void HandleAddressAccepted(MemoryAddressScanner _, DataPacketToken packet)
    {
        if (packet == null)
            return;

        m_MemoryBank?.PreviewAddress(packet.Value);
        SetFeedback(string.Empty, false);
        RefreshPresentation();
    }

    void HandleDataAccepted(MemoryPacketScanner _, DataPacketToken _1)
    {
        SetFeedback(string.Empty, false);
        RefreshPresentation();
    }

    void RefreshExpectedTargets()
    {
        m_AddressScanner?.SetExpectedPacketRole(DataPacketRole.AluResult);
        if (m_DataScanner != null)
            m_DataScanner.SetExpectedPacketRole(DataPacketRole.ReadData2);
    }

    void ResetScannersIfSignalStateIsInvalid()
    {
        if (m_CurrentInstruction == null || !UsesInteractiveMemory())
            return;

        var expectedMemRead = IsLoadInstruction() ? "1" : "0";
        var expectedMemWrite = IsStoreInstruction() ? "1" : "0";

        if (m_MemReadValue == expectedMemRead && m_MemWriteValue == expectedMemWrite)
            return;

        m_AddressScanner?.ResetScanner();
        m_DataScanner?.ResetScanner();
        m_MemoryBank?.ClearPreview();
    }

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
            m_ActionButtonLabel.text = UsesInteractiveMemory()
                ? m_IsAwaitingContinue ? m_ContinueButtonText : m_ExecuteButtonText
                : m_ContinueButtonText;

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

    void SpawnMemoryDataPacket(int addressValue, int loadedValue)
    {
        ClearSpawnedMemoryPacket();

        if (m_MemoryDataPacketPrefab == null || m_MemoryDataSpawnTransform == null)
            return;

        var spawnedPacket = Instantiate(
            m_MemoryDataPacketPrefab,
            m_MemoryDataSpawnTransform.position,
            m_MemoryDataSpawnTransform.rotation);
        spawnedPacket.Configure(
            DataPacketRole.MemoryData,
            $"mem_{addressValue}",
            "Memory Data",
            loadedValue);

        m_SpawnedMemoryPacket = spawnedPacket;
    }

    void ClearSpawnedMemoryPacket()
    {
        if (m_SpawnedMemoryPacket == null)
            return;

        if (Application.isPlaying)
            Destroy(m_SpawnedMemoryPacket.gameObject);
        else
            DestroyImmediate(m_SpawnedMemoryPacket.gameObject);

        m_SpawnedMemoryPacket = null;
    }

    bool UsesInteractiveMemory()
    {
        return m_CurrentInstruction != null && m_CurrentInstruction.UsesInteractiveMemoryPhase();
    }

    bool IsLoadInstruction()
    {
        return m_CurrentInstruction != null && m_CurrentInstruction.mnemonic == InstructionMnemonic.Lw;
    }

    bool IsStoreInstruction()
    {
        return m_CurrentInstruction != null && m_CurrentInstruction.mnemonic == InstructionMnemonic.Sw;
    }

    bool RequiresDataInput()
    {
        return IsStoreInstruction();
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
        HookPhysicalButton(m_MemReadButtonRoot, HandleMemReadPressed, true);
        HookPhysicalButton(m_MemWriteButtonRoot, HandleMemWritePressed, true);
    }

    void UnhookButtons()
    {
        HookPhysicalButton(m_MemReadButtonRoot, HandleMemReadPressed, false);
        HookPhysicalButton(m_MemWriteButtonRoot, HandleMemWritePressed, false);
    }

    void HookActionButton(bool subscribe)
    {
        if (m_ActionButton == null)
            return;

        if (subscribe)
        {
            m_ActionButton.onClick.RemoveListener(HandleActionPressed);
            m_ActionButton.onClick.AddListener(HandleActionPressed);
        }
        else
        {
            m_ActionButton.onClick.RemoveListener(HandleActionPressed);
        }
    }

    void HookHintDropdown(bool subscribe)
    {
        if (m_HintDropdown == null)
            return;

        if (subscribe)
        {
            m_HintDropdown.onValueChanged.RemoveListener(HandleHintDropdownChanged);
            m_HintDropdown.onValueChanged.AddListener(HandleHintDropdownChanged);
        }
        else
        {
            m_HintDropdown.onValueChanged.RemoveListener(HandleHintDropdownChanged);
        }
    }

    void HookScannerEvents(bool subscribe)
    {
        HookAddressEvent(subscribe);
        HookDataEvent(subscribe);
    }

    void HookAddressEvent(bool subscribe)
    {
        if (m_AddressScanner == null)
            return;

        if (subscribe)
        {
            m_AddressScanner.PacketAccepted -= HandleAddressAccepted;
            m_AddressScanner.PacketAccepted += HandleAddressAccepted;
        }
        else
        {
            m_AddressScanner.PacketAccepted -= HandleAddressAccepted;
        }
    }

    void HookDataEvent(bool subscribe)
    {
        if (m_DataScanner == null)
            return;

        if (subscribe)
        {
            m_DataScanner.PacketAccepted -= HandleDataAccepted;
            m_DataScanner.PacketAccepted += HandleDataAccepted;
        }
        else
        {
            m_DataScanner.PacketAccepted -= HandleDataAccepted;
        }
    }

    void HandleHintDropdownChanged(int _)
    {
        RefreshPresentation();
    }

    void CacheReferences()
    {
        m_AddressScanner ??= FindChildComponent<MemoryAddressScanner>("Address Input");
        m_DataScanner ??= FindChildComponent<MemoryPacketScanner>("Data Input");
        m_MemReadButtonRoot ??= transform.Find("MemRead Button");
        m_MemWriteButtonRoot ??= transform.Find("MemWrite Button");

        if (m_MemoryDataSpawnTransform == null)
        {
            var pedestalTransform = FindChildRecursive(transform, "Memory Data Pedestal");
            if (pedestalTransform != null)
                m_MemoryDataSpawnTransform = FindChildRecursive(pedestalTransform, "Spawn Point");
        }

        if (m_MemoryBank == null)
            m_MemoryBank = FindFirstSceneObject<DataMemoryBank>();

        if (m_MemUiRoot == null)
        {
            var memUiTransform = FindSceneTransformByName("Mem UI");
            m_MemUiRoot = memUiTransform != null ? memUiTransform.gameObject : null;
        }

        if (m_MemUiRoot != null)
        {
            m_LessonRuntimeText ??= FindNamedText(m_MemUiRoot.transform, "Text Lesson Runtime");
            m_LoadLessonText ??= FindNamedText(m_MemUiRoot.transform, "Load lesson");
            m_StoreLessonText ??= FindNamedText(m_MemUiRoot.transform, "Store lesson");
            m_MemReadStatusText ??= FindNamedText(m_MemUiRoot.transform, "Text MemRead");
            m_MemWriteStatusText ??= FindNamedText(m_MemUiRoot.transform, "Text MemWrite");
            m_AddressStatusText ??= FindNamedText(m_MemUiRoot.transform, "Text Address");
            m_DataStatusText ??= FindNamedText(m_MemUiRoot.transform, "Text Data");
            m_FeedbackText ??= FindNamedText(m_MemUiRoot.transform, "Text Feedback");
            m_HintMemReadText ??= FindNamedText(m_MemUiRoot.transform, "Hint MemRead");
            m_HintMemWriteText ??= FindNamedText(m_MemUiRoot.transform, "Hint MemWrite");
            m_ActionButton ??= m_MemUiRoot.GetComponentInChildren<Button>(true);
            m_ActionButtonLabel ??= m_ActionButton != null
                ? m_ActionButton.GetComponentInChildren<TMP_Text>(true)
                : null;
        }
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

    static void SetHintBlockActive(TMP_Text textBlock, bool isActive)
    {
        if (textBlock == null)
            return;

        textBlock.gameObject.SetActive(isActive);
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

    static T FindFirstSceneObject<T>() where T : Component
    {
        foreach (var component in Resources.FindObjectsOfTypeAll<T>())
        {
            if (component == null)
                continue;

            if (!component.gameObject.scene.IsValid() || !component.gameObject.scene.isLoaded)
                continue;

            return component;
        }

        return null;
    }
}

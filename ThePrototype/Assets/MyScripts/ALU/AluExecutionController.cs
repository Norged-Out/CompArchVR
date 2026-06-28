using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// Owns the execution-phase interaction for the authored ALU prefab.
/// This includes:
/// - the two physical ALU control buttons on the prefab
/// - the ALU UI panel shown during the execute step
/// - packet validation for both ALU inputs
/// - result computation and result-packet spawning
/// </summary>
[DisallowMultipleComponent]
public class AluExecutionController : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField]
    AluInputScanner m_InputA;

    [SerializeField]
    AluInputScanner m_InputB;

    [SerializeField]
    TMP_Text m_OperationLabelText;

    [SerializeField]
    Transform m_ResultSpawnTransform;

    [SerializeField]
    ParticleSystem m_ComputeParticles;

    [SerializeField]
    DataPacketToken m_ResultPacketPrefab;

    [Header("Physical Buttons")]
    [SerializeField]
    Transform m_AluOpButtonRoot;

    [SerializeField]
    Transform m_AluSrcButtonRoot;

    [Header("ALU UI")]
    [SerializeField]
    GameObject m_AluUiRoot;

    [SerializeField]
    TMP_Text m_AluOpStatusText;

    [SerializeField]
    TMP_Text m_AluSrcStatusText;

    [SerializeField]
    TMP_Text m_Input1StatusText;

    [SerializeField]
    TMP_Text m_Input2StatusText;

    [SerializeField]
    TMP_Text m_FeedbackText;

    [SerializeField]
    Button m_ExecuteButton;

    [SerializeField]
    TMP_Text m_ExecuteButtonLabel;

    [Header("Timing")]
    [SerializeField]
    float m_ResultSpawnDelaySeconds = 1.25f;

    [SerializeField]
    string m_ExecuteButtonText = "Execute";

    [SerializeField]
    string m_ResultReadyButtonText = "Continue";

    [SerializeField]
    Color m_DefaultFeedbackColor = Color.white;

    [SerializeField]
    Color m_SuccessFeedbackColor = new(0.78f, 0.96f, 0.82f, 1f);

    [SerializeField]
    Color m_FailureFeedbackColor = new(1f, 0.55f, 0.55f, 1f);

    InstructionDefinition m_CurrentInstruction;
    Coroutine m_ComputeRoutine;
    DataPacketToken m_SpawnedResultPacket;
    bool m_IsPhaseActive;
    bool m_HasProducedResult;
    bool m_IsAwaitingContinue;
    int m_LastResultValue;
    string m_CurrentAluOpValue = "00";
    string m_CurrentAluSrcValue = "0";

    public event System.Action<int> ExecutionCompleted;

    public bool IsPhaseActive => m_IsPhaseActive;
    public string CurrentAluOpValue => m_CurrentAluOpValue;
    public string CurrentAluSrcValue => m_CurrentAluSrcValue;

    void Awake()
    {
        CacheReferences();
        HookButtons();
        RefreshAllPresentation();
        SetFeedback(string.Empty, false);
    }

    void OnEnable()
    {
        CacheReferences();
        HookButtons();
        HookInputEvents(true);
        RefreshAllPresentation();
    }

    void OnDisable()
    {
        HookInputEvents(false);
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
            PrepareForExecutionStep();

        if (m_AluUiRoot != null)
            m_AluUiRoot.SetActive(isActive);

        if (m_ExecuteButton != null)
            m_ExecuteButton.interactable = isActive && !m_HasProducedResult;

        m_InputA?.SetActive(isActive);
        m_InputB?.SetActive(isActive);
        RefreshAllPresentation();
    }

    public void ResetExecutionState()
    {
        if (m_ComputeRoutine != null)
        {
            StopCoroutine(m_ComputeRoutine);
            m_ComputeRoutine = null;
        }

        if (m_ComputeParticles != null)
            m_ComputeParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        m_CurrentInstruction = null;
        m_CurrentAluOpValue = "00";
        m_CurrentAluSrcValue = "0";
        m_HasProducedResult = false;
        m_IsAwaitingContinue = false;
        m_LastResultValue = 0;
        m_IsPhaseActive = false;

        m_InputA?.ResetScanner();
        m_InputB?.ResetScanner();
        ClearSpawnedResultPacket();
        SetFeedback(string.Empty, false);
        RefreshAllPresentation();

        if (m_AluUiRoot != null)
            m_AluUiRoot.SetActive(false);
    }

    public void HandleExecutePressed()
    {
        if (!m_IsPhaseActive || m_ComputeRoutine != null)
            return;

        if (m_HasProducedResult && m_IsAwaitingContinue)
        {
            m_IsAwaitingContinue = false;
            ExecutionCompleted?.Invoke(m_LastResultValue);
            return;
        }

        if (!TryValidateExecutionSetup(out var validationMessage))
        {
            SetFeedback(validationMessage, true);
            RefreshAllPresentation();
            return;
        }

        SetFeedback($"Executing {GetOperationDisplayName()}...", false);
        RefreshAllPresentation();
        m_ComputeRoutine = StartCoroutine(ComputeRoutine());
    }

    void PrepareForExecutionStep()
    {
        m_CurrentAluOpValue = "00";
        m_CurrentAluSrcValue = "0";
        m_HasProducedResult = false;
        m_IsAwaitingContinue = false;
        m_LastResultValue = 0;
        SetFeedback(string.Empty, false);

        if (m_ComputeRoutine != null)
        {
            StopCoroutine(m_ComputeRoutine);
            m_ComputeRoutine = null;
        }

        if (m_ComputeParticles != null)
            m_ComputeParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        ClearSpawnedResultPacket();
        m_InputA?.ResetScanner();
        m_InputB?.ResetScanner();
        RefreshExpectedInputRoles();
        RefreshAllPresentation();
    }

    IEnumerator ComputeRoutine()
    {
        if (m_ComputeParticles != null)
            m_ComputeParticles.Play();

        yield return new WaitForSeconds(m_ResultSpawnDelaySeconds);

        var resultValue = ComputeResult();
        SpawnResultPacket(resultValue);
        m_LastResultValue = resultValue;
        m_HasProducedResult = true;
        m_IsAwaitingContinue = true;
        m_ComputeRoutine = null;

        if (m_ComputeParticles != null)
            m_ComputeParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);

        SetFeedback($"ALU result ready: {resultValue}. Click Continue to proceed to write-back.", false);
        RefreshAllPresentation();
    }

    int ComputeResult()
    {
        var leftValue = m_InputA != null ? m_InputA.AcceptedValue : 0;
        var rightValue = m_InputB != null ? m_InputB.AcceptedValue : 0;
        var operation = ResolveOperation(m_CurrentInstruction, m_CurrentAluOpValue);

        return operation switch
        {
            AluOperation.Subtract => leftValue - rightValue,
            AluOperation.And => leftValue & rightValue,
            AluOperation.Or => leftValue | rightValue,
            AluOperation.SetOnLessThan => leftValue < rightValue ? 1 : 0,
            _ => leftValue + rightValue,
        };
    }

    bool TryValidateExecutionSetup(out string validationMessage)
    {
        validationMessage = string.Empty;

        var expectedAluOp = GetExpectedAluOpValue(m_CurrentInstruction);
        if (m_CurrentAluOpValue != expectedAluOp)
        {
            validationMessage = $"ALUOp is {m_CurrentAluOpValue}, but {m_CurrentInstruction.displayName} needs {expectedAluOp}.";
            return false;
        }

        var expectedAluSrc = m_CurrentInstruction != null && m_CurrentInstruction.usesImmediate ? "1" : "0";
        if (m_CurrentAluSrcValue != expectedAluSrc)
        {
            validationMessage = $"ALUSrc is {m_CurrentAluSrcValue}, but this instruction needs {expectedAluSrc}.";
            return false;
        }

        if (m_InputA == null || m_InputA.AcceptedPacket == null)
        {
            validationMessage = "Input 1 still needs a Read Data 1 packet.";
            return false;
        }

        if (m_InputB == null || m_InputB.AcceptedPacket == null)
        {
            validationMessage = $"Input 2 still needs a {GetRoleDisplayName(GetExpectedInput2Role())} packet.";
            return false;
        }

        if (m_InputA.AcceptedPacket.PacketRole != DataPacketRole.ReadData1)
        {
            validationMessage = "Input 1 has the wrong packet type.";
            return false;
        }

        var expectedInput2Role = GetExpectedInput2Role();
        if (m_InputB.AcceptedPacket.PacketRole != expectedInput2Role)
        {
            validationMessage = $"Input 2 needs {GetRoleDisplayName(expectedInput2Role)}, not {GetRoleDisplayName(m_InputB.AcceptedPacket.PacketRole)}.";
            return false;
        }

        return true;
    }

    void SpawnResultPacket(int resultValue)
    {
        ClearSpawnedResultPacket();

        if (m_ResultPacketPrefab == null || m_ResultSpawnTransform == null)
            return;

        var spawnedPacket = Instantiate(
            m_ResultPacketPrefab,
            m_ResultSpawnTransform.position,
            m_ResultSpawnTransform.rotation);
        spawnedPacket.Configure(
            DataPacketRole.AluResult,
            "alu_result",
            "ALU Result",
            resultValue);

        m_SpawnedResultPacket = spawnedPacket;
    }

    void ClearSpawnedResultPacket()
    {
        if (m_SpawnedResultPacket == null)
            return;

        if (Application.isPlaying)
            Destroy(m_SpawnedResultPacket.gameObject);
        else
            DestroyImmediate(m_SpawnedResultPacket.gameObject);

        m_SpawnedResultPacket = null;
    }

    void HandleAluOpPressed(SelectEnterEventArgs _)
    {
        if (!m_IsPhaseActive || m_HasProducedResult)
            return;

        m_CurrentAluOpValue = m_CurrentAluOpValue switch
        {
            "00" => "01",
            "01" => "10",
            _ => "00",
        };

        SetFeedback(string.Empty, false);
        RefreshAllPresentation();
    }

    void HandleAluSrcPressed(SelectEnterEventArgs _)
    {
        if (!m_IsPhaseActive || m_HasProducedResult)
            return;

        m_CurrentAluSrcValue = m_CurrentAluSrcValue == "1" ? "0" : "1";
        RefreshExpectedInputRoles();
        SetFeedback(string.Empty, false);
        RefreshAllPresentation();
    }

    void HandlePacketAccepted(AluInputScanner _, DataPacketToken __)
    {
        SetFeedback(string.Empty, false);
        RefreshAllPresentation();
    }

    void RefreshExpectedInputRoles()
    {
        m_InputA?.SetExpectedPacketRole(DataPacketRole.ReadData1);
        m_InputB?.SetExpectedPacketRole(GetExpectedInput2Role());
    }

    DataPacketRole GetExpectedInput2Role()
    {
        return m_CurrentAluSrcValue == "1" ? DataPacketRole.Immediate : DataPacketRole.ReadData2;
    }

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
        if (m_AluOpStatusText != null)
            m_AluOpStatusText.text = $"ALUOp: {m_CurrentAluOpValue}";

        if (m_AluSrcStatusText != null)
            m_AluSrcStatusText.text = $"ALUSrc: {m_CurrentAluSrcValue}";

        if (m_Input1StatusText != null)
            m_Input1StatusText.text = BuildInputStatusText("Input 1", DataPacketRole.ReadData1, m_InputA);

        if (m_Input2StatusText != null)
            m_Input2StatusText.text = BuildInputStatusText("Input 2", GetExpectedInput2Role(), m_InputB);

        if (m_ExecuteButtonLabel != null)
            m_ExecuteButtonLabel.text = m_HasProducedResult ? m_ResultReadyButtonText : m_ExecuteButtonText;

        if (m_ExecuteButton != null)
            m_ExecuteButton.interactable = m_IsPhaseActive && m_ComputeRoutine == null;
    }

    string BuildInputStatusText(string inputLabel, DataPacketRole expectedRole, AluInputScanner scanner)
    {
        if (scanner == null || scanner.AcceptedPacket == null)
            return $"{inputLabel}: waiting for {GetRoleDisplayName(expectedRole)}";

        return $"{inputLabel}: {GetRoleDisplayName(scanner.AcceptedPacket.PacketRole)} = {scanner.AcceptedValue}";
    }

    string GetOperationDisplayName()
    {
        return ResolveOperation(m_CurrentInstruction, m_CurrentAluOpValue) switch
        {
            AluOperation.Subtract => "Sub",
            AluOperation.And => "And",
            AluOperation.Or => "Or",
            AluOperation.SetOnLessThan => "Slt",
            _ => "Add",
        };
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
        m_InputA ??= FindChildComponent<AluInputScanner>("Input 1");
        m_InputB ??= FindChildComponent<AluInputScanner>("Input 2");
        m_OperationLabelText ??= FindChildText("Screen Canvas/Operation Label");
        m_ResultSpawnTransform ??= transform.Find("Data Packet Spawn");
        m_ComputeParticles ??= GetComponentInChildren<ParticleSystem>(true);
        m_AluOpButtonRoot ??= transform.Find("ALUOp Button");
        m_AluSrcButtonRoot ??= transform.Find("ALUSrc Button");

        if (m_AluUiRoot == null)
        {
            var aluUiTransform = FindSceneTransformByName("ALU UI");
            m_AluUiRoot = aluUiTransform != null ? aluUiTransform.gameObject : null;
        }

        if (m_AluUiRoot != null)
        {
            m_AluOpStatusText ??= FindNamedText(m_AluUiRoot.transform, "Text ALUOp");
            m_AluSrcStatusText ??= FindNamedText(m_AluUiRoot.transform, "Text ALUSrc");
            m_Input1StatusText ??= FindNamedText(m_AluUiRoot.transform, "Text Input 1");
            m_Input2StatusText ??= FindNamedText(m_AluUiRoot.transform, "Text Input 2");
            m_FeedbackText ??= FindNamedText(m_AluUiRoot.transform, "Text Feedback");
            m_ExecuteButton ??= m_AluUiRoot.GetComponentInChildren<Button>(true);
            m_ExecuteButtonLabel ??= m_ExecuteButton != null
                ? m_ExecuteButton.GetComponentInChildren<TMP_Text>(true)
                : null;
        }
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

    static string GetExpectedAluOpValue(InstructionDefinition instruction)
    {
        if (instruction == null)
            return "00";

        return instruction.mnemonic switch
        {
            InstructionMnemonic.Beq => "01",
            InstructionMnemonic.Bne => "01",
            InstructionMnemonic.Lw => "00",
            InstructionMnemonic.Sw => "00",
            InstructionMnemonic.Addi => "00",
            InstructionMnemonic.Andi => "10",
            InstructionMnemonic.Ori => "10",
            _ => "10",
        };
    }

    static AluOperation ResolveOperation(InstructionDefinition instruction, string aluOpValue)
    {
        if (aluOpValue == "00")
        {
            return instruction != null && instruction.mnemonic == InstructionMnemonic.Andi
                ? AluOperation.And
                : instruction != null && instruction.mnemonic == InstructionMnemonic.Ori
                    ? AluOperation.Or
                    : AluOperation.Add;
        }

        if (aluOpValue == "01")
            return AluOperation.Subtract;

        if (instruction == null)
            return AluOperation.Add;

        return instruction.mnemonic switch
        {
            InstructionMnemonic.Sub => AluOperation.Subtract,
            InstructionMnemonic.And => AluOperation.And,
            InstructionMnemonic.Andi => AluOperation.And,
            InstructionMnemonic.Or => AluOperation.Or,
            InstructionMnemonic.Ori => AluOperation.Or,
            InstructionMnemonic.Slt => AluOperation.SetOnLessThan,
            _ => AluOperation.Add,
        };
    }

    static string GetRoleDisplayName(DataPacketRole packetRole)
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

    enum AluOperation
    {
        Add,
        Subtract,
        And,
        Or,
        SetOnLessThan,
    }
}

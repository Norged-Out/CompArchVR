using TMPro;
using UnityEngine;
using System.Collections;

/// <summary>
/// First-pass ALU stage coordinator used by the standalone ALU prefab.
/// It keeps the hand inputs synced with control decode, computes the ALU
/// operation once both packets are accepted, then spawns an ALU result packet.
/// </summary>
[DisallowMultipleComponent]
public class AluExecutionController : MonoBehaviour
{
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

    [SerializeField]
    ControlDecodeController m_ControlDecodeController;

    [SerializeField]
    InstructionDefinition m_DefaultInstruction;

    [SerializeField]
    float m_ResultSpawnDelaySeconds = 1.25f;

    [SerializeField]
    bool m_IsActive = true;

    public bool IsReady => m_InputA != null && m_InputB != null &&
                           m_InputA.AcceptedPacket != null && m_InputB.AcceptedPacket != null;

    public int CurrentResultValue { get; private set; }

    Coroutine m_ComputeRoutine;
    DataPacketToken m_SpawnedResultPacket;
    string m_LastAluOpValue = string.Empty;
    string m_LastAluSrcValue = string.Empty;
    InstructionDefinition m_LastInstruction;
    bool m_HasResolvedCurrentInputs;

    void Awake()
    {
        CacheReferences();
        RefreshExpectedInputs();
        RefreshOperationLabel();
    }

    void OnEnable()
    {
        CacheReferences();
        HookInputEvents(true);
        RefreshExpectedInputs();
        RefreshOperationLabel();
    }

    void OnDisable()
    {
        HookInputEvents(false);
    }

    public void SetActive(bool isActive)
    {
        m_IsActive = isActive;

        m_InputA?.SetActive(isActive);
        m_InputB?.SetActive(isActive);

        if (!isActive)
        {
            CurrentResultValue = 0;
            m_HasResolvedCurrentInputs = false;
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
        }

        RefreshExpectedInputs();
        RefreshOperationLabel();
    }

    void Update()
    {
        if (!m_IsActive)
            return;

        RefreshExpectedInputs();
        RefreshOperationLabel();

        if (IsReady && m_ComputeRoutine == null && !m_HasResolvedCurrentInputs)
            m_ComputeRoutine = StartCoroutine(ComputeRoutine());
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

    void HandlePacketAccepted(AluInputScanner _, DataPacketToken __)
    {
        m_HasResolvedCurrentInputs = false;
        RefreshExpectedInputs();
        RefreshOperationLabel();

        if (IsReady && m_ComputeRoutine == null && !m_HasResolvedCurrentInputs)
            m_ComputeRoutine = StartCoroutine(ComputeRoutine());
    }

    void CacheReferences()
    {
        m_InputA ??= FindChildComponent<AluInputScanner>("Input 1");
        m_InputB ??= FindChildComponent<AluInputScanner>("Input 2");

        if (m_OperationLabelText == null)
        {
            var operationLabelTransform = transform.Find("Screen Canvas/Operation Label");
            if (operationLabelTransform != null)
                m_OperationLabelText = operationLabelTransform.GetComponent<TMP_Text>();
        }

        m_ResultSpawnTransform ??= transform.Find("Data Packet Spawn");
        m_ComputeParticles ??= GetComponentInChildren<ParticleSystem>(true);
        m_ControlDecodeController ??= FindFirstObjectByType<ControlDecodeController>();
    }

    void RefreshExpectedInputs()
    {
        var instruction = GetCurrentInstruction();
        var aluSrcValue = GetCurrentAluSrcValue(instruction);

        if (instruction == m_LastInstruction && aluSrcValue == m_LastAluSrcValue)
            return;

        m_LastInstruction = instruction;
        m_LastAluSrcValue = aluSrcValue;

        m_InputA?.SetExpectedPacketRole(DataPacketRole.ReadData1);
        m_InputB?.SetExpectedPacketRole(aluSrcValue == "1" ? DataPacketRole.Immediate : DataPacketRole.ReadData2);
    }

    void RefreshOperationLabel()
    {
        if (m_OperationLabelText == null)
            return;

        var instruction = GetCurrentInstruction();
        var aluOpValue = GetCurrentAluOpValue(instruction);
        if (instruction == m_LastInstruction && aluOpValue == m_LastAluOpValue && !string.IsNullOrWhiteSpace(m_OperationLabelText.text))
            return;

        m_LastInstruction = instruction;
        m_LastAluOpValue = aluOpValue;
        m_OperationLabelText.text = $"Operation: {GetOperationLabel(ResolveOperation(instruction, aluOpValue))}";
    }

    IEnumerator ComputeRoutine()
    {
        if (!TryComputeResult(out var resultValue))
        {
            m_ComputeRoutine = null;
            yield break;
        }

        if (m_ComputeParticles != null)
            m_ComputeParticles.Play();

        yield return new WaitForSeconds(m_ResultSpawnDelaySeconds);

        SpawnResultPacket(resultValue);
        m_HasResolvedCurrentInputs = true;

        if (m_ComputeParticles != null)
            m_ComputeParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);

        m_ComputeRoutine = null;
    }

    bool TryComputeResult(out int resultValue)
    {
        resultValue = 0;
        if (!m_IsActive || !IsReady)
            return false;

        var operation = ResolveOperation(GetCurrentInstruction(), GetCurrentAluOpValue(GetCurrentInstruction()));
        var leftValue = m_InputA.AcceptedValue;
        var rightValue = m_InputB.AcceptedValue;

        resultValue = operation switch
        {
            AluOperation.Subtract => leftValue - rightValue,
            AluOperation.And => leftValue & rightValue,
            AluOperation.Or => leftValue | rightValue,
            AluOperation.SetOnLessThan => leftValue < rightValue ? 1 : 0,
            _ => leftValue + rightValue,
        };

        CurrentResultValue = resultValue;
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

        Destroy(m_SpawnedResultPacket.gameObject);
        m_SpawnedResultPacket = null;
    }

    InstructionDefinition GetCurrentInstruction()
    {
        if (m_ControlDecodeController != null && m_ControlDecodeController.CurrentInstruction != null)
            return m_ControlDecodeController.CurrentInstruction;

        return m_DefaultInstruction != null ? m_DefaultInstruction : InstructionDefaults.CreateFallbackAdd();
    }

    string GetCurrentAluSrcValue(InstructionDefinition instruction)
    {
        if (m_ControlDecodeController != null)
            return m_ControlDecodeController.CurrentALUSrcValue;

        return instruction != null && instruction.usesImmediate ? "1" : "0";
    }

    string GetCurrentAluOpValue(InstructionDefinition instruction)
    {
        if (m_ControlDecodeController != null)
            return m_ControlDecodeController.CurrentALUOpValue;

        if (instruction == null)
            return "00";

        return instruction.mnemonic == InstructionMnemonic.Add ? "10" : "00";
    }

    static AluOperation ResolveOperation(InstructionDefinition instruction, string aluOpValue)
    {
        if (aluOpValue == "01")
            return AluOperation.Subtract;

        if (aluOpValue == "00")
            return AluOperation.Add;

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

    static string GetOperationLabel(AluOperation operation)
    {
        return operation switch
        {
            AluOperation.Subtract => "Subtract",
            AluOperation.And => "AND",
            AluOperation.Or => "OR",
            AluOperation.SetOnLessThan => "Set On Less Than",
            _ => "Add",
        };
    }

    T FindChildComponent<T>(string childName) where T : Component
    {
        var childTransform = transform.Find($"Visuals/{childName}");
        if (childTransform == null)
            childTransform = transform.Find(childName);

        return childTransform != null ? childTransform.GetComponent<T>() : null;
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

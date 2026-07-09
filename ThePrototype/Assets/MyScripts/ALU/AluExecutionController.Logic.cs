using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Execution logic for ALU controls, packet validation, and result generation.
/// </summary>
public partial class AluExecutionController
{
    void PrepareForExecutionStep()
    {
        m_CurrentAluOpValue = "00";
        m_CurrentAluSrcValue = "0";
        m_HasProducedResult = false;
        m_IsAwaitingContinue = false;
        m_LastResultValue = 0;
        m_SelectedFunctOperation = ResolveExpectedFunctOperation(m_CurrentInstruction);
        m_HasExplicitFunctSelection = false;
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

        // Once the ALU has produced its result, both input packets have served
        // their purpose and should leave the execution stage.
        m_InputA?.ConsumeAcceptedPacket();
        m_InputB?.ConsumeAcceptedPacket();

        if (m_ComputeParticles != null)
            m_ComputeParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);

        SetFeedback(GetPostExecuteFeedback(resultValue), false);
        RefreshAllPresentation();
    }

    int ComputeResult()
    {
        var leftValue = m_InputA != null ? m_InputA.AcceptedValue : 0;
        var rightValue = m_InputB != null ? m_InputB.AcceptedValue : 0;
        var operation = ResolveCurrentOperation();

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

        // The execute button checks the same logic the learner just configured:
        // first ALU control state, then the physical packets sitting on the inputs.
        var expectedAluOp = GetExpectedAluOpValue(m_CurrentInstruction);
        if (m_CurrentAluOpValue != expectedAluOp)
        {
            validationMessage = "ALUOp is pointing to the wrong operation family.";
            return false;
        }

        var expectedAluSrc = GetExpectedAluSrcValue(m_CurrentInstruction);
        if (m_CurrentAluSrcValue != expectedAluSrc)
        {
            validationMessage = "ALUSrc is routing the second operand down the wrong path.";
            return false;
        }

        if (m_InputA == null || m_InputA.AcceptedPacket == null)
        {
            validationMessage = "Input 1 is still missing its source operand.";
            return false;
        }

        if (m_InputB == null || m_InputB.AcceptedPacket == null)
        {
            validationMessage = "Input 2 is still missing its source operand.";
            return false;
        }

        if (m_InputA.AcceptedPacket.PacketRole != DataPacketRole.ReadData1)
        {
            validationMessage = "Input 1 is not carrying the first register-read value.";
            return false;
        }

        var expectedInput2Role = GetExpectedInput2Role();
        if (m_InputB.AcceptedPacket.PacketRole != expectedInput2Role)
        {
            validationMessage = "Input 2 does not match the operand source selected by ALUSrc.";
            return false;
        }

        if (expectedInput2Role == DataPacketRole.Immediate && !m_InputB.AcceptedPacket.IsSignExtended)
        {
            validationMessage = "The immediate packet is present, but it has not been sign-extended yet.";
            return false;
        }

        if (expectedAluOp == "10")
        {
            if (!m_HasExplicitFunctSelection)
            {
                validationMessage = "Choose an ALU control operation before executing.";
                return false;
            }

            var expectedFunctOperation = ResolveExpectedFunctOperation(m_CurrentInstruction);
            if (m_SelectedFunctOperation != expectedFunctOperation)
            {
                validationMessage = "The selected ALU control operation does not match this instruction.";
                return false;
            }
        }

        return true;
    }

    void SpawnResultPacket(int resultValue)
    {
        ClearSpawnedResultPacket();

        if (m_ResultPacketPrefab == null || m_ResultSpawnTransform == null)
            return;

        var resultPacketRole = GetResultPacketRole();
        var packetValue = resultPacketRole == DataPacketRole.Zero
            ? (resultValue == 0 ? 1 : 0)
            : resultValue;
        var spawnedPacket = Instantiate(
            m_ResultPacketPrefab,
            m_ResultSpawnTransform.position,
            m_ResultSpawnTransform.rotation);
        spawnedPacket.Configure(
            resultPacketRole,
            resultPacketRole == DataPacketRole.Zero ? "zero" : "alu_result",
            resultPacketRole == DataPacketRole.Zero ? "Zero" : "ALU Result",
            packetValue);

        m_SpawnedResultPacket = spawnedPacket;
    }

    DataPacketRole GetResultPacketRole()
    {
        if (m_CurrentInstruction == null)
            return DataPacketRole.AluResult;

        return m_CurrentInstruction.UsesBranchDecision()
            ? DataPacketRole.Zero
            : DataPacketRole.AluResult;
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

    void HandleFunctDropdownChanged(int selectedIndex)
    {
        m_SelectedFunctOperation = GetDropdownOperation(selectedIndex);
        m_HasExplicitFunctSelection = true;
        SetFeedback(string.Empty, false);
        RefreshAllPresentation();
    }

    void HandleHintDropdownChanged(int _)
    {
        RefreshAllPresentation();
    }

    void HandleAluSrcPressed(SelectEnterEventArgs _)
    {
        if (!m_IsPhaseActive || m_HasProducedResult)
            return;

        m_CurrentAluSrcValue = m_CurrentAluSrcValue == "1" ? "0" : "1";

        var expectedInput2Role = GetExpectedInput2Role();
        if (m_InputB != null &&
            m_InputB.AcceptedPacket != null &&
            m_InputB.AcceptedPacket.PacketRole != expectedInput2Role)
        {
            m_InputB.ResetScanner();
            m_InputB.FlashFailure();
        }

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
        // Input 1 is always Read Data 1 in the current datapath slice.
        // Input 2 flips between Read Data 2 and Immediate based on ALUSrc.
        m_InputA?.SetExpectedPacketRole(DataPacketRole.ReadData1);
        m_InputB?.SetExpectedPacketRole(GetExpectedInput2Role());
    }

    DataPacketRole GetExpectedInput2Role()
    {
        return m_CurrentAluSrcValue == "1" ? DataPacketRole.Immediate : DataPacketRole.ReadData2;
    }

    AluOperation ResolveCurrentOperation()
    {
        if (m_CurrentAluOpValue == "10")
            return m_SelectedFunctOperation;

        return ResolveOperation(m_CurrentInstruction, m_CurrentAluOpValue);
    }

    void SyncDropdownToCurrentOperation()
    {
        if (m_FunctDropdown == null || m_FunctDropdown.options == null || m_FunctDropdown.options.Count == 0)
            return;

        var targetIndex = GetDropdownIndexForOperation(m_SelectedFunctOperation);
        if (targetIndex < 0 || targetIndex >= m_FunctDropdown.options.Count)
            targetIndex = 0;

        m_FunctDropdown.SetValueWithoutNotify(targetIndex);
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

    static string GetExpectedAluSrcValue(InstructionDefinition instruction)
    {
        if (instruction == null)
            return "0";

        if (instruction.UsesBranchDecision())
            return "0";

        return instruction.usesImmediate ? "1" : "0";
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

        return ResolveExpectedFunctOperation(instruction);
    }

    AluOperation GetDropdownOperation(int selectedIndex)
    {
        if (m_FunctDropdown == null || selectedIndex < 0 || selectedIndex >= m_FunctDropdown.options.Count)
            return AluOperation.Add;

        var optionText = m_FunctDropdown.options[selectedIndex].text;
        return optionText.ToLowerInvariant() switch
        {
            "subtract" => AluOperation.Subtract,
            "sub" => AluOperation.Subtract,
            "and" => AluOperation.And,
            "or" => AluOperation.Or,
            "slt" => AluOperation.SetOnLessThan,
            "set on less than" => AluOperation.SetOnLessThan,
            _ => AluOperation.Add,
        };
    }

    static int GetDropdownIndexForOperation(AluOperation operation)
    {
        return operation switch
        {
            AluOperation.Subtract => 1,
            AluOperation.And => 2,
            AluOperation.Or => 3,
            AluOperation.SetOnLessThan => 4,
            _ => 0,
        };
    }

    static AluOperation ResolveExpectedFunctOperation(InstructionDefinition instruction)
    {
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
            DataPacketRole.Zero => "Zero",
            _ => "Packet",
        };
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

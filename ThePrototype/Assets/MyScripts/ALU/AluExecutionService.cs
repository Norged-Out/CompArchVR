using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Pure ALU-domain logic shared by the ALU controller and presentation layer.
/// This service owns validation, operation decoding, packet spawning, and the
/// small bits of datapath policy that should not live inside UI code.
/// </summary>
public sealed class AluExecutionService
{
    /// <summary>
    /// Restores the ALU phase to its authored baseline for the newly active
    /// instruction.
    /// </summary>
    public void PrepareForExecution(AluController controller)
    {
        controller.SetCurrentAluOpValue("00");
        controller.SetCurrentAluSrcValue("0");
        controller.SetProducedResultState(false, false, 0);
        controller.SetSelectedFunctOperation(ResolveExpectedFunctOperation(controller.CurrentInstruction), false);
        controller.SetFeedback(string.Empty, false);

        if (controller.ComputeRoutine != null)
        {
            controller.StopCoroutine(controller.ComputeRoutine);
            controller.SetComputeRoutine(null);
        }

        if (controller.ComputeParticles != null)
            controller.ComputeParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        ClearSpawnedResultPacket(controller);
        controller.InputA?.ResetScanner();
        controller.InputB?.ResetScanner();
        RefreshExpectedInputRoles(controller, false);

        if (controller.IsAssessmentMode)
            controller.ShowPracticeBudgetSummary();
    }

    /// <summary>
    /// Validates the authored ALU controls and the two physical input packets.
    /// </summary>
    public bool TryValidateExecutionSetup(AluController controller, out string validationMessage)
    {
        validationMessage = string.Empty;
        var expectedAluOp = GetExpectedAluOpValue(controller.CurrentInstruction);

        if (controller.CurrentAluOpValue != expectedAluOp)
        {
            validationMessage = "ALUOp is pointing to the wrong operation family.";
            return false;
        }

        var expectedAluSrc = GetExpectedAluSrcValue(controller.CurrentInstruction);
        if (controller.CurrentAluSrcValue != expectedAluSrc)
        {
            validationMessage = "ALUSrc is routing the second operand down the wrong path.";
            return false;
        }

        if (controller.InputA == null || controller.InputA.AcceptedPacket == null)
        {
            validationMessage = "Input 1 is still missing its source operand.";
            return false;
        }

        if (controller.InputB == null || controller.InputB.AcceptedPacket == null)
        {
            validationMessage = "Input 2 is still missing its source operand.";
            return false;
        }

        if (controller.InputA.AcceptedPacket.PacketRole != DataPacketRole.ReadData1)
        {
            validationMessage = "Input 1 is not carrying the first register-read value.";
            return false;
        }

        var expectedInput2Role = GetExpectedInput2Role(controller);
        if (controller.InputB.AcceptedPacket.PacketRole != expectedInput2Role)
        {
            validationMessage = "Input 2 does not match the operand source selected by ALUSrc.";
            return false;
        }

        if (expectedInput2Role == DataPacketRole.Immediate && !controller.InputB.AcceptedPacket.IsSignExtended)
        {
            validationMessage = "The immediate packet is present, but it has not been sign-extended yet.";
            return false;
        }

        if (expectedAluOp == "10")
        {
            if (!controller.HasExplicitFunctSelection)
            {
                validationMessage = "Choose an ALU control operation before executing.";
                return false;
            }

            if (controller.SelectedFunctOperation != ResolveExpectedFunctOperation(controller.CurrentInstruction))
            {
                validationMessage = "The selected ALU control operation does not match this instruction.";
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Computes the ALU result from the two currently accepted input packets.
    /// </summary>
    public int ComputeResult(AluController controller)
    {
        var leftValue = controller.InputA != null ? controller.InputA.AcceptedValue : 0;
        var rightValue = controller.InputB != null ? controller.InputB.AcceptedValue : 0;

        return ResolveCurrentOperation(controller) switch
        {
            AluOperation.Subtract => leftValue - rightValue,
            AluOperation.And => leftValue & rightValue,
            AluOperation.Or => leftValue | rightValue,
            AluOperation.SetOnLessThan => leftValue < rightValue ? 1 : 0,
            _ => leftValue + rightValue,
        };
    }

    /// <summary>
    /// Runs the authored ALU execution sequence from particle playback through
    /// result spawning and consumed-input cleanup.
    /// </summary>
    public IEnumerator RunExecutionRoutine(AluController controller)
    {
        if (controller.ComputeParticles != null)
            controller.ComputeParticles.Play();

        yield return new WaitForSeconds(controller.ResultSpawnDelaySeconds);

        var resultValue = ComputeResult(controller);
        SpawnResultPacket(controller, resultValue);
        controller.SetProducedResultState(true, true, resultValue);
        controller.SetComputeRoutine(null);

        controller.InputA?.ConsumeAcceptedPacket();
        controller.InputB?.ConsumeAcceptedPacket();

        if (controller.ComputeParticles != null)
            controller.ComputeParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);

        controller.SetFeedback(AluPresentation.BuildPostExecuteFeedback(controller.CurrentInstruction, resultValue), false);
        controller.RefreshPresentation();
    }

    /// <summary>
    /// Spawns the ALU output packet at the authored result spawn transform.
    /// </summary>
    public void SpawnResultPacket(AluController controller, int resultValue)
    {
        ClearSpawnedResultPacket(controller);

        if (controller.ResultPacketPrefab == null || controller.ResultSpawnTransform == null)
            return;

        var resultPacketRole = GetResultPacketRole(controller.CurrentInstruction);
        var packetValue = resultPacketRole == DataPacketRole.Zero
            ? (resultValue == 0 ? 1 : 0)
            : resultValue;

        var spawnedPacket = UnityEngine.Object.Instantiate(
            controller.ResultPacketPrefab,
            controller.ResultSpawnTransform.position,
            controller.ResultSpawnTransform.rotation);

        spawnedPacket.Configure(
            resultPacketRole,
            resultPacketRole == DataPacketRole.Zero ? "zero" : "alu_result",
            resultPacketRole == DataPacketRole.Zero ? "Zero" : "ALU Result",
            packetValue);

        controller.SetSpawnedResultPacket(spawnedPacket);
    }

    /// <summary>
    /// Removes any ALU result packet left over from a previous attempt.
    /// </summary>
    public void ClearSpawnedResultPacket(AluController controller)
    {
        if (controller.SpawnedResultPacket == null)
            return;

        if (Application.isPlaying)
            UnityEngine.Object.Destroy(controller.SpawnedResultPacket.gameObject);
        else
            UnityEngine.Object.DestroyImmediate(controller.SpawnedResultPacket.gameObject);

        controller.SetSpawnedResultPacket(null);
    }

    /// <summary>
    /// Reconfigures the two ALU inputs to their currently expected packet roles.
    /// When requested, invalid packets on input 2 are forcibly released.
    /// </summary>
    public void RefreshExpectedInputRoles(AluController controller, bool forceResetInvalidInput2)
    {
        controller.InputA?.SetExpectedPacketRole(DataPacketRole.ReadData1);
        controller.InputB?.SetExpectedPacketRole(GetExpectedInput2Role(controller));

        if (!forceResetInvalidInput2 || controller.InputB == null || controller.InputB.AcceptedPacket == null)
            return;

        if (controller.InputB.AcceptedPacket.PacketRole != GetExpectedInput2Role(controller))
        {
            controller.InputB.ResetScanner();
            controller.InputB.FlashFailure();
        }
    }

    /// <summary>
    /// Returns the packet role that input 2 must currently accept.
    /// </summary>
    public DataPacketRole GetExpectedInput2Role(AluController controller)
    {
        return controller.CurrentAluSrcValue == "1" ? DataPacketRole.Immediate : DataPacketRole.ReadData2;
    }

    /// <summary>
    /// Returns the ALU operation currently implied by ALUOp plus any explicit
    /// funct selection.
    /// </summary>
    public AluOperation ResolveCurrentOperation(AluController controller)
    {
        if (controller.CurrentAluOpValue == "10")
            return controller.SelectedFunctOperation;

        return ResolveOperation(controller.CurrentInstruction, controller.CurrentAluOpValue);
    }

    /// <summary>
    /// Converts a funct dropdown choice into the enum consumed by runtime ALU
    /// logic.
    /// </summary>
    public AluOperation GetDropdownOperation(TMP_Dropdown dropdown, int selectedIndex)
    {
        if (dropdown == null || selectedIndex < 0 || selectedIndex >= dropdown.options.Count)
            return AluOperation.Add;

        var optionText = dropdown.options[selectedIndex].text.Trim().ToLowerInvariant();
        return optionText switch
        {
            "subtract" => AluOperation.Subtract,
            "sub" => AluOperation.Subtract,
            "and" => AluOperation.And,
            "or" => AluOperation.Or,
            "slt" => AluOperation.SetOnLessThan,
            "set less than" => AluOperation.SetOnLessThan,
            "set on less than" => AluOperation.SetOnLessThan,
            _ => AluOperation.Add,
        };
    }

    public static string GetExpectedAluOpValue(InstructionDefinition instruction)
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

    public static string GetExpectedAluSrcValue(InstructionDefinition instruction)
    {
        if (instruction == null)
            return "0";

        if (instruction.UsesBranchDecision())
            return "0";

        return instruction.usesImmediate ? "1" : "0";
    }

    public static AluOperation ResolveExpectedFunctOperation(InstructionDefinition instruction)
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

    static DataPacketRole GetResultPacketRole(InstructionDefinition instruction)
    {
        return instruction != null && instruction.UsesBranchDecision()
            ? DataPacketRole.Zero
            : DataPacketRole.AluResult;
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

}

/// <summary>
/// Supported ALU operations for the current datapath slice.
/// </summary>
public enum AluOperation
{
    Add,
    Subtract,
    And,
    Or,
    SetOnLessThan,
}

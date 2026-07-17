using System;
using UnityEngine;

/// <summary>
/// Decode-stage register validation, scanner configuration, and immediate generation.
/// </summary>
sealed class DecodeFlow
{
    readonly CpuLessonFlow m_Flow;
    readonly LessonState m_State;

    /// <summary>
    /// Captures the flow model and mutable lesson state used by the decode stage.
    /// </summary>
    public DecodeFlow(CpuLessonFlow flow, LessonState state)
    {
        m_Flow = flow;
        m_State = state;
    }

    /// <summary>
    /// Subscribes the register bank callbacks used by decode scanning.
    /// </summary>
    public void Bind()
    {
        if (m_Flow.RegisterBankRef == null)
            return;

        m_Flow.RegisterBankRef.RegisterPressed -= HandleRegisterPressed;
        m_Flow.RegisterBankRef.RegisterPressed += HandleRegisterPressed;
        m_Flow.RegisterBankRef.RegisterScanned -= HandleRegisterScanned;
        m_Flow.RegisterBankRef.RegisterScanned += HandleRegisterScanned;
    }

    /// <summary>
    /// Removes the register bank callbacks used by decode scanning.
    /// </summary>
    public void Unbind()
    {
        if (m_Flow.RegisterBankRef == null)
            return;

        m_Flow.RegisterBankRef.RegisterPressed -= HandleRegisterPressed;
        m_Flow.RegisterBankRef.RegisterScanned -= HandleRegisterScanned;
    }

    /// <summary>
    /// Resets register transforms and scanner caches for a new run while
    /// preserving the current logical register values.
    /// </summary>
    public void PrepareRegisterBankForLesson()
    {
        if (m_Flow.RegisterBankRef == null)
            return;

        m_Flow.RegisterBankRef.RefreshRegisterCache();
        m_Flow.RegisterBankRef.RefreshScannerCache();
        m_Flow.RegisterBankRef.ResetAllRegisters();
    }

    /// <summary>
    /// Activates only the scanner roles needed by the currently authored decode step.
    /// </summary>
    public void ConfigureScanners(InstructionFlowStep step)
    {
        if (m_Flow.RegisterBankRef == null)
            return;

        if (step == null)
        {
            m_Flow.RegisterBankRef.ConfigureScannerRoles(Array.Empty<InstructionRegisterRole>());
            return;
        }

        if (step.requiredInteraction != InstructionStepInteractionType.RegisterSelection)
        {
            m_Flow.RegisterBankRef.ConfigureScannerRoles(Array.Empty<InstructionRegisterRole>());
            return;
        }

        m_State.MarkRegisterSelectionReady(false);

        var activeRoles = LessonChecks.GetRequiredRoles(m_Flow.ActiveInstruction, step);
        m_Flow.RegisterBankRef.ConfigureScannerRoles(activeRoles);
        m_Flow.RegisterBankRef.SetScannerOutputRole(InstructionRegisterRole.Rs, DataPacketRole.ReadData1);
        m_Flow.RegisterBankRef.SetScannerOutputRole(InstructionRegisterRole.Rt, m_Flow.ActiveInstruction.GetDecodeRtPacketRole());
    }

    /// <summary>
    /// Spawns the authored immediate packet once decode is ready to hand work over to execution.
    /// </summary>
    public bool TrySpawnImmediatePacket()
    {
        if (m_Flow.ActiveInstruction == null || !m_Flow.ActiveInstruction.usesImmediate)
            return false;

        if (m_Flow.ImmediateExtenderRef == null)
            return false;

        m_State.RuntimeSelection.immediateValue = m_Flow.ActiveInstruction.expectedImmediateValue;
        return m_Flow.ImmediateExtenderRef.SpawnImmediatePacket(m_Flow.ActiveInstruction.expectedImmediateValue);
    }

    /// <summary>
    /// Converts a logical register role into the data packet label it should emit.
    /// </summary>
    public static string GetPacketLabel(InstructionRegisterRole registerRole)
    {
        return registerRole switch
        {
            InstructionRegisterRole.Rs => "Read Data 1",
            InstructionRegisterRole.Rt => "Read Data 2",
            _ => "data",
        };
    }

    /// <summary>
    /// Converts a packet role into the learner-facing packet name used across later phases.
    /// </summary>
    public static string GetPacketLabel(DataPacketRole packetRole)
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

    /// <summary>
    /// Handles the fallback register-button route used when a learner presses a register directly.
    /// </summary>
    void HandleRegisterPressed(string registerName)
    {
        if (!m_State.HasStarted || string.IsNullOrWhiteSpace(registerName) || m_Flow.CurrentStep == null)
            return;

        if (m_Flow.CurrentStep.requiredInteraction == InstructionStepInteractionType.RegisterSelection)
            ValidateRegisterSelection(InstructionRegisterRole.None, registerName, false);
    }

    /// <summary>
    /// Handles scanner-based register validation during the decode phase.
    /// </summary>
    void HandleRegisterScanned(InstructionRegisterRole scannedRole, string registerName)
    {
        if (!m_State.HasStarted || string.IsNullOrWhiteSpace(registerName) || m_Flow.CurrentStep == null)
            return;

        if (m_Flow.CurrentStep.requiredInteraction != InstructionStepInteractionType.RegisterSelection)
            return;

        Debug.Log(
            $"{m_Flow.LogPrefix} HandleRegisterScanned | role={scannedRole} register={registerName} selectionIndex={m_State.CurrentRegisterSelectionIndex} step={m_Flow.CurrentStep.stepName} frame={Time.frameCount}",
            m_Flow);
        ValidateRegisterSelection(scannedRole, registerName, true);
    }

    /// <summary>
    /// Validates the current register against the authored decode order and updates scanner state.
    /// </summary>
    void ValidateRegisterSelection(InstructionRegisterRole scannedRole, string registerName, bool cameFromScanner)
    {
        var result = LessonChecks.ValidateRegisterSelection(
            m_Flow.ActiveInstruction,
            m_Flow.CurrentStep,
            m_State.CurrentRegisterSelectionIndex,
            registerName);

        var expectedRole = result.expectedRole;
        if (cameFromScanner && scannedRole != expectedRole)
        {
            m_Flow.RegisterBankRef?.FlashScannerFailure(scannedRole);
            m_Flow.RaiseFeedback("That operand does not belong on this scanner.", true);
            return;
        }

        if (!result.isCorrect)
        {
            if (cameFromScanner)
                m_Flow.RegisterBankRef?.FlashScannerFailure(scannedRole);
            else
                m_Flow.RegisterBankRef?.FlashFailure(registerName);

            m_Flow.RaiseFeedback("That register does not match the current decode target.", true);
            return;
        }

        m_State.RuntimeSelection.SetSelectedRegister(result.expectedRole, registerName);
        m_State.AdvanceRegisterSelection();

        // Only rs / rt emit packets during decode. The destination register is
        // still validated here, but write-back owns the actual result transfer.
        if (cameFromScanner)
            m_Flow.RegisterBankRef?.SetScannerSuccess(scannedRole);
        else
            m_Flow.RegisterBankRef?.SetSelected(registerName);

        if (result.completesStep)
        {
            m_State.MarkRegisterSelectionReady(true);
            var completionMessage = "Operand collection is complete.";

            if (m_Flow.ActiveInstruction != null && m_Flow.ActiveInstruction.usesImmediate)
                completionMessage += " Press Continue to generate the immediate value for the next stage.";

            m_Flow.RaiseFeedback(completionMessage, false);
            Debug.Log(
                $"{m_Flow.LogPrefix} Register selection complete | step={m_Flow.CurrentStep.stepName} nextStepPending=true frame={Time.frameCount}",
                m_Flow);
            m_Flow.RaiseStepChanged();
            return;
        }

        m_State.MarkRegisterSelectionReady(false);
        m_Flow.RaiseFeedback("Operand confirmed.", false);
        m_Flow.RaiseStepChanged();
    }
}

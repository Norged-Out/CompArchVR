using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Decode-stage register validation, scanner configuration, and immediate generation.
/// </summary>
sealed class DecodeFlow
{
    readonly CpuLessonFlow m_Flow;
    readonly LessonState m_State;
    readonly HashSet<string> m_ConsumedPracticeScannerFailures = new(StringComparer.Ordinal);
    int m_MaxPracticeScannerAttempts = 3;
    int m_RemainingPracticeScannerAttempts = 3;
    bool m_IsPracticeScannerFailureAwaitingReset;

    /// <summary>
    /// Captures the flow model and mutable lesson state used by the decode stage.
    /// </summary>
    public DecodeFlow(CpuLessonFlow flow, LessonState state)
    {
        m_Flow = flow;
        m_State = state;
    }

    public bool IsPracticeScannerFailureAwaitingReset => m_IsPracticeScannerFailureAwaitingReset;

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

        ResetPracticeScannerBudget();
        m_Flow.RegisterBankRef.RefreshRegisterCache();
        m_Flow.RegisterBankRef.RefreshScannerCache();
        m_Flow.RegisterBankRef.ResetAllRegisters();
    }

    /// <summary>
    /// Lets the lesson guide choose how many failed Practice-mode register
    /// scans the learner can spend during decode.
    /// </summary>
    public void ConfigurePracticeScannerAttempts(int maxAttempts)
    {
        m_MaxPracticeScannerAttempts = Mathf.Max(1, maxAttempts);
        ResetPracticeScannerBudget();
    }

    /// <summary>
    /// Completes the entire decode phase immediately by stamping the authored
    /// operand selections and advancing until decode no longer owns the lesson.
    /// </summary>
    public void ForceCompleteDecodePhase()
    {
        if (!m_State.HasStarted || m_Flow.CurrentStep == null || m_Flow.ActiveInstruction == null)
            return;

        while (m_Flow.CurrentStep != null &&
               (m_Flow.CurrentStep.highlightedNode == DatapathNodeId.InstructionMemory ||
                m_Flow.CurrentStep.requiredInteraction == InstructionStepInteractionType.RegisterSelection))
        {
            var currentStep = m_Flow.CurrentStep;
            if (currentStep.requiredInteraction == InstructionStepInteractionType.RegisterSelection)
                ForceCompleteRegisterSelection(currentStep);

            m_Flow.ProgressRef.ForceAdvanceStep();
        }
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

        if (m_IsPracticeScannerFailureAwaitingReset)
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

        if (m_IsPracticeScannerFailureAwaitingReset)
            return;

        Debug.Log(
            $"{m_Flow.LogPrefix} HandleRegisterScanned | role={scannedRole} register={registerName} completedSelections={m_State.CurrentRegisterSelectionIndex} step={m_Flow.CurrentStep.stepName} frame={Time.frameCount}",
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
            m_State.SelectedRegisterRoles,
            scannedRole,
            registerName);

        if (!result.isCorrect)
        {
            if (cameFromScanner)
                m_Flow.RegisterBankRef?.FlashScannerFailure(scannedRole);
            else
                m_Flow.RegisterBankRef?.FlashFailure(registerName);

            RaiseRegisterSelectionFailure("That register does not match the current decode target.", scannedRole, registerName, cameFromScanner);
            return;
        }

        m_State.RuntimeSelection.SetSelectedRegister(result.matchedRole, registerName);
        m_State.MarkRegisterRoleSelected(result.matchedRole);

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

    void ResetPracticeScannerBudget()
    {
        m_RemainingPracticeScannerAttempts = m_MaxPracticeScannerAttempts;
        m_IsPracticeScannerFailureAwaitingReset = false;
        m_ConsumedPracticeScannerFailures.Clear();
    }

    void RaiseRegisterSelectionFailure(string failureMessage, InstructionRegisterRole scannedRole, string registerName, bool cameFromScanner)
    {
        if (LessonModePolicy.IsAssessmentMode(m_Flow.CurrentMode) && cameFromScanner)
        {
            ConsumePracticeScannerFailure(failureMessage, scannedRole, registerName);
            return;
        }

        m_Flow.RaiseFeedback(failureMessage, true);
    }

    void ConsumePracticeScannerFailure(string failureMessage, InstructionRegisterRole scannedRole, string registerName)
    {
        var failureKey = BuildPracticeScannerFailureKey(failureMessage, scannedRole, registerName);
        if (!m_ConsumedPracticeScannerFailures.Add(failureKey))
        {
            m_Flow.RaiseFeedback(
                $"{failureMessage}\nScanner attempts remaining: {m_RemainingPracticeScannerAttempts}",
                true);
            return;
        }

        m_RemainingPracticeScannerAttempts = Mathf.Max(0, m_RemainingPracticeScannerAttempts - 1);
        var feedbackText = $"{failureMessage}\nScanner attempts remaining: {m_RemainingPracticeScannerAttempts}";

        if (m_RemainingPracticeScannerAttempts > 0)
        {
            m_Flow.RaiseFeedback(feedbackText, true);
            return;
        }

        m_IsPracticeScannerFailureAwaitingReset = true;
        m_Flow.RegisterBankRef?.ConfigureScannerRoles(Array.Empty<InstructionRegisterRole>());
        m_Flow.RaiseFeedback($"{feedbackText}\nPress Restart to reset the lesson.", true);
        m_Flow.NotifyPracticeDecodeScannerFailed();
    }

    void ForceCompleteRegisterSelection(InstructionFlowStep step)
    {
        var instruction = m_Flow.ActiveInstruction;
        if (instruction == null || step == null)
            return;

        var requiredRoles = LessonChecks.GetRequiredRoles(instruction, step);
        foreach (var requiredRole in requiredRoles)
        {
            m_State.RuntimeSelection.SetSelectedRegister(requiredRole, instruction.GetExpectedRegisterName(requiredRole));
            SpawnForcedDecodePacket(requiredRole, instruction);
        }

        if (!string.IsNullOrWhiteSpace(instruction.expectedRd))
            m_State.RuntimeSelection.SetSelectedRegister(InstructionRegisterRole.Rd, instruction.expectedRd);

        if (instruction.usesImmediate)
        {
            m_State.RuntimeSelection.immediateValue = instruction.expectedImmediateValue;
            TrySpawnImmediatePacket();
        }

        m_State.ForceRegisterSelectionComplete(requiredRoles);
    }

    string BuildPracticeScannerFailureKey(string failureMessage, InstructionRegisterRole scannedRole, string registerName)
    {
        return $"{m_State.CurrentStepIndex}:{m_State.CurrentRegisterSelectionIndex}:{scannedRole}:{registerName}:{failureMessage}";
    }

    void SpawnForcedDecodePacket(InstructionRegisterRole role, InstructionDefinition instruction)
    {
        if (m_Flow.RegisterBankRef == null || instruction == null)
            return;

        if (role != InstructionRegisterRole.Rs && role != InstructionRegisterRole.Rt)
            return;

        var registerId = instruction.GetExpectedRegisterName(role);
        if (string.IsNullOrWhiteSpace(registerId))
            return;

        var packetRole = role == InstructionRegisterRole.Rs
            ? DataPacketRole.ReadData1
            : instruction.GetDecodeRtPacketRole();

        m_Flow.RegisterBankRef.SpawnPacketFromScanner(
            role,
            packetRole,
            registerId,
            m_Flow.RegisterBankRef.GetRegisterDisplayLabel(registerId),
            m_Flow.RegisterBankRef.GetRegisterValue(registerId));
    }
}

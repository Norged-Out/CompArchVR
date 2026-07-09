using System;
using UnityEngine;

public partial class CpuLessonFlow
{
    void RebindRegisterBank()
    {
        if (m_RegisterBank == null)
            return;

        m_RegisterBank.RegisterPressed -= HandleRegisterPressed;
        m_RegisterBank.RegisterPressed += HandleRegisterPressed;
        m_RegisterBank.RegisterScanned -= HandleRegisterScanned;
        m_RegisterBank.RegisterScanned += HandleRegisterScanned;
    }

    void HandleRegisterPressed(string registerName)
    {
        if (!HasStarted || string.IsNullOrWhiteSpace(registerName) || CurrentStep == null)
            return;

        if (CurrentStep.requiredInteraction == InstructionStepInteractionType.RegisterSelection)
            ValidateRegisterSelection(InstructionRegisterRole.None, registerName, false);
    }

    void HandleRegisterScanned(InstructionRegisterRole scannedRole, string registerName)
    {
        if (!HasStarted || string.IsNullOrWhiteSpace(registerName) || CurrentStep == null)
            return;

        if (CurrentStep.requiredInteraction != InstructionStepInteractionType.RegisterSelection)
            return;

        Debug.Log(
            $"{k_LogPrefix} HandleRegisterScanned | role={scannedRole} register={registerName} selectionIndex={m_CurrentRegisterSelectionIndex} step={CurrentStep.stepName} frame={Time.frameCount}",
            this);
        ValidateRegisterSelection(scannedRole, registerName, true);
    }

    /// <summary>
    /// Central decode-stage validator for both direct token grabs and scanner
    /// pedestal scans. LessonChecks determines correctness, while this method
    /// applies the authored scene feedback for success/failure.
    /// </summary>
    void ValidateRegisterSelection(InstructionRegisterRole scannedRole, string registerName, bool cameFromScanner)
    {
        var result = LessonChecks.ValidateRegisterSelection(
            m_CurrentInstruction,
            CurrentStep,
            m_CurrentRegisterSelectionIndex,
            registerName);

        var expectedRole = result.expectedRole;
        if (cameFromScanner && scannedRole != expectedRole)
        {
            m_RegisterBank?.FlashScannerFailure(scannedRole);
            SetFeedback("That operand does not belong on this scanner.", true);
            return;
        }

        if (!result.isCorrect)
        {
            if (cameFromScanner)
                m_RegisterBank?.FlashScannerFailure(scannedRole);
            else
                m_RegisterBank?.FlashFailure(registerName);

            SetFeedback("That register does not match the current decode target.", true);
            return;
        }

        m_RuntimeSelection.SetSelectedRegister(result.expectedRole, registerName);
        m_CurrentRegisterSelectionIndex++;

        // Only rs / rt emit packets during decode. The destination register is
        // still validated here, but write-back owns the actual result transfer.
        if (cameFromScanner)
            m_RegisterBank?.SetScannerSuccess(scannedRole);
        else
            m_RegisterBank?.SetSelected(registerName);

        if (result.completesStep)
        {
            m_RegisterSelectionReadyToContinue = true;
            var completionMessage = "Operand collection is complete.";

            if (m_CurrentInstruction != null && m_CurrentInstruction.usesImmediate)
                completionMessage += " Press Continue to generate the immediate value for the next stage.";

            SetFeedback(completionMessage, false);
            Debug.Log(
                $"{k_LogPrefix} Register selection complete | step={CurrentStep.stepName} nextStepPending=true frame={Time.frameCount}",
                this);
            StepChanged?.Invoke(this);
            return;
        }

        m_RegisterSelectionReadyToContinue = false;
        SetFeedback("Operand confirmed.", false);
        StepChanged?.Invoke(this);
    }

    void ConfigureScannersForCurrentStep()
    {
        if (m_RegisterBank == null)
            return;

        if (CurrentStep == null)
        {
            m_RegisterBank.ConfigureScannerRoles(Array.Empty<InstructionRegisterRole>());
            return;
        }

        switch (CurrentStep.requiredInteraction)
        {
            case InstructionStepInteractionType.RegisterSelection:
                m_RegisterSelectionReadyToContinue = false;
                ConfigureRegisterDecodeScanners();
                break;

            default:
                m_RegisterBank.ConfigureScannerRoles(Array.Empty<InstructionRegisterRole>());
                break;
        }
    }

    void ConfigureRegisterDecodeScanners()
    {
        if (m_RegisterBank == null)
            return;

        var activeRoles = LessonChecks.GetRequiredRoles(m_CurrentInstruction, CurrentStep);
        m_RegisterBank.ConfigureScannerRoles(activeRoles);
        m_RegisterBank.SetScannerOutputRole(InstructionRegisterRole.Rs, DataPacketRole.ReadData1);
        m_RegisterBank.SetScannerOutputRole(InstructionRegisterRole.Rt, m_CurrentInstruction.GetDecodeRtPacketRole());
    }

    static string GetScannerLabel(InstructionRegisterRole registerRole)
    {
        return registerRole switch
        {
            InstructionRegisterRole.Rs => "Read Register 1",
            InstructionRegisterRole.Rt => "Read Register 2",
            InstructionRegisterRole.Rd => "Write Register",
            _ => "the correct",
        };
    }

    static string GetPacketLabel(InstructionRegisterRole registerRole)
    {
        return registerRole switch
        {
            InstructionRegisterRole.Rs => "Read Data 1",
            InstructionRegisterRole.Rt => "Read Data 2",
            _ => "data",
        };
    }

    static string GetPacketLabel(DataPacketRole packetRole)
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
}

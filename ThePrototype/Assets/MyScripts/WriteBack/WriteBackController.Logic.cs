using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Write-back validation, pipe animation, and register update logic.
/// </summary>
public partial class WriteBackController
{
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

        m_LastTargetRegister = destinationRegister;
        m_LastTransferredValue = packetValue;
        m_LastTransferredPacketRole = packetRole;
        m_HasAppliedWriteBack = true;
        m_IsAwaitingContinue = true;
        m_TransferRoutine = null;

        m_PacketScanner?.ConsumeAcceptedPacket();
        SetFeedback(
            $"Write-back complete. {destinationRegister} now stores {packetValue}. Click Continue to proceed to Program Counter Update.",
            false);
        RefreshPresentation();
        WriteBackApplied?.Invoke(destinationRegister, packetValue);
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
            validationMessage = "RegWrite does not match the behavior needed by this instruction.";
            return false;
        }

        var expectedRegDst = m_CurrentInstruction.GetExpectedRegDstControlValue();
        if (m_RegDstValue != expectedRegDst)
        {
            validationMessage = "RegDst is selecting the wrong destination path.";
            return false;
        }

        var expectedMemToReg = m_CurrentInstruction.GetExpectedMemToRegControlValue();
        if (m_MemToRegValue != expectedMemToReg)
        {
            validationMessage = "MemToReg is selecting the wrong write-back source.";
            return false;
        }

        if (m_RegisterScanner == null || m_RegisterScanner.AcceptedRegister == null)
        {
            validationMessage = "The destination register has not been placed yet.";
            return false;
        }

        if (m_PacketScanner == null || m_PacketScanner.AcceptedPacket == null)
        {
            validationMessage = "The write-back data packet has not been placed yet.";
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
        ResetScannersIfSignalStateIsInvalid();
        SetFeedback(string.Empty, false);
        RefreshPresentation();
    }

    void HandleRegWritePressed(SelectEnterEventArgs _)
    {
        if (!m_IsPhaseActive || m_HasAppliedWriteBack)
            return;

        m_RegWriteValue = m_RegWriteValue == "1" ? "0" : "1";
        ResetScannersIfSignalStateIsInvalid();
        SetFeedback(string.Empty, false);
        RefreshPresentation();
    }

    void HandleMemToRegPressed(SelectEnterEventArgs _)
    {
        if (!m_IsPhaseActive || m_HasAppliedWriteBack)
            return;

        m_MemToRegValue = m_MemToRegValue == "1" ? "0" : "1";
        RefreshExpectedTargets();
        ResetScannersIfSignalStateIsInvalid();
        SetFeedback(string.Empty, false);
        RefreshPresentation();
    }

    void HandleRegisterAccepted(WriteBackRegisterScanner _, RegisterToken __)
    {
        if (m_HasAppliedWriteBack)
            return;

        SetFeedback(string.Empty, false);
        RefreshPresentation();
    }

    void HandlePacketAccepted(WriteBackPacketScanner _, DataPacketToken __)
    {
        if (m_HasAppliedWriteBack)
            return;

        SetFeedback(string.Empty, false);
        RefreshPresentation();
    }

    void RefreshExpectedTargets()
    {
        m_RegisterScanner?.SetExpectedRegisterId(GetExpectedRegisterIdFromControlState());
        m_PacketScanner?.SetExpectedPacketRole(GetExpectedPacketRoleFromControlState());
    }

    void ResetScannersIfSignalStateIsInvalid()
    {
        if (m_CurrentInstruction == null)
            return;

        var expectedRegDst = m_CurrentInstruction.GetExpectedRegDstControlValue();
        var expectedRegWrite = m_CurrentInstruction.GetExpectedRegWriteControlValue();
        var expectedMemToReg = m_CurrentInstruction.GetExpectedMemToRegControlValue();

        if (m_RegDstValue == expectedRegDst &&
            m_RegWriteValue == expectedRegWrite &&
            m_MemToRegValue == expectedMemToReg)
        {
            return;
        }

        m_RegisterScanner?.ResetScanner();
        m_PacketScanner?.ResetScanner();
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
}

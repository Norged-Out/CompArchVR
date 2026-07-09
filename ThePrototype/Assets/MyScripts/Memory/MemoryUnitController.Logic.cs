using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Runtime memory-phase validation, transfer routing, and packet spawning.
/// </summary>
public partial class MemoryUnitController
{
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
}

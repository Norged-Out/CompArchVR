using System;
using UnityEngine;

/// <summary>
/// Minimal lesson state machine for the current Testing Ground MVP.
/// It owns:
/// - which instruction is active
/// - which lesson step is active
/// - register-scanner validation for the decode-stage source operands
/// - simple continue-style progression for non-placement steps
/// </summary>
[DisallowMultipleComponent]
public partial class CpuLessonFlow : MonoBehaviour
{
    const string k_LogPrefix = "[CpuLessonFlow]";

    [SerializeField]
    InstructionDefinition m_CurrentInstruction;

    [SerializeField]
    string m_DefaultInstructionResourcePath = "InstructionDefinitions/AddInstructionDefinition";

    [SerializeField]
    RegisterBank m_RegisterBank;

    [SerializeField]
    ImmediateExtender m_ImmediateExtender;

    [SerializeField]
    InstructionTerminal m_FetchUploadTerminal;

    [SerializeField]
    InstructionTerminal m_DecodeDownloadTerminal;

    [SerializeField]
    PcUpdateController m_PcUpdateController;

    [SerializeField]
    InstructionRuntimeSelection m_RuntimeSelection = new();

    int m_CurrentStepIndex = -1;
    int m_CurrentRegisterSelectionIndex;
    bool m_RegisterSelectionReadyToContinue;
    int m_LastAdvanceFrame = -1;

    public event Action<CpuLessonFlow> StepChanged;
    public event Action<string, bool> FeedbackChanged;

    public InstructionDefinition CurrentInstruction => m_CurrentInstruction;
    public InstructionRuntimeSelection RuntimeSelection => m_RuntimeSelection;
    public int CurrentStepIndex => m_CurrentStepIndex;
    public int CurrentRegisterSelectionIndex => m_CurrentRegisterSelectionIndex;
    public bool HasStarted => m_CurrentStepIndex >= 0;
    public RegisterBank RegisterBank => m_RegisterBank;
    public bool RegisterSelectionReadyToContinue => m_RegisterSelectionReadyToContinue;
    public bool UsesInstructionTerminals => m_FetchUploadTerminal != null && m_DecodeDownloadTerminal != null;
    public bool IsInstructionReadyForDecode => !UsesInstructionTerminals || HasDownloadedInstructionModule();

    public InstructionFlowStep CurrentStep
    {
        get
        {
            if (m_CurrentInstruction == null ||
                m_CurrentInstruction.flowSteps == null ||
                m_CurrentStepIndex < 0 ||
                m_CurrentStepIndex >= m_CurrentInstruction.flowSteps.Length)
            {
                return null;
            }

            return m_CurrentInstruction.flowSteps[m_CurrentStepIndex];
        }
    }

    void Awake()
    {
        if (m_CurrentInstruction == null)
            m_CurrentInstruction = LoadDefaultInstruction();
    }

    /// <summary>
    /// Lets the authored intro UI decide which instruction asset should drive
    /// the next walkthrough.
    /// </summary>
    public void SetCurrentInstruction(InstructionDefinition instruction)
    {
        if (instruction == null || instruction == m_CurrentInstruction)
            return;

        m_CurrentInstruction = instruction;
        m_RuntimeSelection.definition = m_CurrentInstruction;

        // If the learner changes the selected lesson mid-run, restart cleanly
        // so scene objects and runtime state cannot drift between instructions.
        if (HasStarted)
            ResetLesson();

        StepChanged?.Invoke(this);
    }

    void OnEnable()
    {
        RebindRegisterBank();
    }

    void OnDisable()
    {
        if (m_RegisterBank == null)
            return;

        m_RegisterBank.RegisterPressed -= HandleRegisterPressed;
        m_RegisterBank.RegisterScanned -= HandleRegisterScanned;
    }

    public void StartLesson()
    {
        if (m_CurrentInstruction == null)
            m_CurrentInstruction = LoadDefaultInstruction();

        if (m_CurrentInstruction == null || m_CurrentInstruction.flowSteps == null || m_CurrentInstruction.flowSteps.Length == 0)
            return;

        Debug.Log($"{k_LogPrefix} StartLesson | instruction={m_CurrentInstruction.displayName} assembly={m_CurrentInstruction.assemblyInstructionText} frame={Time.frameCount}", this);

        RebindRegisterBank();
        m_RuntimeSelection.definition = m_CurrentInstruction;
        m_RuntimeSelection.ResetOperands();
        m_CurrentRegisterSelectionIndex = 0;
        m_RegisterSelectionReadyToContinue = false;
        m_LastAdvanceFrame = -1;
        m_CurrentStepIndex = 0;
        m_ImmediateExtender?.ResetScanner();

        // Fetch now has a physical counterpart. Starting a lesson resets both
        // terminals, spawns a fresh module at the uploader, and uploads the
        // currently selected instruction into that module for the learner to carry.
        PrepareInstructionFetchTerminals();

        if (m_RegisterBank != null)
        {
            // A fresh run resets authored register poses, then reapplies the
            // instruction's starting register values for the current lesson.
            m_RegisterBank.RefreshRegisterCache();
            m_RegisterBank.RefreshScannerCache();
            m_RegisterBank.ResetAllRegisters();
            ApplyInitialRegisterValues();
        }

        PresentCurrentStep();
    }

    public void Advance()
    {
        if (!HasStarted || CurrentStep == null)
            return;

        Debug.Log(
            $"{k_LogPrefix} Advance requested | stepIndex={m_CurrentStepIndex} step={CurrentStep.stepName} interaction={CurrentStep.requiredInteraction} readyToContinue={m_RegisterSelectionReadyToContinue} frame={Time.frameCount}",
            this);

        switch (CurrentStep.requiredInteraction)
        {
            case InstructionStepInteractionType.None:
            case InstructionStepInteractionType.ContinueButton:
                if (IsFetchStep(CurrentStep) && UsesInstructionTerminals && !HasDownloadedInstructionModule())
                {
                    SetFeedback(GetFetchTransportPrompt(), true);
                    StepChanged?.Invoke(this);
                    break;
                }

                AdvanceToNextStep();
                break;

            case InstructionStepInteractionType.RegisterSelection:
                if (!m_RegisterSelectionReadyToContinue)
                {
                    SetFeedback("Decode work is not complete yet.", true);
                    break;
                }

                if (m_CurrentInstruction != null &&
                    m_CurrentInstruction.usesImmediate &&
                    !TrySpawnImmediatePacket())
                {
                    SetFeedback("Immediate Extender is missing its packet prefab or spawn anchor.", true);
                    break;
                }

                AdvanceToNextStep();
                break;

            case InstructionStepInteractionType.AluExecution:
                SetFeedback("Set the ALU controls, place the inputs, and execute the operation.", false);
                break;

            case InstructionStepInteractionType.WriteBackExecution:
                if (!string.IsNullOrWhiteSpace(m_RuntimeSelection.confirmedWriteBackRegister) && m_RuntimeSelection.hasAluResult)
                    AdvanceToNextStep();
                else
                    SetFeedback("Set the write-back controls, place the register and result packet, then execute the transfer.", false);
                break;

            case InstructionStepInteractionType.PcUpdateExecution:
                SetFeedback("Set PC + 4 and confirm the next PC path.", false);
                break;

            case InstructionStepInteractionType.Completion:
                ResetLesson();
                break;
        }
    }

    public void ResetLesson()
    {
        Debug.Log($"{k_LogPrefix} ResetLesson | frame={Time.frameCount}", this);

        if (m_RegisterBank != null)
        {
            m_RegisterBank.ResetAllRegisters();
            ApplyInitialRegisterValues();
        }

        m_RuntimeSelection.definition = m_CurrentInstruction;
        m_RuntimeSelection.ResetOperands();
        m_CurrentRegisterSelectionIndex = 0;
        m_RegisterSelectionReadyToContinue = false;
        m_LastAdvanceFrame = -1;
        m_CurrentStepIndex = -1;
        m_ImmediateExtender?.ResetScanner();

        // A full lesson reset also restores fetch so the next run always starts
        // from the same physical handoff.
        PrepareInstructionFetchTerminals();
        StepChanged?.Invoke(this);
        SetFeedback(string.Empty, false);
    }

    public void CompleteAluExecution(int resultValue)
    {
        if (!HasStarted || CurrentStep == null)
            return;

        if (CurrentStep.requiredInteraction != InstructionStepInteractionType.AluExecution)
            return;

        Debug.Log($"{k_LogPrefix} CompleteAluExecution | result={resultValue} stepIndex={m_CurrentStepIndex} step={CurrentStep.stepName} frame={Time.frameCount}", this);

        m_RuntimeSelection.aluResultValue = resultValue;
        m_RuntimeSelection.hasAluResult = true;
        SetFeedback(GetPostAluContinuePrompt(resultValue), false);
        AdvanceToNextStep();
    }

    /// <summary>
    /// Applies the final write-back value after the authored WB prefab has
    /// validated control signals and completed its transfer sequence.
    /// </summary>
    public void CompleteWriteBackExecution(string destinationRegister, int resultValue)
    {
        if (string.IsNullOrWhiteSpace(destinationRegister))
            return;

        Debug.Log($"{k_LogPrefix} CompleteWriteBackExecution | register={destinationRegister} value={resultValue} frame={Time.frameCount}", this);

        m_RuntimeSelection.confirmedWriteBackRegister = destinationRegister;
        m_RuntimeSelection.aluResultValue = resultValue;
        m_RuntimeSelection.hasAluResult = true;
        m_RegisterBank?.SetRegisterValue(destinationRegister, resultValue);
        SetFeedback($"Write-back complete. {destinationRegister} now stores {resultValue}. Press Continue to finish.", false);
        StepChanged?.Invoke(this);
    }
}

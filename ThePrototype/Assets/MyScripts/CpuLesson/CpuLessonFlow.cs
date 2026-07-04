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
public class CpuLessonFlow : MonoBehaviour
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
    public bool HasStarted => m_CurrentStepIndex >= 0;
    public RegisterBank RegisterBank => m_RegisterBank;
    public bool RegisterSelectionReadyToContinue => m_RegisterSelectionReadyToContinue;

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

        CacheSceneReferences();
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
        CacheSceneReferences();
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
                AdvanceToNextStep();
                break;

            case InstructionStepInteractionType.RegisterSelection:
                if (!m_RegisterSelectionReadyToContinue)
                {
                    SetFeedback(GetRegisterSelectionPrompt(), true);
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
        if (!HasStarted || string.IsNullOrWhiteSpace(registerName))
            return;

        if (CurrentStep == null)
            return;

        switch (CurrentStep.requiredInteraction)
        {
            case InstructionStepInteractionType.RegisterSelection:
                ValidateRegisterSelection(InstructionRegisterRole.None, registerName, false);
                break;
        }
    }

    void HandleRegisterScanned(InstructionRegisterRole scannedRole, string registerName)
    {
        if (!HasStarted || string.IsNullOrWhiteSpace(registerName))
            return;

        if (CurrentStep == null)
            return;

        switch (CurrentStep.requiredInteraction)
        {
            case InstructionStepInteractionType.RegisterSelection:
                Debug.Log($"{k_LogPrefix} HandleRegisterScanned | role={scannedRole} register={registerName} selectionIndex={m_CurrentRegisterSelectionIndex} step={CurrentStep.stepName} frame={Time.frameCount}", this);
                ValidateRegisterSelection(scannedRole, registerName, true);
                break;
        }
    }

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
            SetFeedback(
                $"Use {GetScannerLabel(expectedRole)} for {m_CurrentInstruction.GetExpectedRegisterName(expectedRole)}.",
                true);
            return;
        }

        if (!result.isCorrect)
        {
            if (cameFromScanner)
                m_RegisterBank?.FlashScannerFailure(scannedRole);
            else
                m_RegisterBank?.FlashFailure(registerName);

            SetFeedback(
                $"Incorrect. Expected {result.expectedRegister}, not {registerName}.",
                true);
            return;
        }

        m_RuntimeSelection.SetSelectedRegister(result.expectedRole, registerName);
        m_CurrentRegisterSelectionIndex++;

        // Only rs / rt produce packets in the current datapath slice. The write
        // register is still validated here, but it should never spawn output.
        var scannedValue = m_RegisterBank != null ? m_RegisterBank.GetRegisterValue(registerName) : 0;
        var successMessage = ShouldSpawnPacket(result.expectedRole)
            ? $"{GetPacketLabel(result.expectedRole)} packet ready: {scannedValue}."
            : $"{registerName} confirmed on {GetScannerLabel(result.expectedRole)}.";

        if (cameFromScanner)
            m_RegisterBank?.SetScannerSuccess(scannedRole);
        else
            m_RegisterBank?.SetSelected(registerName);

        if (result.completesStep)
        {
            m_RegisterSelectionReadyToContinue = true;
            var completionMessage = successMessage;

            if (m_CurrentInstruction != null && m_CurrentInstruction.usesImmediate)
                completionMessage += $" Press Continue to spawn the Immediate packet at the Immediate Extender.";

            SetFeedback($"{completionMessage} Decode is complete. Press Continue.", false);
            Debug.Log($"{k_LogPrefix} Register selection complete | step={CurrentStep.stepName} nextStepPending=true frame={Time.frameCount}", this);
            StepChanged?.Invoke(this);
            return;
        }

        m_RegisterSelectionReadyToContinue = false;
        SetFeedback($"{successMessage} Next: place {result.nextRegister} on {GetScannerLabel(result.nextRole)}.", false);
        StepChanged?.Invoke(this);
    }

    void AdvanceToNextStep()
    {
        if (m_CurrentInstruction == null || m_CurrentInstruction.flowSteps == null)
            return;

        // UI panels are toggled as part of progression. Without a small
        // debounce, the same click can occasionally be seen twice while panels
        // swap, which makes the lesson jump over authored intermediate steps.
        if (m_LastAdvanceFrame == Time.frameCount)
        {
            Debug.Log($"{k_LogPrefix} AdvanceToNextStep blocked by debounce | currentStepIndex={m_CurrentStepIndex} frame={Time.frameCount}", this);
            return;
        }

        m_LastAdvanceFrame = Time.frameCount;
        var previousStepName = CurrentStep != null ? CurrentStep.stepName : "<none>";
        var previousStepIndex = m_CurrentStepIndex;

        m_CurrentStepIndex++;
        m_CurrentRegisterSelectionIndex = 0;
        m_RegisterSelectionReadyToContinue = false;

        while (m_CurrentStepIndex < m_CurrentInstruction.flowSteps.Length &&
               ShouldSkipStep(m_CurrentInstruction.flowSteps[m_CurrentStepIndex]))
        {
            Debug.Log(
                $"{k_LogPrefix} Skipping step | stepIndex={m_CurrentStepIndex} step={m_CurrentInstruction.flowSteps[m_CurrentStepIndex].stepName} frame={Time.frameCount}",
                this);
            m_CurrentStepIndex++;
        }

        Debug.Log($"{k_LogPrefix} AdvanceToNextStep | fromIndex={previousStepIndex} fromStep={previousStepName} toIndex={m_CurrentStepIndex} frame={Time.frameCount}", this);

        if (m_CurrentStepIndex >= m_CurrentInstruction.flowSteps.Length)
        {
            m_CurrentStepIndex = m_CurrentInstruction.flowSteps.Length - 1;
            StepChanged?.Invoke(this);
            return;
        }

        PresentCurrentStep();
    }

    void PresentCurrentStep()
    {
        ConfigureScannersForCurrentStep();
        Debug.Log(
            $"{k_LogPrefix} PresentCurrentStep | stepIndex={m_CurrentStepIndex} step={CurrentStep?.stepName} interaction={CurrentStep?.requiredInteraction} highlightedNode={CurrentStep?.highlightedNode} frame={Time.frameCount}",
            this);
        StepChanged?.Invoke(this);

        switch (CurrentStep.requiredInteraction)
        {
            case InstructionStepInteractionType.ContinueButton:
                SetFeedback("Press Continue when you are ready.", false);
                break;

            case InstructionStepInteractionType.RegisterSelection:
                SetFeedback(GetRegisterSelectionPrompt(), false);
                break;

            case InstructionStepInteractionType.AluExecution:
                SetFeedback(
                    m_CurrentInstruction != null && m_CurrentInstruction.UsesInteractiveMemoryPhase()
                        ? "Set the ALU controls, place the inputs, execute the operation, then continue to Memory Access."
                        : "Set the ALU controls, place the inputs, execute the operation, then continue directly to Write Back.",
                    false);
                break;

            case InstructionStepInteractionType.WriteBackExecution:
                SetFeedback(
                    $"Write-back target: {m_CurrentInstruction.GetWriteBackTargetRegister()}. Source: {GetPacketLabel(m_CurrentInstruction.GetWriteBackPacketRole())}. Set the controls, place both inputs, and execute the transfer.",
                    false);
                break;

            case InstructionStepInteractionType.Completion:
                SetFeedback("Lesson complete. Press Restart to play it again.", false);
                break;

            default:
                SetFeedback(string.Empty, false);
                break;
        }
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

    string GetRegisterSelectionPrompt()
    {
        if (CurrentStep == null)
            return string.Empty;

        var requiredRoles = LessonChecks.GetRequiredRoles(m_CurrentInstruction, CurrentStep);
        if (m_CurrentRegisterSelectionIndex < 0 || m_CurrentRegisterSelectionIndex >= requiredRoles.Length)
            return "Place the required registers on the active scanners.";

        var currentRole = requiredRoles[m_CurrentRegisterSelectionIndex];
        var expectedRegister = m_CurrentInstruction.GetExpectedRegisterName(currentRole);
        return $"Place {expectedRegister} on {GetScannerLabel(currentRole)}.";
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
            _ => "Packet",
        };
    }

    static bool ShouldSpawnPacket(InstructionRegisterRole registerRole)
    {
        return registerRole == InstructionRegisterRole.Rs ||
               registerRole == InstructionRegisterRole.Rt;
    }

    void ConfigureRegisterDecodeScanners()
    {
        if (m_RegisterBank == null)
            return;

        // ID only exposes the register-file read ports. Destination selection
        // is deferred to write-back, where the learner will choose the target
        // register together with the final data source.
        var activeRoles = LessonChecks.GetRequiredRoles(m_CurrentInstruction, CurrentStep);
        m_RegisterBank.ConfigureScannerRoles(activeRoles);
        m_RegisterBank.SetScannerOutputRole(InstructionRegisterRole.Rs, DataPacketRole.ReadData1);
        m_RegisterBank.SetScannerOutputRole(InstructionRegisterRole.Rt, m_CurrentInstruction.GetDecodeRtPacketRole());
    }

    bool TrySpawnImmediatePacket()
    {
        if (m_CurrentInstruction == null || !m_CurrentInstruction.usesImmediate)
            return false;

        CacheSceneReferences();

        if (m_ImmediateExtender == null)
            return false;

        m_RuntimeSelection.immediateValue = m_CurrentInstruction.expectedImmediateValue;
        return m_ImmediateExtender.SpawnImmediatePacket(m_CurrentInstruction.expectedImmediateValue);
    }

    void SetFeedback(string message, bool isFailure)
    {
        FeedbackChanged?.Invoke(message, isFailure);
    }

    void ApplyInitialRegisterValues()
    {
        if (m_RegisterBank == null)
            return;

        m_RegisterBank.ResetAllRegisterValues();

        var initialRegisterValues = m_CurrentInstruction?.initialRegisterValues;
        if (initialRegisterValues == null)
            return;

        foreach (var registerValue in initialRegisterValues)
        {
            if (registerValue == null || string.IsNullOrWhiteSpace(registerValue.registerId))
                continue;

            m_RegisterBank.SetRegisterValue(registerValue.registerId, registerValue.value);
        }
    }

    InstructionDefinition LoadDefaultInstruction()
    {
        var loadedInstruction = Resources.Load<InstructionDefinition>(m_DefaultInstructionResourcePath);
        return loadedInstruction != null ? loadedInstruction : InstructionDefaults.CreateFallbackAdd();
    }

    void CacheSceneReferences()
    {
        if (m_RegisterBank == null)
            m_RegisterBank = FindFirstSceneObject<RegisterBank>();

        if (m_ImmediateExtender == null)
            m_ImmediateExtender = FindFirstSceneObject<ImmediateExtender>();
    }

    bool ShouldSkipStep(InstructionFlowStep step)
    {
        if (step == null || m_CurrentInstruction == null)
            return false;

        if (step.highlightedNode == DatapathNodeId.DataMemory && !m_CurrentInstruction.UsesInteractiveMemoryPhase())
            return true;

        if ((step.highlightedNode == DatapathNodeId.WriteBack ||
             step.requiredInteraction == InstructionStepInteractionType.WriteBackExecution) &&
            !m_CurrentInstruction.UsesWriteBackPhase())
        {
            return true;
        }

        return false;
    }

    string GetPostAluContinuePrompt(int resultValue)
    {
        if (m_CurrentInstruction == null)
            return $"ALU result produced: {resultValue}. Continue.";

        if (m_CurrentInstruction.UsesInteractiveMemoryPhase())
            return $"ALU result produced: {resultValue}. Continue to Memory Access.";

        if (m_CurrentInstruction.UsesWriteBackPhase())
            return $"ALU result produced: {resultValue}. Data Memory is skipped for this instruction. Continue to Write Back.";

        return $"ALU result produced: {resultValue}. Continue to the recap.";
    }

    static T FindFirstSceneObject<T>() where T : Component
    {
        foreach (var component in Resources.FindObjectsOfTypeAll<T>())
        {
            if (component == null)
                continue;

            if (!component.gameObject.scene.IsValid() || !component.gameObject.scene.isLoaded)
                continue;

            return component;
        }

        return null;
    }
}

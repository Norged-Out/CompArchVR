using System;
using UnityEngine;

/// <summary>
/// Minimal lesson state machine for the current Testing Ground MVP.
/// It owns:
/// - which instruction is active
/// - which lesson step is active
/// - register-scanner validation for rs / rt / rd
/// - simple continue-style progression for non-placement steps
/// </summary>
[DisallowMultipleComponent]
public class CpuLessonFlow : MonoBehaviour
{
    [SerializeField]
    InstructionDefinition m_CurrentInstruction;

    [SerializeField]
    string m_DefaultInstructionResourcePath = "InstructionDefinitions/AddInstructionDefinition";

    [SerializeField]
    RegisterBank m_RegisterBank;

    [SerializeField]
    InstructionRuntimeSelection m_RuntimeSelection = new();

    int m_CurrentStepIndex = -1;
    int m_CurrentRegisterSelectionIndex;

    public event Action<CpuLessonFlow> StepChanged;
    public event Action<string, bool> FeedbackChanged;

    public InstructionDefinition CurrentInstruction => m_CurrentInstruction;
    public InstructionRuntimeSelection RuntimeSelection => m_RuntimeSelection;
    public int CurrentStepIndex => m_CurrentStepIndex;
    public bool HasStarted => m_CurrentStepIndex >= 0;

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

        RebindRegisterBank();
        m_RuntimeSelection.definition = m_CurrentInstruction;
        m_RuntimeSelection.ResetOperands();
        m_CurrentRegisterSelectionIndex = 0;
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

        switch (CurrentStep.requiredInteraction)
        {
            case InstructionStepInteractionType.None:
            case InstructionStepInteractionType.ContinueButton:
                // The current MVP uses a continue step for write-back. We apply
                // the ALU result right before moving on so the updated register
                // value is already visible during recap.
                if (IsWriteBackStep(CurrentStep))
                    ApplyWriteBackResult();

                AdvanceToNextStep();
                break;

            case InstructionStepInteractionType.RegisterSelection:
                SetFeedback(GetRegisterSelectionPrompt(), true);
                break;

            case InstructionStepInteractionType.AluExecution:
                SetFeedback("Configure the ALU, place the packets on both inputs, then execute the operation.", false);
                break;

            case InstructionStepInteractionType.WriteBackRegisterConfirmation:
                SetFeedback(
                    $"Place {m_CurrentInstruction.GetExpectedRegisterName(CurrentStep.confirmationRegisterRole)} on the {GetScannerLabel(CurrentStep.confirmationRegisterRole)} scanner.",
                    true);
                break;

            case InstructionStepInteractionType.Completion:
                ResetLesson();
                break;
        }
    }

    public void ResetLesson()
    {
        if (m_RegisterBank != null)
        {
            m_RegisterBank.ResetAllRegisters();
            ApplyInitialRegisterValues();
        }

        m_RuntimeSelection.definition = m_CurrentInstruction;
        m_RuntimeSelection.ResetOperands();
        m_CurrentRegisterSelectionIndex = 0;
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

        m_RuntimeSelection.aluResultValue = resultValue;
        m_RuntimeSelection.hasAluResult = true;
        SetFeedback($"ALU result produced: {resultValue}. Continue to write back.", false);
        AdvanceToNextStep();
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

            case InstructionStepInteractionType.WriteBackRegisterConfirmation:
                ValidateWriteBack(InstructionRegisterRole.None, registerName, false);
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
                ValidateRegisterSelection(scannedRole, registerName, true);
                break;

            case InstructionStepInteractionType.WriteBackRegisterConfirmation:
                ValidateWriteBack(scannedRole, registerName, true);
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
                $"Wrong pedestal. Put {m_CurrentInstruction.GetExpectedRegisterName(expectedRole)} on {GetScannerLabel(expectedRole)}.",
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
                $"Incorrect. {result.expectedRole} should be {result.expectedRegister}, not {registerName}.",
                true);
            return;
        }

        m_RuntimeSelection.SetSelectedRegister(result.expectedRole, registerName);
        m_CurrentRegisterSelectionIndex++;

        // Only rs / rt produce packets in the current datapath slice. The write
        // register is still validated here, but it should never spawn output.
        var scannedValue = m_RegisterBank != null ? m_RegisterBank.GetRegisterValue(registerName) : 0;
        var successMessage = ShouldSpawnPacket(result.expectedRole)
            ? $"Spawning {GetPacketLabel(result.expectedRole)} packet with value {scannedValue}."
            : $"{registerName} confirmed for {GetScannerLabel(result.expectedRole)}.";

        if (cameFromScanner)
            m_RegisterBank?.SetScannerSuccess(scannedRole);
        else
            m_RegisterBank?.SetSelected(registerName);

        if (result.completesStep)
        {
            SetFeedback($"{successMessage} Registers placed correctly. Continue to the next stage.", false);
            AdvanceToNextStep();
            return;
        }

        SetFeedback($"{successMessage} Now place {result.nextRegister} on {GetScannerLabel(result.nextRole)}.", false);
        StepChanged?.Invoke(this);
    }

    void ValidateWriteBack(InstructionRegisterRole scannedRole, string registerName, bool cameFromScanner)
    {
        if (cameFromScanner && scannedRole != CurrentStep.confirmationRegisterRole)
        {
            m_RegisterBank?.FlashScannerFailure(scannedRole);
            SetFeedback($"Wrong pedestal. Use the {GetScannerLabel(CurrentStep.confirmationRegisterRole)} scanner.", true);
            return;
        }

        var result = LessonChecks.ValidateWriteBack(m_CurrentInstruction, CurrentStep, registerName);
        if (!result.isCorrect)
        {
            if (cameFromScanner)
                m_RegisterBank?.FlashScannerFailure(scannedRole);
            else
                m_RegisterBank?.FlashFailure(registerName);

            SetFeedback($"Incorrect. The write-back destination should be {result.expectedRegister}.", true);
            return;
        }

        m_RuntimeSelection.confirmedWriteBackRegister = registerName;
        if (cameFromScanner)
            m_RegisterBank?.SetScannerSuccess(CurrentStep.confirmationRegisterRole);
        else
            m_RegisterBank?.SetSelected(registerName);

        SetFeedback("Write-back confirmed.", false);
        AdvanceToNextStep();
    }

    void AdvanceToNextStep()
    {
        if (m_CurrentInstruction == null || m_CurrentInstruction.flowSteps == null)
            return;

        m_CurrentStepIndex++;
        m_CurrentRegisterSelectionIndex = 0;

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
                SetFeedback("Configure the ALU, place the packets on both inputs, then execute the operation.", false);
                break;

            case InstructionStepInteractionType.WriteBackRegisterConfirmation:
                SetFeedback(
                    $"Place {m_CurrentInstruction.GetExpectedRegisterName(CurrentStep.confirmationRegisterRole)} on the {GetScannerLabel(CurrentStep.confirmationRegisterRole)} scanner.",
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
                // The authored register zone keeps three scanners in the scene
                // at all times; we only toggle which roles are "live" per step.
                m_RegisterBank.ConfigureScannerRoles(LessonChecks.GetRequiredRoles(CurrentStep));
                break;

            case InstructionStepInteractionType.WriteBackRegisterConfirmation:
                m_RegisterBank.ConfigureScannerRoles(new[] { CurrentStep.confirmationRegisterRole });
                break;

            default:
                m_RegisterBank.ConfigureScannerRoles(Array.Empty<InstructionRegisterRole>());
                break;
        }
    }

    void ApplyWriteBackResult()
    {
        if (m_RegisterBank == null || m_CurrentInstruction == null || !m_RuntimeSelection.hasAluResult)
            return;

        var destinationRegister = m_CurrentInstruction.expectedRd;
        if (string.IsNullOrWhiteSpace(destinationRegister))
            return;

        m_RegisterBank.SetRegisterValue(destinationRegister, m_RuntimeSelection.aluResultValue);
        m_RuntimeSelection.confirmedWriteBackRegister = destinationRegister;
    }

    string GetRegisterSelectionPrompt()
    {
        if (CurrentStep == null)
            return string.Empty;

        var requiredRoles = LessonChecks.GetRequiredRoles(CurrentStep);
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

    static bool ShouldSpawnPacket(InstructionRegisterRole registerRole)
    {
        return registerRole == InstructionRegisterRole.Rs ||
               registerRole == InstructionRegisterRole.Rt;
    }

    static bool IsWriteBackStep(InstructionFlowStep step)
    {
        return step != null && step.highlightedNode == DatapathNodeId.WriteBack;
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
}

using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// Owns the execution-phase interaction for the authored ALU prefab.
/// This includes:
/// - the two physical ALU control buttons on the prefab
/// - the ALU UI panel shown during the execute step
/// - packet validation for both ALU inputs
/// - result computation and result-packet spawning
/// </summary>
[DisallowMultipleComponent]
public class AluExecutionController : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField]
    AluInputScanner m_InputA;

    [SerializeField]
    AluInputScanner m_InputB;

    [SerializeField]
    TMP_Text m_OperationLabelText;

    [SerializeField]
    Transform m_ResultSpawnTransform;

    [SerializeField]
    ParticleSystem m_ComputeParticles;

    [SerializeField]
    DataPacketToken m_ResultPacketPrefab;

    [Header("Physical Buttons")]
    [SerializeField]
    Transform m_AluOpButtonRoot;

    [SerializeField]
    Transform m_AluSrcButtonRoot;

    [Header("ALU UI")]
    [SerializeField]
    GameObject m_AluUiRoot;

    [SerializeField]
    TMP_Text m_LessonRuntimeText;

    [SerializeField]
    TMP_Text m_AluOpStatusText;

    [SerializeField]
    TMP_Text m_AluSrcStatusText;

    [SerializeField]
    TMP_Text m_Input1StatusText;

    [SerializeField]
    TMP_Text m_Input2StatusText;

    [SerializeField]
    TMP_Text m_FeedbackText;

    [SerializeField]
    Button m_ExecuteButton;

    [SerializeField]
    TMP_Text m_ExecuteButtonLabel;

    [SerializeField]
    TMP_Dropdown m_FunctDropdown;

    [SerializeField]
    TMP_Dropdown m_HintDropdown;

    [SerializeField]
    TMP_Text m_HintAluOpText;

    [SerializeField]
    TMP_Text m_HintAluSrcText;

    [SerializeField]
    TMP_Text m_HintAluControlText;

    [Header("Timing")]
    [SerializeField]
    float m_ResultSpawnDelaySeconds = 1.25f;

    [SerializeField]
    string m_ExecuteButtonText = "Execute";

    [SerializeField]
    string m_ResultReadyButtonText = "Continue";

    [SerializeField]
    Color m_SuccessFeedbackColor = new(0.78f, 0.96f, 0.82f, 1f);

    [SerializeField]
    Color m_FailureFeedbackColor = new(1f, 0.55f, 0.55f, 1f);

    InstructionDefinition m_CurrentInstruction;
    Coroutine m_ComputeRoutine;
    DataPacketToken m_SpawnedResultPacket;
    bool m_IsPhaseActive;
    bool m_HasProducedResult;
    bool m_IsAwaitingContinue;
    int m_LastResultValue;
    string m_CurrentAluOpValue = "00";
    string m_CurrentAluSrcValue = "0";
    AluOperation m_SelectedFunctOperation = AluOperation.Add;
    bool m_HasExplicitFunctSelection;

    public event System.Action<int> ExecutionCompleted;

    public bool IsPhaseActive => m_IsPhaseActive;

    void Awake()
    {
        CacheReferences();
        PopulateHintDropdown();
        HookButtons();
        HookDropdown();
        HookHintDropdown();
        RefreshAllPresentation();
        SetFeedback(string.Empty, false);
    }

    void OnEnable()
    {
        CacheReferences();
        PopulateHintDropdown();
        HookButtons();
        HookDropdown();
        HookHintDropdown();
        HookInputEvents(true);
        RefreshAllPresentation();
    }

    void OnDisable()
    {
        HookInputEvents(false);
        UnhookButtons();
        UnhookDropdown();
        UnhookHintDropdown();
    }

    void Update()
    {
        if (!m_IsPhaseActive || m_HasProducedResult || m_ComputeRoutine != null)
            return;

        RefreshUiTexts();
    }

    public void SetPhaseState(bool isActive, InstructionDefinition instruction)
    {
        CacheReferences();

        var instructionChanged = instruction != null && instruction != m_CurrentInstruction;
        var isEnteringPhase = isActive && !m_IsPhaseActive;
        m_IsPhaseActive = isActive;
        m_CurrentInstruction = instruction != null ? instruction : InstructionDefaults.CreateFallbackAdd();

        // Entering the phase or swapping instructions should always rebuild the
        // ALU's expected inputs from lesson data.
        if (isEnteringPhase || instructionChanged)
            PrepareForExecutionStep();

        if (m_AluUiRoot != null)
            m_AluUiRoot.SetActive(isActive);

        if (m_ExecuteButton != null)
            m_ExecuteButton.interactable = isActive && !m_HasProducedResult;

        m_InputA?.SetActive(isActive);
        m_InputB?.SetActive(isActive);
        RefreshAllPresentation();
    }

    public void ResetExecutionState()
    {
        if (m_ComputeRoutine != null)
        {
            StopCoroutine(m_ComputeRoutine);
            m_ComputeRoutine = null;
        }

        if (m_ComputeParticles != null)
            m_ComputeParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        m_CurrentInstruction = null;
        m_CurrentAluOpValue = "00";
        m_CurrentAluSrcValue = "0";
        m_HasProducedResult = false;
        m_IsAwaitingContinue = false;
        m_LastResultValue = 0;
        m_IsPhaseActive = false;

        m_InputA?.ResetScanner();
        m_InputB?.ResetScanner();
        ClearSpawnedResultPacket();
        SetFeedback(string.Empty, false);
        RefreshAllPresentation();

        if (m_AluUiRoot != null)
            m_AluUiRoot.SetActive(false);
    }

    public void HandleExecutePressed()
    {
        if (!m_IsPhaseActive || m_ComputeRoutine != null)
            return;

        if (m_HasProducedResult && m_IsAwaitingContinue)
        {
            m_IsAwaitingContinue = false;
            ExecutionCompleted?.Invoke(m_LastResultValue);
            return;
        }

        if (!TryValidateExecutionSetup(out var validationMessage))
        {
            SetFeedback(validationMessage, true);
            RefreshAllPresentation();
            return;
        }

        SetFeedback($"Executing {GetOperationDisplayName()}...", false);
        RefreshAllPresentation();
        m_ComputeRoutine = StartCoroutine(ComputeRoutine());
    }

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

    void RefreshAllPresentation()
    {
        RefreshExpectedInputRoles();
        RefreshPrefabLabels();
        RefreshUiTexts();
    }

    void RefreshPrefabLabels()
    {
        if (m_OperationLabelText != null)
            m_OperationLabelText.text = GetOperationDisplayName();
    }

    void RefreshUiTexts()
    {
        if (m_LessonRuntimeText != null)
            m_LessonRuntimeText.text = BuildLessonRuntimeText();

        if (m_AluOpStatusText != null)
            m_AluOpStatusText.text = $"ALUOp: {m_CurrentAluOpValue}";

        if (m_AluSrcStatusText != null)
            m_AluSrcStatusText.text = $"ALUSrc: {m_CurrentAluSrcValue}";

        if (m_FunctDropdown != null)
        {
            var showFunctDropdown = m_CurrentAluOpValue == "10";
            m_FunctDropdown.gameObject.SetActive(showFunctDropdown);
            m_FunctDropdown.interactable = showFunctDropdown && !m_HasProducedResult;
            if (showFunctDropdown)
            {
                if (m_HasExplicitFunctSelection)
                    SyncDropdownToCurrentOperation();
            }
        }

        RefreshHintBlocks();

        if (m_Input1StatusText != null)
            m_Input1StatusText.text = BuildInputStatusText("Input 1", DataPacketRole.ReadData1, m_InputA);

        if (m_Input2StatusText != null)
            m_Input2StatusText.text = BuildInputStatusText("Input 2", GetExpectedInput2Role(), m_InputB);

        if (m_ExecuteButtonLabel != null)
            m_ExecuteButtonLabel.text = m_HasProducedResult ? m_ResultReadyButtonText : m_ExecuteButtonText;

        if (m_ExecuteButton != null)
            m_ExecuteButton.interactable = m_IsPhaseActive && m_ComputeRoutine == null;
    }

    string BuildInputStatusText(string inputLabel, DataPacketRole expectedRole, AluInputScanner scanner)
    {
        if (scanner == null)
            return $"{inputLabel}: waiting for {GetRoleDisplayName(expectedRole)}";

        if (scanner.AcceptedPacket == null)
        {
            if (scanner.CurrentIssue == AluInputScanner.PacketIssue.ImmediateNotSignExtended)
                return $"{inputLabel}: Immediate detected (not sign-extended)";

            return $"{inputLabel}: waiting for {GetRoleDisplayName(expectedRole)}";
        }

        var signExtensionSuffix = scanner.AcceptedPacket.PacketRole == DataPacketRole.Immediate
            ? scanner.AcceptedPacket.IsSignExtended ? " (sign-extended)" : " (not sign-extended)"
            : string.Empty;
        return $"{inputLabel}: {GetRoleDisplayName(scanner.AcceptedPacket.PacketRole)} = {scanner.AcceptedValue}{signExtensionSuffix}";
    }

    string GetOperationDisplayName()
    {
        if (m_CurrentAluOpValue == "10" && !m_HasExplicitFunctSelection)
            return "None";

        return GetOperationDisplayName(ResolveCurrentOperation());
    }

    AluOperation ResolveCurrentOperation()
    {
        if (m_CurrentAluOpValue == "10")
            return m_SelectedFunctOperation;

        return ResolveOperation(m_CurrentInstruction, m_CurrentAluOpValue);
    }

    string GetOperationDisplayName(AluOperation operation)
    {
        return operation switch
        {
            AluOperation.Subtract => "Sub",
            AluOperation.And => "And",
            AluOperation.Or => "Or",
            AluOperation.SetOnLessThan => "Slt",
            _ => "Add",
        };
    }

    string GetPostExecuteFeedback(int resultValue)
    {
        if (m_CurrentInstruction == null)
            return $"ALU result ready: {resultValue}. Click Continue.";

        if (m_CurrentInstruction.UsesBranchDecision())
            return $"Zero result ready: {resultValue}. Click Continue to proceed to Program Counter Update.";

        if (m_CurrentInstruction.UsesInteractiveMemoryPhase())
            return $"ALU result ready: {resultValue}. Click Continue to proceed to Memory Access.";

        if (m_CurrentInstruction.UsesWriteBackPhase())
            return $"ALU result ready: {resultValue}. Memory Access is skipped for this instruction. Click Continue to proceed to Write Back.";

        return $"ALU result ready: {resultValue}. Click Continue to proceed to Program Counter Update.";
    }

    string GetNextPhaseLabel()
    {
        if (m_CurrentInstruction == null)
            return "Continue";

        if (m_CurrentInstruction.UsesInteractiveMemoryPhase())
            return "Memory Access";

        return m_CurrentInstruction.UsesWriteBackPhase() ? "Write Back" : "Recap";
    }

    void SetFeedback(string message, bool isFailure)
    {
        if (m_FeedbackText == null)
            return;

        m_FeedbackText.text = message;
        m_FeedbackText.color = isFailure ? m_FailureFeedbackColor : m_SuccessFeedbackColor;
        m_FeedbackText.gameObject.SetActive(!string.IsNullOrWhiteSpace(message));
    }


    void HookButtons()
    {
        HookPhysicalButton(m_AluOpButtonRoot, HandleAluOpPressed, true);
        HookPhysicalButton(m_AluSrcButtonRoot, HandleAluSrcPressed, true);

        if (m_ExecuteButton != null)
        {
            m_ExecuteButton.onClick.RemoveListener(HandleExecutePressed);
            m_ExecuteButton.onClick.AddListener(HandleExecutePressed);
        }
    }

    void UnhookButtons()
    {
        HookPhysicalButton(m_AluOpButtonRoot, HandleAluOpPressed, false);
        HookPhysicalButton(m_AluSrcButtonRoot, HandleAluSrcPressed, false);

        if (m_ExecuteButton != null)
            m_ExecuteButton.onClick.RemoveListener(HandleExecutePressed);
    }

    void HookDropdown()
    {
        if (m_FunctDropdown == null)
            return;

        m_FunctDropdown.onValueChanged.RemoveListener(HandleFunctDropdownChanged);
        m_FunctDropdown.onValueChanged.AddListener(HandleFunctDropdownChanged);
    }

    void UnhookDropdown()
    {
        if (m_FunctDropdown == null)
            return;

        m_FunctDropdown.onValueChanged.RemoveListener(HandleFunctDropdownChanged);
    }

    void HookHintDropdown()
    {
        if (m_HintDropdown == null)
            return;

        m_HintDropdown.onValueChanged.RemoveListener(HandleHintDropdownChanged);
        m_HintDropdown.onValueChanged.AddListener(HandleHintDropdownChanged);
    }

    void UnhookHintDropdown()
    {
        if (m_HintDropdown == null)
            return;

        m_HintDropdown.onValueChanged.RemoveListener(HandleHintDropdownChanged);
    }

    void HookInputEvents(bool subscribe)
    {
        HookInputEvent(m_InputA, subscribe);
        HookInputEvent(m_InputB, subscribe);
    }

    void HookInputEvent(AluInputScanner inputScanner, bool subscribe)
    {
        if (inputScanner == null)
            return;

        if (subscribe)
        {
            inputScanner.PacketAccepted -= HandlePacketAccepted;
            inputScanner.PacketAccepted += HandlePacketAccepted;
        }
        else
        {
            inputScanner.PacketAccepted -= HandlePacketAccepted;
        }
    }

    void CacheReferences()
    {
        m_InputA ??= FindChildComponent<AluInputScanner>("Input 1");
        m_InputB ??= FindChildComponent<AluInputScanner>("Input 2");
        m_OperationLabelText ??= FindChildText("Screen Canvas/Operation Label");
        m_ResultSpawnTransform ??= transform.Find("Data Packet Spawn");
        m_ComputeParticles ??= GetComponentInChildren<ParticleSystem>(true);
        m_AluOpButtonRoot ??= transform.Find("ALUOp Button");
        m_AluSrcButtonRoot ??= transform.Find("ALUSrc Button");

        if (m_AluUiRoot == null)
        {
            var aluUiTransform = FindSceneTransformByName("ALU UI");
            m_AluUiRoot = aluUiTransform != null ? aluUiTransform.gameObject : null;
        }

        if (m_AluUiRoot != null)
        {
            m_LessonRuntimeText ??= FindNamedText(m_AluUiRoot.transform, "Text Lesson Runtime");
            m_AluOpStatusText ??= FindNamedText(m_AluUiRoot.transform, "Text ALUOp");
            m_AluSrcStatusText ??= FindNamedText(m_AluUiRoot.transform, "Text ALUSrc");
            m_Input1StatusText ??= FindNamedText(m_AluUiRoot.transform, "Text Input 1");
            m_Input2StatusText ??= FindNamedText(m_AluUiRoot.transform, "Text Input 2");
            m_FeedbackText ??= FindNamedText(m_AluUiRoot.transform, "Text Feedback");
            m_HintAluOpText ??= FindNamedText(m_AluUiRoot.transform, "Hint ALUOp");
            m_HintAluSrcText ??= FindNamedText(m_AluUiRoot.transform, "Hint ALUSrc");
            m_HintAluControlText ??= FindNamedText(m_AluUiRoot.transform, "Hint ALU Control");
            m_ExecuteButton ??= m_AluUiRoot.GetComponentInChildren<Button>(true);
            m_ExecuteButtonLabel ??= m_ExecuteButton != null
                ? m_ExecuteButton.GetComponentInChildren<TMP_Text>(true)
                : null;
            m_FunctDropdown ??= m_AluUiRoot.GetComponentInChildren<TMP_Dropdown>(true);
        }
    }

    string BuildLessonRuntimeText()
    {
        var instructionName = m_CurrentInstruction != null ? m_CurrentInstruction.displayName : "instruction";
        var assembly = m_CurrentInstruction != null ? m_CurrentInstruction.assemblyInstructionText : "add t2, t0, t1";

        return $"Instruction: {instructionName}\nAssembly: {assembly}";
    }

    void RefreshHintBlocks()
    {
        var selectedHint = m_HintDropdown != null ? m_HintDropdown.value : 0;

        SetHintBlockActive(m_HintAluOpText, selectedHint == 1);
        SetHintBlockActive(m_HintAluSrcText, selectedHint == 2);
        SetHintBlockActive(m_HintAluControlText, selectedHint == 3);
    }

    void PopulateHintDropdown()
    {
        if (m_HintDropdown == null)
            return;

        var selectedValue = Mathf.Clamp(m_HintDropdown.value, 0, 3);
        m_HintDropdown.ClearOptions();
        m_HintDropdown.AddOptions(new System.Collections.Generic.List<string>
        {
            "Choose Option",
            "ALUOp",
            "ALUSrc",
            "ALU Control",
        });
        m_HintDropdown.SetValueWithoutNotify(selectedValue);
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

    static void HookPhysicalButton(
        Transform buttonRoot,
        UnityEngine.Events.UnityAction<SelectEnterEventArgs> handler,
        bool subscribe)
    {
        var button = buttonRoot != null ? buttonRoot.GetComponent<XRSimpleInteractable>() : null;
        if (button == null)
            return;

        if (subscribe)
        {
            button.firstSelectEntered.RemoveListener(handler);
            button.firstSelectEntered.AddListener(handler);
        }
        else
        {
            button.firstSelectEntered.RemoveListener(handler);
        }
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

    T FindChildComponent<T>(string childName) where T : Component
    {
        var childTransform = transform.Find($"Visuals/{childName}");
        if (childTransform == null)
            childTransform = transform.Find(childName);

        return childTransform != null ? childTransform.GetComponent<T>() : null;
    }

    TMP_Text FindChildText(string childPath)
    {
        var childTransform = transform.Find(childPath);
        return childTransform != null ? childTransform.GetComponent<TMP_Text>() : null;
    }

    static TMP_Text FindNamedText(Transform root, string objectName)
    {
        if (root == null)
            return null;

        foreach (var textMesh in root.GetComponentsInChildren<TMP_Text>(true))
        {
            if (textMesh != null && textMesh.name == objectName)
                return textMesh;
        }

        return null;
    }

    static void SetHintBlockActive(TMP_Text textBlock, bool isActive)
    {
        if (textBlock == null)
            return;

        textBlock.gameObject.SetActive(isActive);
    }

    static Transform FindSceneTransformByName(string objectName)
    {
        foreach (var sceneTransform in Resources.FindObjectsOfTypeAll<Transform>())
        {
            if (sceneTransform == null || sceneTransform.name != objectName)
                continue;

            if (!sceneTransform.gameObject.scene.IsValid() || !sceneTransform.gameObject.scene.isLoaded)
                continue;

            return sceneTransform;
        }

        return null;
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

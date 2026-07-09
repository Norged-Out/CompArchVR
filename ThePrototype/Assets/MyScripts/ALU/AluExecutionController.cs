using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Owns the execution-phase interaction for the authored ALU prefab.
/// This includes:
/// - the two physical ALU control buttons on the prefab
/// - the ALU UI panel shown during the execute step
/// - packet validation for both ALU inputs
/// - result computation and result-packet spawning
/// </summary>
[DisallowMultipleComponent]
public partial class AluExecutionController : MonoBehaviour
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

}

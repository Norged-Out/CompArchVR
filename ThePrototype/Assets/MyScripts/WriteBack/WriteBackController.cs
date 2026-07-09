using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Owns the authored write-back prefab and WB UI.
/// Responsibilities:
/// - watch the RegDst / RegWrite / MemToReg physical buttons
/// - keep the register and data pedestals synced to those control settings
/// - validate the final write-back transfer
/// - run the authored pipe / particle sequence
/// - notify the lesson flow when write-back has been applied and when the user
///   is ready to continue to recap
/// </summary>
[DisallowMultipleComponent]
public partial class WriteBackController : MonoBehaviour
{
    [Header("Write-Back Prefab")]
    [SerializeField]
    WriteBackRegisterScanner m_RegisterScanner;

    [SerializeField]
    WriteBackPacketScanner m_PacketScanner;

    [SerializeField]
    ParticleSystem m_TransferParticles;

    [SerializeField]
    Transform m_RegDstButtonRoot;

    [SerializeField]
    Transform m_RegWriteButtonRoot;

    [SerializeField]
    Transform m_MemToRegButtonRoot;

    [SerializeField]
    Renderer[] m_PipeRenderers;

    [Header("WB UI")]
    [SerializeField]
    GameObject m_WbUiRoot;

    [SerializeField]
    TMP_Text m_LessonRuntimeText;

    [SerializeField]
    TMP_Text m_RegDstStatusText;

    [SerializeField]
    TMP_Text m_RegWriteStatusText;

    [SerializeField]
    TMP_Text m_MemToRegStatusText;

    [SerializeField]
    TMP_Text m_RegisterStatusText;

    [SerializeField]
    TMP_Text m_DataStatusText;

    [SerializeField]
    TMP_Text m_FeedbackText;

    [SerializeField]
    Button m_ActionButton;

    [SerializeField]
    TMP_Text m_ActionButtonLabel;

    [SerializeField]
    TMP_Dropdown m_HintDropdown;

    [SerializeField]
    TMP_Text m_HintRegDstText;

    [SerializeField]
    TMP_Text m_HintRegWriteText;

    [SerializeField]
    TMP_Text m_HintMemToRegText;

    [Header("Timing")]
    [SerializeField]
    float m_PipeStepDelaySeconds = 0.3f;

    [SerializeField]
    float m_ParticleLeadTimeSeconds = 0.75f;

    [SerializeField]
    string m_ExecuteButtonText = "Execute Write Back";

    [SerializeField]
    string m_ContinueButtonText = "Continue";

    [SerializeField]
    Color m_SuccessFeedbackColor = new(0.78f, 0.96f, 0.82f, 1f);

    [SerializeField]
    Color m_FailureFeedbackColor = new(1f, 0.55f, 0.55f, 1f);

    InstructionDefinition m_CurrentInstruction;
    RegisterBank m_RegisterBank;
    Coroutine m_TransferRoutine;
    readonly Dictionary<Renderer, Material> m_OriginalPipeMaterials = new();
    bool m_IsPhaseActive;
    bool m_IsAwaitingContinue;
    bool m_HasAppliedWriteBack;
    int m_LastTransferredValue;
    string m_LastTargetRegister = string.Empty;
    DataPacketRole m_LastTransferredPacketRole = DataPacketRole.None;
    string m_RegDstValue = "0";
    string m_RegWriteValue = "0";
    string m_MemToRegValue = "0";

    public event System.Action<string, int> WriteBackApplied;
    public event System.Action ContinueRequested;

    void Awake()
    {
        CacheReferences();
        PopulateHintDropdown();
        HookButtons();
        HookHintDropdown(true);
        CachePipeMaterials();
        ResetPipeMaterials();
        RefreshPresentation();
        SetFeedback(string.Empty, false);
    }

    void OnEnable()
    {
        CacheReferences();
        PopulateHintDropdown();
        HookButtons();
        HookHintDropdown(true);
        HookScannerEvents(true);
        CachePipeMaterials();
        RefreshPresentation();
    }

    void OnDisable()
    {
        HookScannerEvents(false);
        HookHintDropdown(false);
        UnhookButtons();
    }

    public void SetPhaseState(bool isActive, InstructionDefinition instruction, RegisterBank registerBank)
    {
        CacheReferences();

        var instructionChanged = instruction != null && instruction != m_CurrentInstruction;
        var isEnteringPhase = isActive && !m_IsPhaseActive;

        m_IsPhaseActive = isActive;
        m_CurrentInstruction = instruction != null ? instruction : InstructionDefaults.CreateFallbackAdd();
        m_RegisterBank = registerBank != null ? registerBank : m_RegisterBank;

        if (isEnteringPhase || instructionChanged)
            PrepareForWriteBackStep();

        if (m_WbUiRoot != null)
            m_WbUiRoot.SetActive(isActive);

        m_RegisterScanner?.SetActive(isActive);
        m_PacketScanner?.SetActive(isActive);
        RefreshPresentation();
    }

    public void ResetWriteBackState()
    {
        if (m_TransferRoutine != null)
        {
            StopCoroutine(m_TransferRoutine);
            m_TransferRoutine = null;
        }

        if (m_TransferParticles != null)
            m_TransferParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        m_CurrentInstruction = null;
        m_IsPhaseActive = false;
        m_IsAwaitingContinue = false;
        m_HasAppliedWriteBack = false;
        m_LastTransferredValue = 0;
        m_LastTargetRegister = string.Empty;
        m_LastTransferredPacketRole = DataPacketRole.None;
        m_RegDstValue = "0";
        m_RegWriteValue = "0";
        m_MemToRegValue = "0";

        m_RegisterScanner?.ResetScanner();
        m_PacketScanner?.ResetScanner();
        ResetPipeMaterials();
        SetFeedback(string.Empty, false);
        RefreshPresentation();

        if (m_WbUiRoot != null)
            m_WbUiRoot.SetActive(false);
    }

    public void HandleActionPressed()
    {
        if (!m_IsPhaseActive || m_TransferRoutine != null)
            return;

        if (m_IsAwaitingContinue)
        {
            m_IsAwaitingContinue = false;
            ContinueRequested?.Invoke();
            return;
        }

        if (!TryValidateSetup(out var validationMessage))
        {
            SetFeedback(validationMessage, true);
            RefreshPresentation();
            return;
        }

        SetFeedback("Write-back confirmed. Transferring value into the destination register...", false);
        RefreshPresentation();
        m_TransferRoutine = StartCoroutine(ApplyWriteBackRoutine());
    }
}

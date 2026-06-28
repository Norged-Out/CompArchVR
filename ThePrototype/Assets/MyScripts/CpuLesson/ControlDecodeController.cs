using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// Scene-authored control decode interaction controller.
/// All button and text references are assigned directly from Testing Ground.
/// </summary>
[DisallowMultipleComponent]
public class ControlDecodeController : MonoBehaviour
{
    enum ControlSignal
    {
        RegDst,
        Branch,
        MemRead,
        MemtoReg,
        ALUOp,
        MemWrite,
        ALUSrc,
        RegWrite,
    }

    [SerializeField]
    CpuLessonFlow m_LessonFlow;

    [Header("Physical Buttons")]
    [SerializeField]
    Transform m_RegDstButtonRoot;

    [SerializeField]
    Transform m_BranchButtonRoot;

    [SerializeField]
    Transform m_MemReadButtonRoot;

    [SerializeField]
    Transform m_MemtoRegButtonRoot;

    [SerializeField]
    Transform m_ALUOpButtonRoot;

    [SerializeField]
    Transform m_MemWriteButtonRoot;

    [SerializeField]
    Transform m_ALUSrcButtonRoot;

    [SerializeField]
    Transform m_RegWriteButtonRoot;

    [Header("UI")]
    [SerializeField]
    Button m_CheckButton;

    [SerializeField]
    TMP_Text m_CheckButtonLabel;

    [SerializeField]
    TMP_Text m_RegDstText;

    [SerializeField]
    TMP_Text m_BranchText;

    [SerializeField]
    TMP_Text m_MemReadText;

    [SerializeField]
    TMP_Text m_MemtoRegText;

    [SerializeField]
    TMP_Text m_ALUOpText;

    [SerializeField]
    TMP_Text m_MemWriteText;

    [SerializeField]
    TMP_Text m_ALUSrcText;

    [SerializeField]
    TMP_Text m_RegWriteText;

    [SerializeField]
    TMP_Text m_ProgressFeedbackText;

    [SerializeField]
    string m_DefaultCheckLabel = "Check Signals";

    [SerializeField]
    string m_SuccessCheckLabel = "Correct";

    [SerializeField]
    string m_FailureCheckLabel = "Try Again";

    [SerializeField]
    string m_ContinueLabel = "Continue";

    [SerializeField]
    Color m_DefaultTextColor = Color.white;

    [SerializeField]
    Color m_SuccessTextColor = new Color(0.78f, 0.96f, 0.82f, 1f);

    [SerializeField]
    Color m_FailureTextColor = new Color(1f, 0.55f, 0.55f, 1f);

    string m_RegDstValue = "0";
    string m_BranchValue = "0";
    string m_MemReadValue = "0";
    string m_MemtoRegValue = "0";
    string m_ALUOpValue = "00";
    string m_MemWriteValue = "0";
    string m_ALUSrcValue = "0";
    string m_RegWriteValue = "0";
    bool m_IsPhaseActive;
    bool m_IsSolvedAwaitingContinue;
    InstructionDefinition m_CurrentInstruction;

    public string CurrentALUSrcValue => m_ALUSrcValue;
    public string CurrentALUOpValue => m_ALUOpValue;
    public InstructionDefinition CurrentInstruction => m_CurrentInstruction;

    void Awake()
    {
        RefreshSignalText();
        SetCheckButtonLabel(m_DefaultCheckLabel);
        SetProgressFeedback(string.Empty, false);
    }

    void OnEnable()
    {
        HookButton(m_RegDstButtonRoot, HandleRegDstPressed, true);
        HookButton(m_BranchButtonRoot, HandleBranchPressed, true);
        HookButton(m_MemReadButtonRoot, HandleMemReadPressed, true);
        HookButton(m_MemtoRegButtonRoot, HandleMemtoRegPressed, true);
        HookButton(m_ALUOpButtonRoot, HandleALUOpPressed, true);
        HookButton(m_MemWriteButtonRoot, HandleMemWritePressed, true);
        HookButton(m_ALUSrcButtonRoot, HandleALUSrcPressed, true);
        HookButton(m_RegWriteButtonRoot, HandleRegWritePressed, true);

        if (m_CheckButton != null)
        {
            m_CheckButton.onClick.RemoveListener(HandleCheckPressed);
            m_CheckButton.onClick.AddListener(HandleCheckPressed);
        }
    }

    void OnDisable()
    {
        HookButton(m_RegDstButtonRoot, HandleRegDstPressed, false);
        HookButton(m_BranchButtonRoot, HandleBranchPressed, false);
        HookButton(m_MemReadButtonRoot, HandleMemReadPressed, false);
        HookButton(m_MemtoRegButtonRoot, HandleMemtoRegPressed, false);
        HookButton(m_ALUOpButtonRoot, HandleALUOpPressed, false);
        HookButton(m_MemWriteButtonRoot, HandleMemWritePressed, false);
        HookButton(m_ALUSrcButtonRoot, HandleALUSrcPressed, false);
        HookButton(m_RegWriteButtonRoot, HandleRegWritePressed, false);

        if (m_CheckButton != null)
            m_CheckButton.onClick.RemoveListener(HandleCheckPressed);
    }

    public void SetPhaseState(bool isActive, InstructionDefinition instruction)
    {
        var phaseJustActivated = isActive && !m_IsPhaseActive;
        var instructionChanged = instruction != m_CurrentInstruction;

        m_IsPhaseActive = isActive;
        m_CurrentInstruction = instruction;

        if (m_CheckButton != null)
            m_CheckButton.interactable = isActive;

        if (!isActive)
        {
            m_IsSolvedAwaitingContinue = false;
            ResetSignalColors();
            SetCheckButtonLabel(m_DefaultCheckLabel);
            SetProgressFeedback(string.Empty, false);
            return;
        }

        if (phaseJustActivated || instructionChanged)
            ResetSelections();

        RefreshSignalText();
    }

    void HandleRegDstPressed(SelectEnterEventArgs _)
    {
        ToggleBinarySignal(ControlSignal.RegDst);
    }

    void HandleBranchPressed(SelectEnterEventArgs _)
    {
        ToggleBinarySignal(ControlSignal.Branch);
    }

    void HandleMemReadPressed(SelectEnterEventArgs _)
    {
        ToggleBinarySignal(ControlSignal.MemRead);
    }

    void HandleMemtoRegPressed(SelectEnterEventArgs _)
    {
        ToggleBinarySignal(ControlSignal.MemtoReg);
    }

    void HandleALUOpPressed(SelectEnterEventArgs _)
    {
        if (!m_IsPhaseActive)
            return;

        m_ALUOpValue = m_ALUOpValue switch
        {
            "00" => "01",
            "01" => "10",
            _ => "00",
        };

        HandleSignalChanged();
    }

    void HandleMemWritePressed(SelectEnterEventArgs _)
    {
        ToggleBinarySignal(ControlSignal.MemWrite);
    }

    void HandleALUSrcPressed(SelectEnterEventArgs _)
    {
        ToggleBinarySignal(ControlSignal.ALUSrc);
    }

    void HandleRegWritePressed(SelectEnterEventArgs _)
    {
        ToggleBinarySignal(ControlSignal.RegWrite);
    }

    void HandleCheckPressed()
    {
        if (!m_IsPhaseActive || m_CurrentInstruction == null)
            return;

        if (m_IsSolvedAwaitingContinue)
        {
            m_IsSolvedAwaitingContinue = false;
            SetProgressFeedback(string.Empty, false);
            m_LessonFlow?.Advance();
            return;
        }

        var expectedRegDst = m_CurrentInstruction.usesDestinationRegister ? "1" : "0";
        var expectedBranch = "0";
        var expectedMemRead = m_CurrentInstruction.mnemonic == InstructionMnemonic.Lw ? "1" : "0";
        var expectedMemtoReg = m_CurrentInstruction.mnemonic == InstructionMnemonic.Lw ? "1" : "0";
        var expectedALUOp = m_CurrentInstruction.mnemonic == InstructionMnemonic.Add ? "10" : "00";
        var expectedMemWrite = "0";
        var expectedALUSrc = m_CurrentInstruction.usesImmediate ? "1" : "0";
        var expectedRegWrite = m_CurrentInstruction.writesRegisterFile ? "1" : "0";

        var isRegDstCorrect = ValidateSignal(m_RegDstText, m_RegDstValue, expectedRegDst);
        var isBranchCorrect = ValidateSignal(m_BranchText, m_BranchValue, expectedBranch);
        var isMemReadCorrect = ValidateSignal(m_MemReadText, m_MemReadValue, expectedMemRead);
        var isMemtoRegCorrect = ValidateSignal(m_MemtoRegText, m_MemtoRegValue, expectedMemtoReg);
        var isALUOpCorrect = ValidateSignal(m_ALUOpText, m_ALUOpValue, expectedALUOp);
        var isMemWriteCorrect = ValidateSignal(m_MemWriteText, m_MemWriteValue, expectedMemWrite);
        var isALUSrcCorrect = ValidateSignal(m_ALUSrcText, m_ALUSrcValue, expectedALUSrc);
        var isRegWriteCorrect = ValidateSignal(m_RegWriteText, m_RegWriteValue, expectedRegWrite);

        var isCorrect = isRegDstCorrect &&
                        isBranchCorrect &&
                        isMemReadCorrect &&
                        isMemtoRegCorrect &&
                        isALUOpCorrect &&
                        isMemWriteCorrect &&
                        isALUSrcCorrect &&
                        isRegWriteCorrect;

        SetCheckButtonLabel(isCorrect ? m_SuccessCheckLabel : m_FailureCheckLabel);

        if (isCorrect)
        {
            m_IsSolvedAwaitingContinue = true;
            SetCheckButtonLabel(m_ContinueLabel);
            SetProgressFeedback("Control signals are correct. Click the button once more to proceed.", false);
        }
        else
        {
            m_IsSolvedAwaitingContinue = false;
            SetProgressFeedback("Some control signals are still incorrect. Fix the highlighted entries and check again.", true);
        }
    }

    void ToggleBinarySignal(ControlSignal signal)
    {
        if (!m_IsPhaseActive)
            return;

        switch (signal)
        {
            case ControlSignal.RegDst:
                m_RegDstValue = m_RegDstValue == "1" ? "0" : "1";
                break;
            case ControlSignal.Branch:
                m_BranchValue = m_BranchValue == "1" ? "0" : "1";
                break;
            case ControlSignal.MemRead:
                m_MemReadValue = m_MemReadValue == "1" ? "0" : "1";
                break;
            case ControlSignal.MemtoReg:
                m_MemtoRegValue = m_MemtoRegValue == "1" ? "0" : "1";
                break;
            case ControlSignal.MemWrite:
                m_MemWriteValue = m_MemWriteValue == "1" ? "0" : "1";
                break;
            case ControlSignal.ALUSrc:
                m_ALUSrcValue = m_ALUSrcValue == "1" ? "0" : "1";
                break;
            case ControlSignal.RegWrite:
                m_RegWriteValue = m_RegWriteValue == "1" ? "0" : "1";
                break;
        }

        HandleSignalChanged();
    }

    void HandleSignalChanged()
    {
        m_IsSolvedAwaitingContinue = false;
        RefreshSignalText();
        ResetSignalColors();
        SetCheckButtonLabel(m_DefaultCheckLabel);
        SetProgressFeedback(string.Empty, false);
    }

    void ResetSelections()
    {
        m_IsSolvedAwaitingContinue = false;
        m_RegDstValue = "0";
        m_BranchValue = "0";
        m_MemReadValue = "0";
        m_MemtoRegValue = "0";
        m_ALUOpValue = "00";
        m_MemWriteValue = "0";
        m_ALUSrcValue = "0";
        m_RegWriteValue = "0";

        ResetSignalColors();
        RefreshSignalText();
        SetCheckButtonLabel(m_DefaultCheckLabel);
        SetProgressFeedback(string.Empty, false);
    }

    void RefreshSignalText()
    {
        SetSignalText(m_RegDstText, "RegDst", m_RegDstValue);
        SetSignalText(m_BranchText, "Branch", m_BranchValue);
        SetSignalText(m_MemReadText, "MemRead", m_MemReadValue);
        SetSignalText(m_MemtoRegText, "MemtoReg", m_MemtoRegValue);
        SetSignalText(m_ALUOpText, "ALUOp", m_ALUOpValue);
        SetSignalText(m_MemWriteText, "MemWrite", m_MemWriteValue);
        SetSignalText(m_ALUSrcText, "ALUSrc", m_ALUSrcValue);
        SetSignalText(m_RegWriteText, "RegWrite", m_RegWriteValue);
    }

    bool ValidateSignal(TMP_Text target, string actualValue, string expectedValue)
    {
        var isCorrect = actualValue == expectedValue;
        if (target != null)
            target.color = isCorrect ? m_SuccessTextColor : m_FailureTextColor;

        return isCorrect;
    }

    void ResetSignalColors()
    {
        SetSignalColor(m_RegDstText, m_DefaultTextColor);
        SetSignalColor(m_BranchText, m_DefaultTextColor);
        SetSignalColor(m_MemReadText, m_DefaultTextColor);
        SetSignalColor(m_MemtoRegText, m_DefaultTextColor);
        SetSignalColor(m_ALUOpText, m_DefaultTextColor);
        SetSignalColor(m_MemWriteText, m_DefaultTextColor);
        SetSignalColor(m_ALUSrcText, m_DefaultTextColor);
        SetSignalColor(m_RegWriteText, m_DefaultTextColor);
    }

    void SetCheckButtonLabel(string labelText)
    {
        if (m_CheckButtonLabel != null)
            m_CheckButtonLabel.text = labelText;
    }

    void SetProgressFeedback(string message, bool isFailure)
    {
        if (m_ProgressFeedbackText == null)
            return;

        m_ProgressFeedbackText.text = message;
        m_ProgressFeedbackText.color = isFailure ? m_FailureTextColor : m_SuccessTextColor;
        m_ProgressFeedbackText.gameObject.SetActive(!string.IsNullOrWhiteSpace(message));
    }

    static void SetSignalText(TMP_Text target, string label, string value)
    {
        if (target != null)
            target.text = $"{label}: {value}";
    }

    static void SetSignalColor(TMP_Text target, Color color)
    {
        if (target != null)
            target.color = color;
    }

    static void HookButton(Transform buttonRoot, UnityEngine.Events.UnityAction<SelectEnterEventArgs> handler, bool subscribe)
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
}

using TMPro;
using UnityEngine;

/// <summary>
/// First-pass ALU stage coordinator.
/// For now it only waits for both expected input packets to be accepted and
/// computes the result in code. Result spawning is left for the next pass.
/// </summary>
[DisallowMultipleComponent]
public class AluExecutionController : MonoBehaviour
{
    [SerializeField]
    AluInputScanner m_InputA;

    [SerializeField]
    AluInputScanner m_InputB;

    [SerializeField]
    TMP_Text m_FeedbackText;

    [SerializeField]
    bool m_IsActive;

    public bool IsReady => m_InputA != null && m_InputB != null &&
                           m_InputA.AcceptedPacket != null && m_InputB.AcceptedPacket != null;

    public int CurrentResultValue { get; private set; }

    void Awake()
    {
        RefreshFeedback();
    }

    void OnEnable()
    {
        HookInputEvents(true);
        RefreshFeedback();
    }

    void OnDisable()
    {
        HookInputEvents(false);
    }

    public void SetActive(bool isActive)
    {
        m_IsActive = isActive;

        m_InputA?.SetActive(isActive);
        m_InputB?.SetActive(isActive);

        if (!isActive)
        {
            CurrentResultValue = 0;
            m_InputA?.ResetScanner();
            m_InputB?.ResetScanner();
        }

        RefreshFeedback();
    }

    public bool TryComputeAddResult(out int resultValue)
    {
        resultValue = 0;
        if (!m_IsActive || !IsReady)
            return false;

        resultValue = m_InputA.AcceptedValue + m_InputB.AcceptedValue;
        CurrentResultValue = resultValue;
        RefreshFeedback();
        return true;
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

    void HandlePacketAccepted(AluInputScanner _, DataPacketToken __)
    {
        RefreshFeedback();
    }

    void RefreshFeedback()
    {
        if (m_FeedbackText == null)
            return;

        if (!m_IsActive)
        {
            m_FeedbackText.text = "ALU idle";
            return;
        }

        if (!IsReady)
        {
            m_FeedbackText.text = "Waiting for ALU input packets.";
            return;
        }

        m_FeedbackText.text = $"ALU ready: {m_InputA.AcceptedValue} + {m_InputB.AcceptedValue}";
    }
}

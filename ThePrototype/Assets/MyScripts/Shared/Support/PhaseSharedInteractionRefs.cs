using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public sealed class PhaseSharedInteractionRefs
{
    [SerializeField]
    TMP_Text m_FeedbackText;

    [SerializeField]
    Button m_ActionButton;

    [SerializeField]
    TMP_Text m_ActionLabel;

    public TMP_Text FeedbackText => m_FeedbackText;
    public Button ActionButton => m_ActionButton;
    public TMP_Text ActionLabel => m_ActionLabel;
}

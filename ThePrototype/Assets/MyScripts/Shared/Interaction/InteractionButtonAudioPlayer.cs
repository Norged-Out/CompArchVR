using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// Small optional bridge that lets a reusable button root play a single authored
/// audio source from either XR presses or normal Unity UI clicks.
/// </summary>
[DisallowMultipleComponent]
public sealed class InteractionButtonAudioPlayer : MonoBehaviour
{
    [SerializeField]
    XRSimpleInteractable m_Interactable;

    [SerializeField]
    Button m_Button;

    [SerializeField]
    AudioSource m_AudioSource;

    void Awake()
    {
        CacheReferences();
    }

    void OnEnable()
    {
        CacheReferences();

        if (m_Interactable != null)
        {
            m_Interactable.firstSelectEntered.RemoveListener(HandleFirstSelectEntered);
            m_Interactable.firstSelectEntered.AddListener(HandleFirstSelectEntered);
        }

        if (m_Button != null)
        {
            m_Button.onClick.RemoveListener(PlayPressAudio);
            m_Button.onClick.AddListener(PlayPressAudio);
        }
    }

    void OnDisable()
    {
        if (m_Interactable != null)
            m_Interactable.firstSelectEntered.RemoveListener(HandleFirstSelectEntered);

        if (m_Button != null)
            m_Button.onClick.RemoveListener(PlayPressAudio);
    }

    /// <summary>
    /// Shared manual entry point so scene-side UnityEvents can also reuse the cue.
    /// </summary>
    public void PlayPressAudio()
    {
        if (m_AudioSource == null)
            return;

        m_AudioSource.Stop();
        m_AudioSource.Play();
    }

    void HandleFirstSelectEntered(SelectEnterEventArgs _)
    {
        PlayPressAudio();
    }

    void CacheReferences()
    {
        m_Interactable ??= GetComponent<XRSimpleInteractable>();
        m_Button ??= GetComponent<Button>();
        m_AudioSource ??= GetComponentInChildren<AudioSource>(true);
    }
}

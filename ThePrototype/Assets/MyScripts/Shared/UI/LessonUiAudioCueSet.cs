using System;
using UnityEngine;

/// <summary>
/// Shared serialized bundle for lesson UI cue playback.
/// One authored audio source can spatialize a small family of phase-level cues.
/// </summary>
[Serializable]
public sealed class LessonUiAudioCueSet
{
    [SerializeField]
    AudioSource m_AudioSource;

    [SerializeField]
    AudioClip m_PhaseActivatedClip;

    [SerializeField]
    AudioClip m_PhaseCompletedClip;

    [SerializeField]
    AudioClip m_IncorrectClip;

    [SerializeField]
    AudioClip m_LessonCompletedClip;

    [SerializeField]
    AudioClip m_FailureClip;

    /// <summary>
    /// Plays the phase-entered cue from the authored source.
    /// </summary>
    public void PlayPhaseActivatedCue() => PlayClip(m_PhaseActivatedClip);

    /// <summary>
    /// Plays the phase-cleared cue from the authored source.
    /// </summary>
    public void PlayPhaseCompletedCue() => PlayClip(m_PhaseCompletedClip);

    /// <summary>
    /// Plays the incorrect-action cue from the authored source.
    /// </summary>
    public void PlayIncorrectCue() => PlayClip(m_IncorrectClip);

    /// <summary>
    /// Plays the final lesson-complete cue from the authored source.
    /// </summary>
    public void PlayLessonCompletedCue() => PlayClip(m_LessonCompletedClip);

    /// <summary>
    /// Plays the authored failure cue from the authored source.
    /// </summary>
    public void PlayFailureCue() => PlayClip(m_FailureClip);

    void PlayClip(AudioClip clip)
    {
        if (m_AudioSource == null || clip == null)
            return;

        m_AudioSource.PlayOneShot(clip);
    }
}

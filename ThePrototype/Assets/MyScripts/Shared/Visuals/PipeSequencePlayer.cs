using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Reusable authored pipe animation helper for phases that need a simple
/// sequential material sweep.
/// Attach this to a phase root, bind the pipe renderers in order, then call
/// the provided playback methods from the owning controller.
/// </summary>
[DisallowMultipleComponent]
public sealed class PipeSequencePlayer : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField]
    Renderer[] m_PipeRenderers;

    [Header("Materials")]
    [SerializeField]
    Material m_IdleMaterial;

    [SerializeField]
    Material m_WaitingMaterial;

    [SerializeField]
    Material m_SuccessMaterial;

    [Header("Timing")]
    [SerializeField]
    float m_DefaultStepDelaySeconds = 0.4f;

    readonly Dictionary<Renderer, Material> m_OriginalMaterials = new();
    Coroutine m_PlaybackRoutine;

    /// <summary>
    /// Shared authored pipe timing used when a phase does not explicitly
    /// override the sweep delay.
    /// </summary>
    public float DefaultStepDelaySeconds => m_DefaultStepDelaySeconds;

    /// <summary>
    /// Number of authored pipe renderers bound to this player.
    /// </summary>
    public int PipeCount => m_PipeRenderers != null ? m_PipeRenderers.Length : 0;

    void Awake()
    {
        CacheOriginalMaterials();
    }

    void OnValidate()
    {
        if (!Application.isPlaying)
            CacheOriginalMaterials();
    }

    /// <summary>
    /// Stops any active pipe animation and restores the authored idle state.
    /// </summary>
    public void ResetToIdle()
    {
        StopPlayback();

        foreach (var pipeRenderer in m_PipeRenderers)
        {
            if (pipeRenderer == null)
                continue;

            pipeRenderer.sharedMaterial = m_IdleMaterial != null
                ? m_IdleMaterial
                : GetOriginalMaterial(pipeRenderer);
        }
    }

    /// <summary>
    /// Plays a one-shot waiting sweep across the authored pipe order.
    /// </summary>
    public void PlayWaitingSweep(bool reverse = false, float? stepDelaySeconds = null)
    {
        PlaySweep(m_WaitingMaterial, reverse, stepDelaySeconds, null);
    }

    /// <summary>
    /// Plays a one-shot success sweep across the authored pipe order.
    /// </summary>
    public void PlaySuccessSweep(bool reverse = false, float? stepDelaySeconds = null)
    {
        PlaySweep(m_SuccessMaterial, reverse, stepDelaySeconds, null);
    }

    /// <summary>
    /// Plays a one-shot idle sweep so a phase can visually settle back to its
    /// baseline after it finishes.
    /// </summary>
    public void PlayIdleSweep(bool reverse = false, float? stepDelaySeconds = null, Action onComplete = null)
    {
        PlaySweep(m_IdleMaterial, reverse, stepDelaySeconds, onComplete);
    }

    /// <summary>
    /// Plays a one-shot success sweep and fires a callback when the authored
    /// sweep finishes.
    /// </summary>
    public void PlaySuccessSweep(bool reverse, float? stepDelaySeconds, Action onComplete)
    {
        PlaySweep(m_SuccessMaterial, reverse, stepDelaySeconds, onComplete);
    }

    /// <summary>
    /// Forces all authored pipes into the waiting material without animation.
    /// </summary>
    public void ApplyWaitingState()
    {
        ApplyMaterialToAll(m_WaitingMaterial);
    }

    /// <summary>
    /// Stops any active playback coroutine without changing the current pipe
    /// materials. Use ResetToIdle when the visuals should also be restored.
    /// </summary>
    public void StopPlayback()
    {
        if (m_PlaybackRoutine == null)
            return;

        StopCoroutine(m_PlaybackRoutine);
        m_PlaybackRoutine = null;
    }

    void PlaySweep(Material targetMaterial, bool reverse, float? stepDelaySeconds, Action onComplete)
    {
        StopPlayback();
        m_PlaybackRoutine = StartCoroutine(PlaySweepRoutine(targetMaterial, reverse, stepDelaySeconds ?? m_DefaultStepDelaySeconds, onComplete));
    }

    IEnumerator PlaySweepRoutine(Material targetMaterial, bool reverse, float stepDelaySeconds, Action onComplete)
    {
        if (m_PipeRenderers == null || m_PipeRenderers.Length == 0 || targetMaterial == null)
        {
            m_PlaybackRoutine = null;
            onComplete?.Invoke();
            yield break;
        }

        if (reverse)
        {
            for (var index = m_PipeRenderers.Length - 1; index >= 0; index--)
            {
                ApplyMaterial(m_PipeRenderers[index], targetMaterial);
                yield return new WaitForSeconds(stepDelaySeconds);
            }
        }
        else
        {
            for (var index = 0; index < m_PipeRenderers.Length; index++)
            {
                ApplyMaterial(m_PipeRenderers[index], targetMaterial);
                yield return new WaitForSeconds(stepDelaySeconds);
            }
        }

        m_PlaybackRoutine = null;
        onComplete?.Invoke();
    }

    void CacheOriginalMaterials()
    {
        m_OriginalMaterials.Clear();

        if (m_PipeRenderers == null)
            return;

        foreach (var pipeRenderer in m_PipeRenderers)
        {
            if (pipeRenderer != null && pipeRenderer.sharedMaterial != null)
                m_OriginalMaterials[pipeRenderer] = pipeRenderer.sharedMaterial;
        }
    }

    Material GetOriginalMaterial(Renderer pipeRenderer)
    {
        return pipeRenderer != null && m_OriginalMaterials.TryGetValue(pipeRenderer, out var material)
            ? material
            : null;
    }

    void ApplyMaterialToAll(Material material)
    {
        if (material == null || m_PipeRenderers == null)
            return;

        foreach (var pipeRenderer in m_PipeRenderers)
            ApplyMaterial(pipeRenderer, material);
    }

    static void ApplyMaterial(Renderer pipeRenderer, Material material)
    {
        if (pipeRenderer != null && material != null)
            pipeRenderer.sharedMaterial = material;
    }
}

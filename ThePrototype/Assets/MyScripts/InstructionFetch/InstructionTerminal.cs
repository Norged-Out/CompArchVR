using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Shared prefab script for both instruction-platform behaviors:
/// - uploader: spawns an empty module and can upload the current instruction
/// - downloader: accepts a carried module and locks it in place as a gate key
/// </summary>
[DisallowMultipleComponent]
public class InstructionTerminal : MonoBehaviour
{
    public enum TerminalMode
    {
        Uploader,
        Downloader,
    }

    [Serializable]
    public class InstructionModuleEvent : UnityEvent<InstructionModule> { }

    [Serializable]
    public class InstructionDefinitionEvent : UnityEvent<InstructionDefinition> { }

    [Header("Mode")]
    [SerializeField]
    TerminalMode m_Mode = TerminalMode.Uploader;

    [Header("Scene References")]
    [SerializeField]
    InstructionModule m_ModulePrefab;

    [SerializeField]
    CpuLessonFlow m_LessonFlow;

    [SerializeField]
    InstructionDefinition m_InstructionOverride;

    [SerializeField]
    Transform m_SpawnPoint;

    [SerializeField]
    ParticleSystem m_ParticleSystem;

    [SerializeField]
    float m_ParticleBurstSeconds = 1.25f;

    [SerializeField]
    float m_StateChangeDelaySeconds = 0.35f;

    [Header("Audio")]
    [SerializeField]
    AudioSource m_ActionAudioSource;

    [Header("Uploader Tuning")]
    [SerializeField]
    bool m_SpawnEmptyModuleOnEnable = true;

    [Header("Downloader Tuning")]
    [SerializeField]
    bool m_RequireInstructionBeforeDownload = true;

    [SerializeField]
    bool m_LockModuleOnDownload = true;

    [SerializeField]
    float m_StableDownloadSeconds = 0.5f;

    [Header("Events")]
    [SerializeField]
    InstructionModuleEvent m_OnModuleSpawned = new();

    [SerializeField]
    InstructionDefinitionEvent m_OnInstructionUploaded = new();

    [SerializeField]
    InstructionModuleEvent m_OnModuleDownloaded = new();

    InstructionModule m_SpawnedModule;
    InstructionModule m_DownloadedModule;
    InstructionModule m_CurrentCandidate;
    float m_CurrentCandidateTime;
    Coroutine m_ParticleRoutine;
    Coroutine m_StateChangeRoutine;

    public TerminalMode Mode => m_Mode;
    public InstructionModule SpawnedModule => m_SpawnedModule;
    public InstructionModule DownloadedModule => m_DownloadedModule;
    public bool HasDownloadedModule => m_DownloadedModule != null;

    void Awake()
    {
        CacheReferences();
    }

    void OnEnable()
    {
        CacheReferences();

        // The uploader owns the baseline empty module for each lesson run.
        if (m_Mode == TerminalMode.Uploader && m_SpawnEmptyModuleOnEnable)
            EnsureSpawnedModule();
    }

    void OnValidate()
    {
        CacheReferences();
    }

    /// <summary>
    /// Resets runtime state so a new lesson can begin with a clean terminal.
    /// The uploader respawns a fresh empty module, while the downloader clears
    /// any locked-in module from the previous run.
    /// </summary>
    public void ResetTerminal(bool respawnUploaderModule = true)
    {
        m_CurrentCandidate = null;
        m_CurrentCandidateTime = 0f;

        if (m_Mode == TerminalMode.Downloader)
        {
            if (m_DownloadedModule != null)
            {
                var downloadedGameObject = m_DownloadedModule.gameObject;
                m_DownloadedModule = null;
                Destroy(downloadedGameObject);
            }

            return;
        }

        if (m_SpawnedModule != null)
        {
            var spawnedGameObject = m_SpawnedModule.gameObject;
            m_SpawnedModule = null;
            Destroy(spawnedGameObject);
        }

        if (respawnUploaderModule)
            EnsureSpawnedModule();
    }

    public InstructionModule EnsureSpawnedModule()
    {
        if (m_Mode != TerminalMode.Uploader || m_ModulePrefab == null || m_SpawnPoint == null)
            return m_SpawnedModule;

        if (m_SpawnedModule != null)
            return m_SpawnedModule;

        m_SpawnedModule = Instantiate(
            m_ModulePrefab,
            m_SpawnPoint.position,
            m_SpawnPoint.rotation);

        // A newly spawned module starts blank and locked in place so fetch can
        // visibly "load" the selected instruction before the learner carries it.
        m_SpawnedModule.ClearInstruction();
        m_SpawnedModule.SnapToAnchor(m_SpawnPoint, true);
        m_OnModuleSpawned.Invoke(m_SpawnedModule);
        return m_SpawnedModule;
    }

    public bool UploadCurrentInstruction()
    {
        return UploadInstruction(ResolveInstructionToUpload());
    }

    public bool UploadInstruction(InstructionDefinition instruction)
    {
        if (m_Mode != TerminalMode.Uploader || instruction == null)
            return false;

        var module = EnsureSpawnedModule();
        if (module == null)
            return false;

        BeginUploadSequence(module, instruction);
        return true;
    }

    public bool HasMatchingDownloadedInstruction(InstructionDefinition instruction)
    {
        return m_DownloadedModule != null &&
               instruction != null &&
               m_DownloadedModule.CurrentInstruction == instruction;
    }

    public void ClearDownloadedModule(bool releaseGrab = false)
    {
        if (m_DownloadedModule == null)
            return;

        if (releaseGrab)
        {
            m_DownloadedModule.ReleaseFromAnchor();
            m_DownloadedModule.SetGrabEnabled(true);
        }

        m_DownloadedModule = null;
        m_CurrentCandidate = null;
        m_CurrentCandidateTime = 0f;
    }

    void OnTriggerStay(Collider other)
    {
        if (m_Mode != TerminalMode.Downloader)
            return;

        var module = other.GetComponentInParent<InstructionModule>();
        if (module == null)
            return;

        if (m_DownloadedModule != null || !CanAcceptModule(module))
            return;

        if (module != m_CurrentCandidate)
        {
            m_CurrentCandidate = module;
            m_CurrentCandidateTime = 0f;
        }

        m_CurrentCandidateTime += Time.deltaTime;
        if (m_CurrentCandidateTime >= m_StableDownloadSeconds)
            DockModule(module);
    }

    void OnTriggerExit(Collider other)
    {
        if (m_Mode != TerminalMode.Downloader)
            return;

        var module = other.GetComponentInParent<InstructionModule>();
        if (module == null)
            return;

        if (module == m_CurrentCandidate)
        {
            m_CurrentCandidate = null;
            m_CurrentCandidateTime = 0f;
        }
    }

    bool CanAcceptModule(InstructionModule module)
    {
        if (module == null)
            return false;

        if (module.IsGrabbed)
            return false;

        if (m_RequireInstructionBeforeDownload && !module.HasInstruction)
            return false;

        return true;
    }

    void DockModule(InstructionModule module)
    {
        m_DownloadedModule = module;
        m_CurrentCandidate = null;
        m_CurrentCandidateTime = 0f;

        // Decode uses the module like a physical gate key. Once it is accepted,
        // it snaps into place so the learner can clearly see the handoff finish.
        if (m_LockModuleOnDownload && m_SpawnPoint != null)
            module.SnapToAnchor(m_SpawnPoint, true);

        PlayActionAudio();
        PlayParticles();
        StartStateChangeRoutine(FinalizeDownloadedModuleAfterDelay(module));
    }

    InstructionDefinition ResolveInstructionToUpload()
    {
        if (m_InstructionOverride != null)
            return m_InstructionOverride;

        return m_LessonFlow != null ? m_LessonFlow.CurrentInstruction : null;
    }

    void CacheReferences()
    {
        if (m_SpawnPoint == null)
            m_SpawnPoint = FindChildTransform("SpawnPoint");

        if (m_ParticleSystem == null)
            m_ParticleSystem = GetComponentInChildren<ParticleSystem>(true);
    }

    void PlayParticles()
    {
        if (m_ParticleSystem == null)
            return;

        m_ParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        m_ParticleSystem.Play(true);

        if (m_ParticleRoutine != null)
            StopCoroutine(m_ParticleRoutine);

        m_ParticleRoutine = StartCoroutine(StopParticlesAfterDelay());
    }

    IEnumerator StopParticlesAfterDelay()
    {
        yield return new WaitForSeconds(m_ParticleBurstSeconds);

        if (m_ParticleSystem != null)
            m_ParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);

        m_ParticleRoutine = null;
    }

    void StartStateChangeRoutine(IEnumerator routine)
    {
        if (m_StateChangeRoutine != null)
            StopCoroutine(m_StateChangeRoutine);

        m_StateChangeRoutine = StartCoroutine(routine);
    }

    IEnumerator ApplyUploadedInstructionAfterDelay(InstructionModule module, InstructionDefinition instruction)
    {
        yield return new WaitForSeconds(m_StateChangeDelaySeconds);

        if (module != null)
        {
            module.ReleaseFromAnchor();
            module.UploadInstruction(instruction);
        }

        m_OnInstructionUploaded.Invoke(instruction);
        m_StateChangeRoutine = null;
    }

    IEnumerator FinalizeDownloadedModuleAfterDelay(InstructionModule module)
    {
        yield return new WaitForSeconds(m_StateChangeDelaySeconds);

        if (module != null)
            module.MarkDownloaded();

        m_OnModuleDownloaded.Invoke(module);
        m_LessonFlow?.NotifyInstructionModuleDownloaded(module);
        m_StateChangeRoutine = null;
    }

    void BeginUploadSequence(InstructionModule module, InstructionDefinition instruction)
    {
        if (module == null || instruction == null)
            return;

        // Lesson flow hears about the upload immediately so IF UI can update,
        // but the module itself waits a beat before changing state so the short
        // VFX burst reads like the instruction is being written into it.
        m_LessonFlow?.NotifyInstructionUploaded(instruction);
        PlayActionAudio();
        PlayParticles();
        StartStateChangeRoutine(ApplyUploadedInstructionAfterDelay(module, instruction));
    }

    /// <summary>
    /// Optional scene-authored sound cue used by both upload and download terminals.
    /// </summary>
    void PlayActionAudio()
    {
        if (m_ActionAudioSource == null)
            return;

        m_ActionAudioSource.Stop();
        m_ActionAudioSource.Play();
    }

    Transform FindChildTransform(string childName)
    {
        foreach (var childTransform in GetComponentsInChildren<Transform>(true))
        {
            if (childTransform == null || childTransform == transform)
                continue;

            if (childTransform.name.Equals(childName, StringComparison.Ordinal))
                return childTransform;
        }

        return null;
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Scene-side representation of the Data Memory bank.
/// It owns:
/// - the ordered list of authored word slots
/// - the central "Address / Data" readout
/// - pipe material animation between the memory bank and memory unit
/// - temporary highlight of whichever word matches the scanned address
/// </summary>
[DisallowMultipleComponent]
public class DataMemoryBank : MonoBehaviour
{
    [Header("Word Entries")]
    [SerializeField]
    MemoryWord[] m_Words = Array.Empty<MemoryWord>();

    [Header("Central Display")]
    [SerializeField]
    TMP_Text m_AddressText;

    [SerializeField]
    TMP_Text m_DataText;

    [Header("Pipe Animation")]
    [SerializeField]
    Renderer[] m_PipeRenderers = Array.Empty<Renderer>();

    [SerializeField]
    Material m_IdlePipeMaterial;

    [SerializeField]
    Material m_WaitingPipeMaterial;

    [SerializeField]
    Material m_TransferPipeMaterial;

    [SerializeField]
    float m_PipeStepDelaySeconds = 0.5f;

    MemoryWord m_HighlightedWord;
    Coroutine m_WaitingRoutine;
    Coroutine m_TransferRoutine;
    bool m_IsWaitingAnimationActive;

    public int WordCount => m_Words != null ? m_Words.Length : 0;

    void Awake()
    {
        CacheReferences();
        RebindWords();
        ResetPipeMaterials();
        ClearDisplay();
    }

    void OnEnable()
    {
        CacheReferences();
        RebindWords();
    }

    public void SetPhaseState(bool isActive, bool animateWaiting)
    {
        m_IsWaitingAnimationActive = isActive && animateWaiting;

        if (!isActive)
        {
            StopAllAnimations();
            ClearHighlightedWord();
            return;
        }

        if (animateWaiting)
            StartWaitingAnimation();
        else
            StopWaitingAnimation();
    }

    public bool TryReadWord(int address, out int value, out MemoryWord word)
    {
        word = GetWordByAddress(address);
        if (word == null)
        {
            value = 0;
            return false;
        }

        value = word.StoredValue;
        return true;
    }

    public bool TryWriteWord(int address, int value, out MemoryWord word)
    {
        word = GetWordByAddress(address);
        if (word == null)
            return false;

        word.SetStoredValue(value);
        ShowWordDetails(word);
        return true;
    }

    public void PreviewAddress(int address)
    {
        var word = GetWordByAddress(address);
        if (word == null)
        {
            ClearHighlightedWord();
            SetDisplay($"Address: {FormatAddress(address)}", "Value: no mapped word");
            return;
        }

        SetHighlightedWord(word);
        ShowWordDetails(word);
    }

    public void ClearPreview()
    {
        ClearHighlightedWord();
        ClearDisplay();
    }

    public void ShowWordDetails(MemoryWord word)
    {
        if (word == null)
            return;

        SetDisplay($"Address: {word.AddressDisplay}", $"Value: {word.DataDisplay}");
    }

    public bool ShouldAllowHoverPreview()
    {
        return m_HighlightedWord == null;
    }

    public void HandleWordHoverExited(MemoryWord word)
    {
        if (m_HighlightedWord != null)
        {
            ShowWordDetails(m_HighlightedWord);
            return;
        }

        if (word != null)
            ClearDisplay();
    }

    public void PlayTransferSequence(bool bankToUnit, Action onComplete = null)
    {
        if (m_TransferRoutine != null)
            StopCoroutine(m_TransferRoutine);

        StopWaitingAnimation(false);
        m_TransferRoutine = StartCoroutine(PlayTransferSequenceRoutine(bankToUnit, onComplete));
    }

    public void StopAllAnimations()
    {
        StopWaitingAnimation();

        if (m_TransferRoutine != null)
        {
            StopCoroutine(m_TransferRoutine);
            m_TransferRoutine = null;
        }

        ResetPipeMaterials();
    }

    void CacheReferences()
    {
        if (m_Words == null || m_Words.Length == 0)
            m_Words = GetComponentsInChildren<MemoryWord>(true);

        if (m_AddressText == null || m_DataText == null)
        {
            var allTexts = GetComponentsInChildren<TMP_Text>(true);
            foreach (var textMesh in allTexts)
            {
                if (textMesh == null)
                    continue;

                if (m_AddressText == null && textMesh.name.IndexOf("Word", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    m_AddressText = textMesh;
                    continue;
                }

                if (m_DataText == null && textMesh.name.IndexOf("Data", StringComparison.OrdinalIgnoreCase) >= 0)
                    m_DataText = textMesh;
            }
        }

        if (m_PipeRenderers == null || m_PipeRenderers.Length == 0)
        {
            var pipeRoot = FindChildRecursive(transform, "Pipes");
            if (pipeRoot != null)
                m_PipeRenderers = pipeRoot.GetComponentsInChildren<Renderer>(true);
        }
    }

    void RebindWords()
    {
        if (m_Words == null)
            return;

        foreach (var memoryWord in m_Words)
        {
            if (memoryWord != null)
                memoryWord.SetOwningBank(this);
        }
    }

    MemoryWord GetWordByAddress(int address)
    {
        if (m_Words == null)
            return null;

        foreach (var memoryWord in m_Words)
        {
            if (memoryWord != null && memoryWord.Address == address)
                return memoryWord;
        }

        return null;
    }

    void SetHighlightedWord(MemoryWord word)
    {
        if (m_HighlightedWord == word)
            return;

        if (m_HighlightedWord != null)
            m_HighlightedWord.SetAddressHighlighted(false);

        m_HighlightedWord = word;

        if (m_HighlightedWord != null)
            m_HighlightedWord.SetAddressHighlighted(true);
    }

    void ClearHighlightedWord()
    {
        if (m_HighlightedWord != null)
            m_HighlightedWord.SetAddressHighlighted(false);

        m_HighlightedWord = null;
    }

    void ClearDisplay()
    {
        SetDisplay("Address:", "Value:");
    }

    void SetDisplay(string addressLine, string dataLine)
    {
        if (m_AddressText != null)
            m_AddressText.text = addressLine;

        if (m_DataText != null)
            m_DataText.text = dataLine;
    }

    void StartWaitingAnimation()
    {
        if (!m_IsWaitingAnimationActive)
            return;

        if (m_WaitingRoutine != null)
            return;

        m_WaitingRoutine = StartCoroutine(PlayWaitingAnimationRoutine());
    }

    void StopWaitingAnimation(bool resetMaterials = true)
    {
        if (m_WaitingRoutine != null)
        {
            StopCoroutine(m_WaitingRoutine);
            m_WaitingRoutine = null;
        }

        if (resetMaterials)
            ResetPipeMaterials();
    }

    IEnumerator PlayWaitingAnimationRoutine()
    {
        if (m_PipeRenderers == null || m_PipeRenderers.Length == 0)
        {
            m_WaitingRoutine = null;
            yield break;
        }

        ResetPipeMaterials();

        for (var index = 0; index < m_PipeRenderers.Length; index++)
        {
            ApplyPipeMaterial(m_PipeRenderers[index], m_WaitingPipeMaterial);
            yield return new WaitForSeconds(m_PipeStepDelaySeconds);

            if (!m_IsWaitingAnimationActive)
            {
                m_WaitingRoutine = null;
                yield break;
            }
        }

        m_WaitingRoutine = null;
    }

    IEnumerator PlayTransferSequenceRoutine(bool bankToUnit, Action onComplete)
    {
        if (m_IsWaitingAnimationActive)
            ApplyWaitingStateToAllPipes();
        else
            ResetPipeMaterials();

        if (m_PipeRenderers != null && m_PipeRenderers.Length > 0)
        {
            if (bankToUnit)
            {
                for (var index = m_PipeRenderers.Length - 1; index >= 0; index--)
                {
                    ApplyPipeMaterial(m_PipeRenderers[index], m_TransferPipeMaterial);
                    yield return new WaitForSeconds(m_PipeStepDelaySeconds);
                }
            }
            else
            {
                for (var index = 0; index < m_PipeRenderers.Length; index++)
                {
                    ApplyPipeMaterial(m_PipeRenderers[index], m_TransferPipeMaterial);
                    yield return new WaitForSeconds(m_PipeStepDelaySeconds);
                }
            }
        }

        m_TransferRoutine = null;

        if (!m_IsWaitingAnimationActive)
            ResetPipeMaterials();

        onComplete?.Invoke();
    }

    void ResetPipeMaterials()
    {
        if (m_PipeRenderers == null)
            return;

        foreach (var pipeRenderer in m_PipeRenderers)
            ApplyPipeMaterial(pipeRenderer, m_IdlePipeMaterial);
    }

    void ApplyWaitingStateToAllPipes()
    {
        if (m_PipeRenderers == null)
            return;

        foreach (var pipeRenderer in m_PipeRenderers)
            ApplyPipeMaterial(pipeRenderer, m_WaitingPipeMaterial);
    }

    void ApplyPipeMaterial(Renderer pipeRenderer, Material pipeMaterial)
    {
        if (pipeRenderer == null || pipeMaterial == null)
            return;

        pipeRenderer.sharedMaterial = pipeMaterial;
    }

    static Transform FindChildRecursive(Transform root, string childName)
    {
        if (root == null)
            return null;

        foreach (var childTransform in root.GetComponentsInChildren<Transform>(true))
        {
            if (childTransform != null && childTransform.name == childName)
                return childTransform;
        }

        return null;
    }

    static string FormatAddress(int value)
    {
        return $"0x{value:X8}";
    }
}

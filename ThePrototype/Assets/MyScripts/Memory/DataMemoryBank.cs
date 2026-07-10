using System;
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
    readonly DataMemoryStore m_Store = new();

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
    PipeSequencePlayer m_PipeSequencePlayer;

    MemoryWord m_HighlightedWord;
    bool m_IsWaitingAnimationActive;

    public int WordCount => m_Words != null ? m_Words.Length : 0;

    void Awake()
    {
        RebindWords();
        RebuildStore();
        m_PipeSequencePlayer?.ResetToIdle();
        ClearDisplay();
    }

    void OnEnable()
    {
        RebindWords();
        RebuildStore();
    }

    void OnValidate()
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

        if (m_PipeSequencePlayer == null)
            m_PipeSequencePlayer = GetComponentInChildren<PipeSequencePlayer>(true);
    }

    /// <summary>
    /// Tells the bank whether the Mem phase is currently active and whether the
    /// pipes should settle into their waiting state.
    /// </summary>
    public void SetPhaseState(bool isActive, bool animateWaiting)
    {
        m_IsWaitingAnimationActive = isActive && animateWaiting;

        if (!isActive)
        {
            StopWaitingAnimation(false);
            m_PipeSequencePlayer?.PlayIdleSweep();
            ClearHighlightedWord();
            return;
        }

        if (animateWaiting)
            StartWaitingAnimation();
        else
            StopWaitingAnimation();
    }

    /// <summary>
    /// Reads a word from the internal store using its byte address.
    /// </summary>
    public bool TryReadWord(int address, out int value, out MemoryWord word)
    {
        return m_Store.TryReadWord(address, out value, out word);
    }

    /// <summary>
    /// Writes a new value into the addressed word and refreshes the central display.
    /// </summary>
    public bool TryWriteWord(int address, int value, out MemoryWord word)
    {
        if (!m_Store.TryWriteWord(address, value, out word))
            return false;

        ShowWordDetails(word);
        return true;
    }

    /// <summary>
    /// Highlights the addressed word and updates the central display, if that
    /// address is currently part of the authored bank.
    /// </summary>
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

    /// <summary>
    /// Clears temporary hover or address-driven preview state.
    /// </summary>
    public void ClearPreview()
    {
        ClearHighlightedWord();
        ClearDisplay();
    }

    /// <summary>
    /// Pushes one word's address/value pair into the central bank display.
    /// </summary>
    public void ShowWordDetails(MemoryWord word)
    {
        if (word == null)
            return;

        SetDisplay($"Address: {word.AddressDisplay}", $"Value: {word.DataDisplay}");
    }

    /// <summary>
    /// Hover previews are only allowed when no explicit address packet is being previewed.
    /// </summary>
    public bool ShouldAllowHoverPreview()
    {
        return m_HighlightedWord == null;
    }

    /// <summary>
    /// Restores the explicit address preview after a hover ends, or clears the
    /// display if no preview is locked.
    /// </summary>
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

    /// <summary>
    /// Plays the one-shot success sweep between the unit and the bank.
    /// `bankToUnit=true` means a load path, while `false` means a store path.
    /// </summary>
    public void PlayTransferSequence(bool bankToUnit, Action onComplete = null)
    {
        if (m_PipeSequencePlayer == null)
        {
            onComplete?.Invoke();
            return;
        }

        m_PipeSequencePlayer.StopPlayback();

        if (m_IsWaitingAnimationActive)
            m_PipeSequencePlayer.ApplyWaitingState();
        else
            m_PipeSequencePlayer.ResetToIdle();

        m_PipeSequencePlayer.PlaySuccessSweep(
            reverse: bankToUnit,
            stepDelaySeconds: null,
            onComplete: () =>
            {
                if (!m_IsWaitingAnimationActive)
                    m_PipeSequencePlayer.ResetToIdle();

                onComplete?.Invoke();
            });
    }

    /// <summary>
    /// Stops any active pipe animation and restores the bank pipes to idle.
    /// </summary>
    public void StopAllAnimations()
    {
        StopWaitingAnimation();
        m_PipeSequencePlayer?.ResetToIdle();
    }

    /// <summary>
    /// Reattaches each authored word to this bank after scene changes or validation.
    /// </summary>
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

    /// <summary>
    /// Rebuilds the pure address lookup model from the currently authored words.
    /// </summary>
    void RebuildStore()
    {
        m_Store.Rebuild(m_Words);
    }

    MemoryWord GetWordByAddress(int address)
    {
        return m_Store.GetWordByAddress(address);
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

        m_PipeSequencePlayer?.ResetToIdle();
        m_PipeSequencePlayer?.PlayWaitingSweep();
    }

    void StopWaitingAnimation(bool resetMaterials = true)
    {
        m_PipeSequencePlayer?.StopPlayback();

        if (resetMaterials)
            m_PipeSequencePlayer?.ResetToIdle();
    }

    static string FormatAddress(int value)
    {
        return $"0x{value:X8}";
    }
}

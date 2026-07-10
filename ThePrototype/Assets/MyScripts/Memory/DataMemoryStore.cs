using System.Collections.Generic;

/// <summary>
/// Lightweight address-to-word lookup used by the Data Memory bank.
/// It keeps the actual storage rules separate from display and animation code.
/// </summary>
public sealed class DataMemoryStore
{
    readonly Dictionary<int, MemoryWord> m_WordsByAddress = new();

    /// <summary>
    /// Rebuilds the store from the currently authored memory words.
    /// </summary>
    public void Rebuild(MemoryWord[] words)
    {
        m_WordsByAddress.Clear();

        if (words == null)
            return;

        foreach (var word in words)
        {
            if (word == null)
                continue;

            m_WordsByAddress[word.Address] = word;
        }
    }

    /// <summary>
    /// Looks up a memory word by address.
    /// </summary>
    public MemoryWord GetWordByAddress(int address)
    {
        return m_WordsByAddress.TryGetValue(address, out var word) ? word : null;
    }

    /// <summary>
    /// Reads the value stored at the given address.
    /// </summary>
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

    /// <summary>
    /// Writes a new value into the word at the given address.
    /// </summary>
    public bool TryWriteWord(int address, int value, out MemoryWord word)
    {
        word = GetWordByAddress(address);
        if (word == null)
            return false;

        word.SetStoredValue(value);
        return true;
    }
}

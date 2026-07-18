/// <summary>
/// Shared bit-string cleanup so every Practice decode field compares the same
/// normalized representation, regardless of spaces or line breaks.
/// </summary>
static class PracticeDecodeBitText
{
    public static string Normalize(string rawBits)
    {
        return string.IsNullOrWhiteSpace(rawBits)
            ? string.Empty
            : rawBits.Replace(" ", string.Empty).Replace("\n", string.Empty).Replace("\r", string.Empty).Trim();
    }
}

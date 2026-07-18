sealed class PracticeDecodeInputState
{
    public string OpcodeBits { get; }
    public string RsBits { get; }
    public string RtBits { get; }
    public bool UseRd { get; }
    public string RdBits { get; }
    public bool UseImmediate { get; }
    public string ImmediateBits { get; }
    public bool UseFunct { get; }
    public string FunctBits { get; }

    public PracticeDecodeInputState(
        string opcodeBits,
        string rsBits,
        string rtBits,
        bool useRd,
        string rdBits,
        bool useImmediate,
        string immediateBits,
        bool useFunct,
        string functBits)
    {
        OpcodeBits = opcodeBits;
        RsBits = rsBits;
        RtBits = rtBits;
        UseRd = useRd;
        RdBits = rdBits;
        UseImmediate = useImmediate;
        ImmediateBits = immediateBits;
        UseFunct = useFunct;
        FunctBits = functBits;
    }
}

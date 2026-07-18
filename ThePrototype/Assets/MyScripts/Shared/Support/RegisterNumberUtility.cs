using System;

/// <summary>
/// Converts between common MIPS register identifiers and their architectural
/// register numbers so Practice mode can use the same #x convention across
/// Decode and later phases.
/// </summary>
public static class RegisterNumberUtility
{
    public static int GetRegisterNumber(string registerId)
    {
        return NormalizeRegisterId(registerId) switch
        {
            "zero" => 0,
            "at" => 1,
            "v0" => 2,
            "v1" => 3,
            "a0" => 4,
            "a1" => 5,
            "a2" => 6,
            "a3" => 7,
            "t0" => 8,
            "t1" => 9,
            "t2" => 10,
            "t3" => 11,
            "t4" => 12,
            "t5" => 13,
            "t6" => 14,
            "t7" => 15,
            "s0" => 16,
            "s1" => 17,
            "s2" => 18,
            "s3" => 19,
            "s4" => 20,
            "s5" => 21,
            "s6" => 22,
            "s7" => 23,
            "t8" => 24,
            "t9" => 25,
            "k0" => 26,
            "k1" => 27,
            "gp" => 28,
            "sp" => 29,
            "fp" => 30,
            "ra" => 31,
            _ => -1,
        };
    }

    public static string FormatRegisterNumber(string registerId)
    {
        var registerNumber = GetRegisterNumber(registerId);
        return registerNumber >= 0 ? $"#{registerNumber}" : string.Empty;
    }

    static string NormalizeRegisterId(string registerId)
    {
        return string.IsNullOrWhiteSpace(registerId)
            ? string.Empty
            : registerId.Trim().TrimStart('$').ToLowerInvariant();
    }
}

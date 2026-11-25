using System.Security.Cryptography;

namespace CamaroesPOS.Application.Services;

/// <summary>
/// Collision-proof transaction and reference number generator.
/// Format: TX-yyyyMMdd-HHmmss-XXXX (monotonic counter + random alphanumeric suffix).
/// </summary>
public static class TransactionNumberGenerator
{
    private const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
    private static int _counter;

    public static string NextTransactionNo(string prefix = "TX")
        => Generate(prefix);

    public static string NextPaymentRef(string providerPrefix)
        => Generate(providerPrefix);

    public static string NextStockRef()
        => Generate("STK");

    private static string Generate(string prefix)
    {
        int seq = Interlocked.Increment(ref _counter) & 0xFFFF;
        Span<char> suffix = stackalloc char[4];
        Span<byte> random = stackalloc byte[2];
        RandomNumberGenerator.Fill(random);

        suffix[0] = Alphabet[(seq >> 5) % Alphabet.Length];
        suffix[1] = Alphabet[seq % Alphabet.Length];
        suffix[2] = Alphabet[random[0] % Alphabet.Length];
        suffix[3] = Alphabet[random[1] % Alphabet.Length];

        return $"{prefix}-{DateTime.Now:yyyyMMdd-HHmmss}-{new string(suffix)}";
    }
}

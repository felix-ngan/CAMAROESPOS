using System.Security.Cryptography;

namespace CamaroesPOS.Application.Security;

/// <summary>
/// PBKDF2-SHA256 password hashing with per-password random salt and constant-time verification.
/// Format: v1${iterations}${saltBase64}${hashBase64}
/// </summary>
public static class PasswordHasher
{
    private const string Prefix = "v1";
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int DefaultIterations = 100_000;

    public static string Hash(string plainPassword, int iterations = DefaultIterations)
    {
        if (string.IsNullOrEmpty(plainPassword))
            throw new ArgumentException("Password cannot be empty.", nameof(plainPassword));

        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
        byte[] derived = Rfc2898DeriveBytes.Pbkdf2(
            plainPassword,
            salt,
            iterations,
            HashAlgorithmName.SHA256,
            KeySize);

        return $"{Prefix}${iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(derived)}";
    }

    public static bool IsHashed(string? stored)
        => !string.IsNullOrWhiteSpace(stored) && stored.StartsWith($"{Prefix}$", StringComparison.Ordinal);

    public static bool Verify(string? plainPassword, string? storedHash)
    {
        if (string.IsNullOrEmpty(plainPassword) || string.IsNullOrEmpty(storedHash))
            return false;

        if (!IsHashed(storedHash))
        {
            // Legacy plaintext fallback for seamless migration of existing SQLite databases
            return CryptographicOperations.FixedTimeEquals(
                System.Text.Encoding.UTF8.GetBytes(plainPassword),
                System.Text.Encoding.UTF8.GetBytes(storedHash));
        }

        var parts = storedHash.Split('$');
        if (parts.Length != 4 || parts[0] != Prefix)
            return false;

        if (!int.TryParse(parts[1], out int iterations) || iterations < 10_000)
            return false;

        try
        {
            byte[] salt = Convert.FromBase64String(parts[2]);
            byte[] expected = Convert.FromBase64String(parts[3]);
            byte[] actual = Rfc2898DeriveBytes.Pbkdf2(
                plainPassword,
                salt,
                iterations,
                HashAlgorithmName.SHA256,
                expected.Length);

            return CryptographicOperations.FixedTimeEquals(actual, expected);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}

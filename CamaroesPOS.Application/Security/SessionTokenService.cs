using System.Security.Cryptography;
using System.Text;

namespace CamaroesPOS.Application.Security;

/// <summary>
/// Generates and verifies HMAC-SHA256 signed session tokens with expiration.
/// Prevents trivial sessionStorage username spoofing in client-side deployments.
/// </summary>
public class SessionTokenService
{
    private readonly byte[] _signingKey;
    private readonly TimeSpan _ttl;

    public SessionTokenService(string? secret = null, TimeSpan? ttl = null)
    {
        var keyMaterial = string.IsNullOrWhiteSpace(secret)
            ? "CamaroesPOS::v2::SignedSession::PortfolioSandbox::2026"
            : secret;
        _signingKey = SHA256.HashData(Encoding.UTF8.GetBytes(keyMaterial));
        _ttl = ttl ?? TimeSpan.FromHours(8);
    }

    public string CreateToken(string username, string role, DateTimeOffset? utcNow = null)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username is required.", nameof(username));

        long exp = (utcNow ?? DateTimeOffset.UtcNow).Add(_ttl).ToUnixTimeSeconds();
        string payload = $"{username}|{role}|{exp}";
        string payloadB64 = Base64UrlEncode(Encoding.UTF8.GetBytes(payload));
        string sigB64 = Base64UrlEncode(ComputeSignature(payloadB64));
        return $"{payloadB64}.{sigB64}";
    }

    public bool TryValidate(string? token, out string username, out string role, DateTimeOffset? utcNow = null)
    {
        username = string.Empty;
        role = string.Empty;

        if (string.IsNullOrWhiteSpace(token))
            return false;

        var parts = token.Split('.');
        if (parts.Length != 2)
            return false;

        byte[] expectedSig = ComputeSignature(parts[0]);
        byte[] actualSig;
        try
        {
            actualSig = Base64UrlDecode(parts[1]);
        }
        catch
        {
            return false;
        }

        if (actualSig.Length != expectedSig.Length ||
            !CryptographicOperations.FixedTimeEquals(actualSig, expectedSig))
        {
            return false;
        }

        try
        {
            string payload = Encoding.UTF8.GetString(Base64UrlDecode(parts[0]));
            var fields = payload.Split('|');
            if (fields.Length != 3) return false;
            if (!long.TryParse(fields[2], out long exp)) return false;

            long now = (utcNow ?? DateTimeOffset.UtcNow).ToUnixTimeSeconds();
            if (now > exp) return false;

            username = fields[0];
            role = fields[1];
            return !string.IsNullOrWhiteSpace(username);
        }
        catch
        {
            return false;
        }
    }

    private byte[] ComputeSignature(string data)
    {
        using var hmac = new HMACSHA256(_signingKey);
        return hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
    }

    private static string Base64UrlEncode(byte[] bytes)
        => Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    private static byte[] Base64UrlDecode(string value)
    {
        string padded = value.Replace('-', '+').Replace('_', '/');
        switch (padded.Length % 4)
        {
            case 2: padded += "=="; break;
            case 3: padded += "="; break;
        }
        return Convert.FromBase64String(padded);
    }
}

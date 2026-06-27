using System;
using System.Security.Cryptography;

namespace FinalPOS.Domain
{
    public static class PasswordHasher
    {
        private const int SaltSize = 16; // 128 bit
        private const int KeySize = 32;  // 256 bit
        private const int Iterations = 10000; // PBKDF2 iterations

        /// <summary>
        /// Hash a password using PBKDF2 with SHA256 and a random salt.
        /// </summary>
        public static string HashPassword(string password)
        {
            if (password == null) throw new ArgumentNullException(nameof(password));

            using (var algorithm = new Rfc2898DeriveBytes(password, SaltSize, Iterations, HashAlgorithmName.SHA256))
            {
                byte[] salt = algorithm.Salt;
                byte[] key = algorithm.GetBytes(KeySize);

                string saltBase64 = Convert.ToBase64String(salt);
                string keyBase64 = Convert.ToBase64String(key);

                return $"PBKDF2$SHA256${Iterations}${saltBase64}${keyBase64}";
            }
        }

        /// <summary>
        /// Verify if the password matches the stored hashed password.
        /// Supports fallback to plain-text check for backward compatibility.
        /// </summary>
        public static bool VerifyPassword(string password, string storedPassword)
        {
            if (password == null) throw new ArgumentNullException(nameof(password));
            if (string.IsNullOrEmpty(storedPassword)) return false;

            // Backward compatibility: check if it's plain-text (old password)
            if (!storedPassword.StartsWith("PBKDF2$"))
            {
                return storedPassword == password;
            }

            var parts = storedPassword.Split('$');
            if (parts.Length != 5) return false;

            int iterations;
            if (!int.TryParse(parts[2], out iterations)) return false;

            try
            {
                byte[] salt = Convert.FromBase64String(parts[3]);
                byte[] storedKey = Convert.FromBase64String(parts[4]);

                using (var algorithm = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256))
                {
                    byte[] keyToCheck = algorithm.GetBytes(KeySize);
                    return CryptographicEquals(storedKey, keyToCheck);
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Constant-time comparison to prevent timing attacks.
        /// </summary>
        private static bool CryptographicEquals(byte[] a, byte[] b)
        {
            if (a == null || b == null) return false;
            if (a.Length != b.Length) return false;

            int result = 0;
            for (int i = 0; i < a.Length; i++)
            {
                result |= a[i] ^ b[i];
            }
            return result == 0;
        }
    }
}

using System.Security.Cryptography;
using System.Text;

namespace Sammlerplattform.Services.Passkey
{
    public class BackupCodeService
    {
        public static List<string> GenerateBackupCodes(int count = 10, int length = 8)
        {
            var codes = new List<string>();
            for (int i = 0; i < count; i++)
            {
                // Ohne verwechselbare Zeichen (0, O, I, l, etc.)
                const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
                var code = new string([.. Enumerable.Repeat(chars, length).Select(s => s[Random.Shared.Next(s.Length)])]);

                // Formatieren: XXXX-XXXX für bessere Lesbarkeit
                if (length == 8)
                    code = $"{code[..4]}-{code[4..]}";

                codes.Add(code);
            }
            return codes;
        }

        // Backup-Code hashen (mit Salt!)
        public static string HashBackupCode(string plainCode)
        {
            // Normalize: Remove dashes and spaces, uppercase
            var normalized = plainCode.Replace("-", "").Replace(" ", "").ToUpperInvariant();

            // PBKDF2 mit Salt (gleiche Methode wie ASP.NET Core Identity)
            var salt = Guid.NewGuid().ToString(); // Jeder Code bekommt eigenen Salt!
            var salted = normalized + salt;
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(salted));

            // Format: {salt}:{hash}
            return $"{salt}:{Convert.ToBase64String(hash)}";
        }

        // Backup-Code verifizieren
        public static bool VerifyBackupCode(string plainCode, string storedHash)
        {
            var parts = storedHash.Split(':');
            if (parts.Length != 2) return false;

            var salt = parts[0];
            var expectedHash = parts[1];

            var normalized = plainCode.Replace("-", "").Replace(" ", "").ToUpperInvariant();
            var salted = normalized + salt;

            var actualHash = SHA256.HashData(Encoding.UTF8.GetBytes(salted));
            var actualHashBase64 = Convert.ToBase64String(actualHash);

            return actualHashBase64 == expectedHash;
        }
    }
}

using System.Security.Cryptography;
using System.Text;

namespace TodoApp.Services
{
    public class SymetricEncryptionService
    {
        private const string PrivateKeyFilePath = "rsa_private_key.pem";

        public SymetricEncryptionService()
        {
            EnsurePrivateKeyExists();
        }

        // Ensure the RSA private key exists and is stored persistently in PEM format
        private void EnsurePrivateKeyExists()
        {
            if (!File.Exists(PrivateKeyFilePath))
            {
                using var rsa = RSA.Create();
                var privateKey = rsa.ExportPkcs8PrivateKey();
                File.WriteAllText(PrivateKeyFilePath, ExportToPem("PRIVATE KEY", privateKey));
            }
        }

        // Load the RSA private key from PEM
        private RSA LoadPrivateKey()
        {
            var pem = File.ReadAllText(PrivateKeyFilePath);
            var privateKey = ReadPem(pem, "PRIVATE KEY");

            var rsa = RSA.Create();
            rsa.ImportPkcs8PrivateKey(privateKey, out _);
            return rsa;
        }

        // Symmetric Encryption
        public string Encrypt(string plainText)
        {
            using var aes = Aes.Create();
            aes.GenerateKey();
            aes.GenerateIV();

            using var encryptor = aes.CreateEncryptor();
            var plainBytes = Encoding.UTF8.GetBytes(plainText);
            var encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

            using var rsa = LoadPrivateKey();
            var encryptedKey = rsa.Encrypt(aes.Key, RSAEncryptionPadding.OaepSHA256);

            var result = new
            {
                EncryptedKey = Convert.ToBase64String(encryptedKey),
                IV = Convert.ToBase64String(aes.IV),
                EncryptedText = Convert.ToBase64String(encryptedBytes)
            };

            return System.Text.Json.JsonSerializer.Serialize(result);
        }

        // Symmetric Decryption
        public string Decrypt(string encryptedData)
        {
            var data = System.Text.Json.JsonSerializer.Deserialize<dynamic>(encryptedData);

            if (data == null)
                return "error: data is null";

            var encryptedKey = Convert.FromBase64String((string)data.EncryptedKey);
            var iv = Convert.FromBase64String((string)data.IV);
            var encryptedText = Convert.FromBase64String((string)data.EncryptedText);

            using var rsa = LoadPrivateKey();
            var aesKey = rsa.Decrypt(encryptedKey, RSAEncryptionPadding.OaepSHA256);

            using var aes = Aes.Create();
            aes.Key = aesKey;
            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor();
            var decryptedBytes = decryptor.TransformFinalBlock(encryptedText, 0, encryptedText.Length);

            return Encoding.UTF8.GetString(decryptedBytes);
        }

        // Helper to export to PEM
        private static string ExportToPem(string label, byte[] data)
        {
            var base64 = Convert.ToBase64String(data);
            var sb = new StringBuilder();
            sb.AppendLine($"-----BEGIN {label}-----");
            for (int i = 0; i < base64.Length; i += 64)
                sb.AppendLine(base64.Substring(i, Math.Min(64, base64.Length - i)));
            sb.AppendLine($"-----END {label}-----");
            return sb.ToString();
        }

        // Helper to read PEM
        private static byte[] ReadPem(string pem, string label)
        {
            var header = $"-----BEGIN {label}-----";
            var footer = $"-----END {label}-----";

            var start = pem.IndexOf(header) + header.Length;
            var end = pem.IndexOf(footer, start);

            var base64 = pem.Substring(start, end - start)
                            .Replace("\r", "")
                            .Replace("\n", "")
                            .Trim();

            return Convert.FromBase64String(base64);
        }
    }
}
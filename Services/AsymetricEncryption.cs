using System.Security.Cryptography;
using System.Text;

namespace TodoApp.Services
{
    public class AsymetricEncryption
    {
        private const string PrivateKeyPath = "asymmetric_private_key.pem";
        private const string PublicKeyPath = "asymmetric_public_key.pem";

        private readonly byte[] _privateKey;
        private readonly byte[] _publicKey;

        public AsymetricEncryption()
        {
            if (!File.Exists(PrivateKeyPath) || !File.Exists(PublicKeyPath))
            {
                using var rsa = RSA.Create();

                var privateKeyBytes = rsa.ExportPkcs8PrivateKey(); // PKCS#8
                var publicKeyBytes = rsa.ExportSubjectPublicKeyInfo(); // X.509

                File.WriteAllText(PrivateKeyPath, ExportToPem("PRIVATE KEY", privateKeyBytes));
                File.WriteAllText(PublicKeyPath, ExportToPem("PUBLIC KEY", publicKeyBytes));
            }

            _privateKey = ReadPem(PrivateKeyPath, "PRIVATE KEY");
            _publicKey = ReadPem(PublicKeyPath, "PUBLIC KEY");
        }

        public byte[] PublicKey => _publicKey;

        public string Encrypt(string plainText)
        {
            using var rsa = RSA.Create();
            rsa.ImportSubjectPublicKeyInfo(_publicKey, out _);

            var plainBytes = Encoding.UTF8.GetBytes(plainText);
            var encryptedBytes = rsa.Encrypt(plainBytes, RSAEncryptionPadding.OaepSHA256);

            return Convert.ToBase64String(encryptedBytes);
        }

        public string Decrypt(string encryptedText)
        {
            using var rsa = RSA.Create();
            rsa.ImportPkcs8PrivateKey(_privateKey, out _);

            var encryptedBytes = Convert.FromBase64String(encryptedText);
            var decryptedBytes = rsa.Decrypt(encryptedBytes, RSAEncryptionPadding.OaepSHA256);

            return Encoding.UTF8.GetString(decryptedBytes);
        }

        // Helper: Write bytes to PEM format
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

        // Helper: Read bytes from PEM file
        private static byte[] ReadPem(string filePath, string label)
        {
            var pem = File.ReadAllText(filePath);
            var header = $"-----BEGIN {label}-----";
            var footer = $"-----END {label}-----";

            var start = pem.IndexOf(header, StringComparison.Ordinal) + header.Length;
            var end = pem.IndexOf(footer, start, StringComparison.Ordinal);

            var base64 = pem.Substring(start, end - start).Replace("\n", "").Replace("\r", "").Trim();
            return Convert.FromBase64String(base64);
        }
    }
}
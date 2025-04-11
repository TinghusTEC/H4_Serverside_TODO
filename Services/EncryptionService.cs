using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using BCrypt.Net;

namespace TodoApp.Services
{
    public class EncryptionService
    {
        private readonly HttpClient _httpClient;
        private readonly AsymetricEncryption _asymetricEncryption;

        public EncryptionService(HttpClient httpClient, AsymetricEncryption asymetricEncryption)
        {
            _httpClient = httpClient;
            _asymetricEncryption = asymetricEncryption;
            
            
            Console.WriteLine($"EncryptionService is using BaseAddress: {_httpClient.BaseAddress}");
        }

        public async Task<string> EncryptViaWebApiAsync(string plainText)
        {
            var publicKey = Convert.ToBase64String(_asymetricEncryption.PublicKey);

            var request = new
            {
                PublicKey = publicKey,
                Text = plainText
            };

            var response = await _httpClient.PostAsJsonAsync("api/encryption/encrypt", request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        public string Decrypt(string encryptedText)
        {
            return _asymetricEncryption.Decrypt(encryptedText);
        }

        public string HashWithSHA2(string input)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(input);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        public bool VerifySHA2Hash(string input, string hashedValue)
        {
            var computedHash = HashWithSHA2(input);
            return computedHash == hashedValue;
        }

        public string HashWithHMAC(string input, string key)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
            var bytes = Encoding.UTF8.GetBytes(input);
            var hash = hmac.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        public bool VerifyHMACHash(string input, string key, string hashedValue)
        {
            var computedHash = HashWithHMAC(input, key);
            return computedHash == hashedValue;
        }

        public string HashWithPBKDF2(string input, string salt = "1234", int iterations = 10000, int hashLength = 32)
        {
            var bytes = Encoding.UTF8.GetBytes(input);
            var saltBytes = Encoding.UTF8.GetBytes(salt);
            using var pbkdf2 = new Rfc2898DeriveBytes(bytes, saltBytes, iterations, HashAlgorithmName.SHA256);
            return Convert.ToBase64String(pbkdf2.GetBytes(hashLength));
        }

        public bool VerifyPBKDF2Hash(string input, string salt, string hashedValue, int iterations = 10000, int hashLength = 32)
        {
            var computedHash = HashWithPBKDF2(input, salt, iterations, hashLength);
            return computedHash == hashedValue;
        }

        public string HashWithBCrypt(string input)
        {
            var workFactor = 12;
            var salt = BCrypt.Net.BCrypt.GenerateSalt(workFactor);
            return BCrypt.Net.BCrypt.HashPassword(input, salt);
        }

        public bool VerifyBCryptHash(string input, string hashedValue)
        {
            return BCrypt.Net.BCrypt.Verify(input, hashedValue);
        }
    }
}
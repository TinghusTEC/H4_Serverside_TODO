using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;

namespace TodoApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EncryptionController : ControllerBase
    {
        [HttpPost("encrypt")]
        public IActionResult Encrypt([FromBody] EncryptionRequest request)
        {
            try
            {
                var publicKeyBytes = Convert.FromBase64String(request.PublicKey);
                using var rsa = RSA.Create();
                rsa.ImportSubjectPublicKeyInfo(publicKeyBytes, out _);

                var plainBytes = Encoding.UTF8.GetBytes(request.Text);
                var encryptedBytes = rsa.Encrypt(plainBytes, RSAEncryptionPadding.OaepSHA256);

                return Ok(Convert.ToBase64String(encryptedBytes));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }

    public class EncryptionRequest
    {
        public string PublicKey { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
    }
}
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace OP.PORTAL.Helpers
{
    public class AesEncryptionHelper
    {
        private readonly byte[] _key;
        private readonly byte[] _iv;

        public AesEncryptionHelper(string key, string iv)
        {
            _key = Convert.FromBase64String(key);   // 32 bytes
            _iv = Convert.FromBase64String(iv);    // 16 bytes
        }

        public string Encrypt(string plainText)
        {
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = _iv;

            using var encryptor = aes.CreateEncryptor();
            var bytes = Encoding.UTF8.GetBytes(plainText);
            var encrypted = encryptor.TransformFinalBlock(bytes, 0, bytes.Length);

            return Convert.ToBase64String(encrypted);
        }

        public string Decrypt(string cipherText)
        {
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = _iv;

            using var decryptor = aes.CreateDecryptor();
            var bytes = Convert.FromBase64String(cipherText);
            var decrypted = decryptor.TransformFinalBlock(bytes, 0, bytes.Length);

            return Encoding.UTF8.GetString(decrypted);
        }

        public string EncryptUrl(string plainText)
        {
            return WebUtility.UrlEncode(this.Encrypt(plainText));
        }

        public string DecryptUrl(string cipherText)
        {
            return WebUtility.UrlDecode(this.Decrypt(cipherText));
        }
    }
}

using System.Security.Cryptography;
using System.Text;

namespace ProcessManagement.Services
{
    /// <summary>
    /// Service để encrypt/decrypt password (chỉ dùng cho môi trường nội bộ)
    /// </summary>
    public class PasswordEncryptionService
    {
        private readonly string _encryptionKey;
        private const int KeySize = 256;
        private const int BlockSize = 128;

        public PasswordEncryptionService(IConfiguration configuration)
        {
            // Lấy key từ configuration, nếu không có thì dùng key mặc định (KHÔNG AN TOÀN!)
            _encryptionKey = configuration["PasswordEncryption:Key"] ?? "YourSecretKey1234567890123456789012"; // 32 bytes key
            
            if (_encryptionKey.Length < 32)
            {
                // Pad key nếu ngắn hơn 32 bytes
                _encryptionKey = _encryptionKey.PadRight(32, '0');
            }
            else if (_encryptionKey.Length > 32)
            {
                _encryptionKey = _encryptionKey.Substring(0, 32);
            }
        }

        public string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return string.Empty;

            using (var aes = Aes.Create())
            {
                aes.KeySize = KeySize;
                aes.BlockSize = BlockSize;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.Key = Encoding.UTF8.GetBytes(_encryptionKey);
                aes.GenerateIV();

                using (var encryptor = aes.CreateEncryptor())
                using (var ms = new MemoryStream())
                {
                    ms.Write(aes.IV, 0, aes.IV.Length);
                    
                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    using (var sw = new StreamWriter(cs))
                    {
                        sw.Write(plainText);
                    }

                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        public string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
                return string.Empty;

            try
            {
                var fullCipher = Convert.FromBase64String(cipherText);

                using (var aes = Aes.Create())
                {
                    aes.KeySize = KeySize;
                    aes.BlockSize = BlockSize;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;
                    aes.Key = Encoding.UTF8.GetBytes(_encryptionKey);

                    var iv = new byte[16];
                    Array.Copy(fullCipher, 0, iv, 0, iv.Length);
                    aes.IV = iv;

                    using (var decryptor = aes.CreateDecryptor())
                    using (var ms = new MemoryStream(fullCipher, iv.Length, fullCipher.Length - iv.Length))
                    using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    using (var sr = new StreamReader(cs))
                    {
                        return sr.ReadToEnd();
                    }
                }
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}



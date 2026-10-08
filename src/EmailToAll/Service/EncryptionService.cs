using EmailToAll.Data.Interface;
using System.Security.Cryptography;
using System.Text;

namespace EmailToAll.Service
{
    public class EncryptionService : IEncryptionRepository
    {
        private readonly byte[] Key;

        private readonly byte[] IV;

        public EncryptionService(IConfiguration configuration)
        {
            Key = Encoding.UTF8.GetBytes(configuration["Encryption:Key"] ?? string.Empty);
            IV = Encoding.UTF8.GetBytes(configuration["Encryption:IV"] ?? string.Empty);

            if (Key.Length != 32)
                throw new InvalidOperationException("Encryption:Key must be exactly 32 characters (AES-256).");

            if (IV.Length != 16)
                throw new InvalidOperationException("Encryption:IV must be exactly 16 characters.");
        }

        public string Encrypt(string plainText)
        {
            using Aes aes = Aes.Create();

            aes.Key = Key;
            aes.IV = IV;

            using ICryptoTransform encryptor = aes.CreateEncryptor();

            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);

            byte[] encryptedBytes = encryptor.TransformFinalBlock(
                plainBytes,
                0,
                plainBytes.Length
            );

            return Convert.ToBase64String(encryptedBytes);
        }

        public string Decrypt(string cipherText)
        {
            if (string.IsNullOrWhiteSpace(cipherText))
                throw new ArgumentException("Cipher text is empty.");

            using Aes aes = Aes.Create();

            aes.Key = Key;
            aes.IV = IV;

            using ICryptoTransform decryptor = aes.CreateDecryptor();

            byte[] encryptedBytes = Convert.FromBase64String(cipherText);

            byte[] decryptedBytes = decryptor.TransformFinalBlock(
                encryptedBytes,
                0,
                encryptedBytes.Length
            );

            return Encoding.UTF8.GetString(decryptedBytes);
        }

    }
}

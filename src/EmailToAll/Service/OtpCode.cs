using System.Security.Cryptography;

namespace EmailToAll.Service
{
    public static class OtpCode
    {
        public const int ValidMinutes = 5;

        /// <summary>Generates a cryptographically random 6-digit code.</summary>
        public static string Generate()
        {
            return RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
        }
    }
}

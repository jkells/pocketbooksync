using System;
using System.Security.Cryptography;
using System.Text;

namespace PocketBookSync.Data
{
    public static class Crypto
    {
        private static readonly byte[] AdditionalEntropy =
        {
            0xe7, 0x4f, 0xf1, 0x36, 0x3f, 0x91, 0xae, 0x57, 0x1d, 0xf3, 0x74,
            0x12, 0x1c, 0xdf, 0xe1, 0x38, 0x1b, 0xe6, 0x13, 0x1d, 0x38, 0x86, 0x61, 0xf9, 0x17, 0x2b, 0xb2, 0x2a, 0x48,
            0x80, 0x1a, 0x24
        };

        public static string Encrypt(string value)
        {
            var protectedData = ProtectedData.Protect(Encoding.UTF8.GetBytes(value), AdditionalEntropy,
                DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(protectedData);
        }

        public static string Decrypt(string value)
        {
            var protectedData = Convert.FromBase64String(value);
            var unprotectedData = ProtectedData.Unprotect(protectedData, AdditionalEntropy,
                DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(unprotectedData);
        }
    }
}
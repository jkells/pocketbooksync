using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace PocketBookSync.Data
{
    public class Config
    {
        private readonly byte[] _additionalEntropy =
        {
            0xe7, 0x4f, 0xf1, 0x36, 0x3f, 0x91, 0xae, 0x57, 0x1d, 0xf3, 0x74,
            0x12, 0x1c, 0xdf, 0xe1, 0x38, 0x1b, 0xe6, 0x13, 0x1d, 0x38, 0x86, 0x61, 0xf9, 0x17, 0x2b, 0xb2, 0x2a, 0x48,
            0x80, 0x1a, 0x24
        };

        [Key]
        public int Id { get; set; } = 1;

        public string Data { get; set; }

        [NotMapped]
        public IEnumerable<Account> Accounts
        {
            get
            {
                if (Data == null)
                    return new List<Account>();

                return JsonConvert.DeserializeObject<IEnumerable<Account>>(Decrypt(Data));
            }
            set { Data = Encrypt(JsonConvert.SerializeObject(value)); }
        }

        private string Encrypt(string value)
        {
            var protectedData = ProtectedData.Protect(Encoding.UTF8.GetBytes(value), _additionalEntropy,
                DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(protectedData);
        }

        private string Decrypt(string value)
        {
            var protectedData = Convert.FromBase64String(value);
            var unprotectedData = ProtectedData.Unprotect(protectedData, _additionalEntropy,
                DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(unprotectedData);
        }
    }
}
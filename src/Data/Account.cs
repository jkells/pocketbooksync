using System.ComponentModel.DataAnnotations.Schema;

namespace PocketBookSync.Data
{
    public class Account
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public string XUsername { get; set; }
        public string XPassword { get; set; }
        public string PocketBookAccountNumber { get; set; }
        public string AccountReference { get; set; }

        [NotMapped]
        public string Username
        {
            get { return XUsername == null ? null : Crypto.Decrypt(XUsername); }
            set { XUsername = Crypto.Encrypt(value); }
        }

        [NotMapped]
        public string Password
        {
            get { return XPassword == null ? null : Crypto.Decrypt(XPassword); }
            set { XPassword = Crypto.Encrypt(value); }
        }
    }
}
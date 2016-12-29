using System.ComponentModel.DataAnnotations.Schema;

namespace PocketBookSync.Data
{
    public class Config
    {
        public int Id { get; set; }
        public string XPocketBookUsername { get; set; }
        public string XPocketBookPassword { get; set; }

        [NotMapped]
        public string PocketBookUsername
        {
            get { return XPocketBookUsername == null ? null : Crypto.Decrypt(XPocketBookUsername); }
            set { XPocketBookUsername = Crypto.Encrypt(value); }
        }

        [NotMapped]
        public string PocketBookPassword
        {
            get { return XPocketBookPassword == null ? null : Crypto.Decrypt(XPocketBookPassword); }
            set { XPocketBookPassword = Crypto.Encrypt(value); }
        }
    }
}
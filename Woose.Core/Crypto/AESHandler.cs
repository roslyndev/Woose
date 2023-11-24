using System.Security.Cryptography;
using System.Text;
using System;

namespace Woose.Core
{
    public class AESHandler
    {
        protected string PublicKey { get; set; } = string.Empty;

        protected string PrivateKey { get; set; } = string.Empty;

        public AESHandler() 
        { 
        }

        public void Set(string publickey, string privatekey)
        {
            this.PublicKey = publickey;
            this.PrivateKey = privatekey;
        }

        public string Decrypt(string keyString)
        {
            string result = string.Empty;

            using (RijndaelManaged Aes = new RijndaelManaged())
            {
                Aes.Mode = CipherMode.CBC;
                Aes.KeySize = 128;
                Aes.BlockSize = 128;

                byte[] encryptedData = Convert.FromBase64String(CryptoHelper.SaltRemove(keyString.Trim()));
                Aes.Key = Encoding.ASCII.GetBytes(this.PublicKey);
                Aes.IV = Encoding.ASCII.GetBytes(this.PrivateKey);

                ICryptoTransform transform = Aes.CreateDecryptor();
                byte[] plainText = transform.TransformFinalBlock(encryptedData, 0, encryptedData.Length);

                result = Encoding.UTF8.GetString(plainText);
            }

            return result;
        }

        public string Encrypt(string keyString)
        {
            string result = string.Empty;

            using (RijndaelManaged Aes = new RijndaelManaged())
            {
                Aes.Mode = CipherMode.CBC;
                Aes.KeySize = 128;
                Aes.BlockSize = 128;
                Aes.Key = Encoding.ASCII.GetBytes(this.PublicKey);
                Aes.IV = Encoding.ASCII.GetBytes(this.PrivateKey);

                ICryptoTransform transform = Aes.CreateEncryptor();
                byte[] plainText = Encoding.UTF8.GetBytes(keyString);
                byte[] cipherBytes = transform.TransformFinalBlock(plainText, 0, plainText.Length);

                result = CryptoHelper.SaltAdd(Convert.ToBase64String(cipherBytes));
            }

            return result;
        }
    }
}

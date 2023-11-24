using System;
using System.Text;

namespace Woose.Core
{
    public class CryptoHelper
    {
        private static readonly Lazy<AESHandler> aes = new Lazy<AESHandler>(() => new AESHandler());
        public static AESHandler AES { get { return aes.Value; } }

        private static readonly Lazy<SHA256Handler> sha256 = new Lazy<SHA256Handler>(() => new SHA256Handler());
        public static SHA256Handler SHA256 { get { return sha256.Value; } }

        private static readonly Lazy<SHA512Handler> sha512 = new Lazy<SHA512Handler>(() => new SHA512Handler());
        public static SHA512Handler SHA512 { get { return sha512.Value; } }

        private static readonly Lazy<AES128Handler> aes128 = new Lazy<AES128Handler>(() => new AES128Handler());
        public static AES128Handler AES128 { get { return aes128.Value; } }

        private static readonly Lazy<AES256Handler> aes256 = new Lazy<AES256Handler>(() => new AES256Handler());
        public static AES256Handler AES256 { get { return aes256.Value; } }

        private static readonly Lazy<Base64Handler> base64 = new Lazy<Base64Handler>(() => new Base64Handler());
        public static Base64Handler BASE64 { get { return base64.Value; } }

        public CryptoHelper()
        {
        }

        public static string SaltAdd(string targetString)
        {
            StringBuilder builder = new StringBuilder(targetString);
            builder.Replace("=", "EvSxrTzQ");
            builder.Replace("+", "PDkcVjeDL");
            builder.Replace("/", "SkenFkkd");
            return builder.ToString();
        }

        public static string SaltRemove(string targetString)
        {
            StringBuilder builder = new StringBuilder(targetString);
            builder.Replace("EvSxrTzQ", "=");
            builder.Replace("PDkcVjeDL", "+");
            builder.Replace("SkenFkkd", "/");
            return builder.ToString();
        }
    }
}

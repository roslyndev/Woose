﻿using System;
using System.Text;

namespace Woose.Core
{
    public class Base64Handler
    {
        public string Decrypt(string keyString)
        {
            if (!String.IsNullOrWhiteSpace(keyString))
            {
                UTF8Encoding encoder = new UTF8Encoding();
                Decoder utf8Decode = encoder.GetDecoder();
                string str = keyString.Trim();
                byte[] todecode_byte = Convert.FromBase64String(str);
                int charCount = utf8Decode.GetCharCount(todecode_byte, 0, todecode_byte.Length);
                char[] decoded_char = new char[charCount];
                utf8Decode.GetChars(todecode_byte, 0, todecode_byte.Length, decoded_char, 0);
                return new String(decoded_char);
            }
            else
            {
                return String.Empty;
            }
        }

        public string Encrypt(string keyString)
        {
            if (!String.IsNullOrWhiteSpace(keyString))
            {
                byte[] strByte = Encoding.UTF8.GetBytes(keyString.Trim());
                return Convert.ToBase64String(strByte);
            }
            else
            {
                return String.Empty;
            }
        }

        public string SaltDecrypt(string keyString)
        {
            if (!String.IsNullOrWhiteSpace(keyString))
            {
                UTF8Encoding encoder = new UTF8Encoding();
                Decoder utf8Decode = encoder.GetDecoder();
                string str = CryptoHelper.SaltRemove(keyString.Trim());
                byte[] todecode_byte = Convert.FromBase64String(str);
                int charCount = utf8Decode.GetCharCount(todecode_byte, 0, todecode_byte.Length);
                char[] decoded_char = new char[charCount];
                utf8Decode.GetChars(todecode_byte, 0, todecode_byte.Length, decoded_char, 0);
                return new String(decoded_char);
            }
            else
            {
                return String.Empty;
            }
        }

        public string SaltEncrypt(string keyString)
        {
            if (!String.IsNullOrWhiteSpace(keyString))
            {
                byte[] strByte = Encoding.UTF8.GetBytes(keyString.Trim());
                return CryptoHelper.SaltAdd(Convert.ToBase64String(strByte));
            }
            else
            {
                return String.Empty;
            }
        }
    }
}

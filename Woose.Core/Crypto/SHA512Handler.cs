﻿using System.Security.Cryptography;
using System;

namespace Woose.Core
{
    public class SHA512Handler
    {
        private const int PBKDF2_ITERATIONS = 1000;

        public SHA512Handler()
        {
        }

        public string Encrypt(string keyString)
        {
            string result = String.Empty;

            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] salt = new byte[24];
                rng.GetBytes(salt);
                byte[] hash = PBKDF2(keyString, salt, PBKDF2_ITERATIONS, 64); // SHA-512 has 64 bytes output
                result = String.Format("{0}:{1}:{2}", PBKDF2_ITERATIONS, Convert.ToBase64String(salt), Convert.ToBase64String(hash));
            }

            return result;
        }

        public bool ValidateCheck(string targetHash, string keyString)
        {
            char[] delimiter = { ':' };
            string[] split = targetHash.Split(delimiter);
            int iterations = Int32.Parse(split[0]);
            byte[] salt = Convert.FromBase64String(split[1]);
            byte[] hash = Convert.FromBase64String(split[2]);
            byte[] testHash = PBKDF2(keyString, salt, iterations, hash.Length);
            return SlowEquals(hash, testHash);
        }

        private byte[] PBKDF2(string password, byte[] salt, int iterations, int outputBytes)
        {
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA512))
            {
                return pbkdf2.GetBytes(outputBytes);
            }
        }

        private bool SlowEquals(byte[] a, byte[] b)
        {
            uint diff = (uint)a.Length ^ (uint)b.Length;
            for (int i = 0; i < a.Length && i < b.Length; i++)
                diff |= (uint)(a[i] ^ b[i]);
            return diff == 0;
        }
    }
}

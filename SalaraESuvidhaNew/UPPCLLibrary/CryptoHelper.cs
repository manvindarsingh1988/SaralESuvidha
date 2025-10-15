using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;

namespace UPPCLLibrary
{
    public class CryptoHelper
    {
        public static string Encrypt(string data, string secretKey)
        {
            byte[] plaintext = Encoding.UTF8.GetBytes(data);
            byte[] password = Encoding.UTF8.GetBytes(secretKey);
            byte[] salt = new byte[8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            const int keySize = 32;
            const int ivSize = 16;
            byte[] derived = new byte[keySize + ivSize];
            using (var md5 = MD5.Create())
            {
                byte[] previous = Array.Empty<byte>();
                int currentPos = 0;
                while (currentPos < derived.Length)
                {
                    byte[] hashInput = new byte[previous.Length + password.Length + salt.Length];
                    if (previous.Length > 0)
                    {
                        previous.CopyTo(hashInput, 0);
                    }
                    password.CopyTo(hashInput, previous.Length);
                    salt.CopyTo(hashInput, previous.Length + password.Length);
                    byte[] hash = md5.ComputeHash(hashInput);
                    Array.Copy(hash, 0, derived, currentPos, Math.Min(hash.Length, derived.Length - currentPos));
                    currentPos += hash.Length;
                    previous = hash;
                }
            }

            byte[] key = new byte[keySize];
            byte[] iv = new byte[ivSize];
            Array.Copy(derived, 0, key, 0, keySize);
            Array.Copy(derived, keySize, iv, 0, ivSize);

            byte[] ciphertext;
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                using (var encryptor = aes.CreateEncryptor())
                using (var ms = new MemoryStream())
                using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                {
                    cs.Write(plaintext, 0, plaintext.Length);
                    cs.FlushFinalBlock();
                    ciphertext = ms.ToArray();
                }
            }

            byte[] saltedPrefix = Encoding.ASCII.GetBytes("Salted__");
            byte[] formatted = new byte[saltedPrefix.Length + salt.Length + ciphertext.Length];
            saltedPrefix.CopyTo(formatted, 0);
            salt.CopyTo(formatted, saltedPrefix.Length);
            ciphertext.CopyTo(formatted, saltedPrefix.Length + salt.Length);

            string base64 = Convert.ToBase64String(formatted);
            string encoded = Uri.EscapeDataString(base64);
            return encoded;
        }
    }
}

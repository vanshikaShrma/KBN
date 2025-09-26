using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
namespace KBN.Services
{
   
    public class AesEncryption:IAesEncryption
    {
        // Use a fixed key/IV in production securely from config (never hardcode!)
        private static readonly byte[] Key = Encoding.UTF8.GetBytes("qwertyuiopasdfghqwertyuiopasdfgh"); // Must be 16/24/32 bytes
        private static readonly byte[] IV = Encoding.UTF8.GetBytes("1234567890abcdef");        // Must be 16 bytes

        public  string Encrypt(string plainText)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = Key;
                aes.IV = IV;

                using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using var ms = new MemoryStream();
                using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                using (var sw = new StreamWriter(cs))
                {
                    sw.Write(plainText);
                }
                return Convert.ToBase64String(ms.ToArray());
            }
        }

        public string Decrypt(string cipherText)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = Key;
                aes.IV = IV;

                using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                using var ms = new MemoryStream(Convert.FromBase64String(cipherText));
                using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
                using var sr = new StreamReader(cs);
                return sr.ReadToEnd();
            }
        }
    }

}

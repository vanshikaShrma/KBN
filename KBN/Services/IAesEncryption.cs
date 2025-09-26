namespace KBN.Services
{
    public interface IAesEncryption
    {
        public  string Encrypt(string plainText);
        public string Decrypt(string cipherText);
    }
}

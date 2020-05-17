using System;
using System.Security.Cryptography;
using System.Text;

namespace Chat.Shared
{
    public class SecurityService
    {
        public static (string publicKey, string privateKey) GenerateKeys()
        {
            var rsa = new RSACryptoServiceProvider();
            var publicKey = rsa.ToXmlString(false); // false to get the public key   
            var privateKey = rsa.ToXmlString(true); // true to get the private key   

            return (publicKey, privateKey);
        }

        public static string EncryptText(string publicKey, string text)
        {
            // Convert the text to an array of bytes   
            var byteConverter = new UnicodeEncoding();
            var dataToEncrypt = byteConverter.GetBytes(text);

            // Create a byte array to store the encrypted data in it   
            using var rsa = new RSACryptoServiceProvider();
            // Set the rsa pulic key   
            rsa.FromXmlString(publicKey);

            // Encrypt the data and store it in the encyptedData Array   
            var encryptedData = rsa.Encrypt(dataToEncrypt, false);

            return Convert.ToBase64String(encryptedData);
        }

        // Method to decrypt the data withing a specific file using a RSA algorithm private key   
        public static string DecryptData(string privateKey, string data)
        {
            // Create an array to store the decrypted data in it   
            using var rsa = new RSACryptoServiceProvider();
            // Set the private key of the algorithm   
            rsa.FromXmlString(privateKey);
            var decryptedData = rsa.Decrypt(Convert.FromBase64String(data), false);

            // Get the string value from the decryptedData byte array   
            var byteConverter = new UnicodeEncoding();
            return byteConverter.GetString(decryptedData);
        }
    }
}
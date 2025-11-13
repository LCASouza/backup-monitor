using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace BackupMonitor.Services;

public class CriptografiaService
{
    private byte[] GetKey(string password)
    {
        using (var sha256 = SHA256.Create())
        {
            return sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        }
    }

    public void EncryptFile(string inputPath, string outputPath, string password)
    {
        using (var aes = Aes.Create())
        {
            aes.Key = GetKey(password);
            aes.GenerateIV();

            using (var inputStream = File.OpenRead(inputPath))
            using (var outputStream = File.Create(outputPath))
            {
                outputStream.Write(aes.IV, 0, aes.IV.Length);

                using (var cryptoStream = new CryptoStream(outputStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    inputStream.CopyTo(cryptoStream);
                }
            }
        }
    }

    public void DecryptFile(string encryptedPath, string outputPath, string password)
    {
        using (var aes = Aes.Create())
        using (var inputStream = File.OpenRead(encryptedPath))
        {
            byte[] iv = new byte[16];
            inputStream.ReadExactly(iv);

            aes.Key = GetKey(password);
            aes.IV = iv;

            using (var cryptoStream = new CryptoStream(inputStream, aes.CreateDecryptor(), CryptoStreamMode.Read))
            using (var outputStream = File.Create(outputPath))
            {
                cryptoStream.CopyTo(outputStream);
            }
        }
    }
}
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace BackupMonitor.Services;

public class CriptografiaService
{
    private byte[] GetKey(string password)
    {
        using (var deriveBytes = new Rfc2898DeriveBytes(password, salt: new byte[8], 1000))
        {
            return deriveBytes.GetBytes(32); // 256 bits para AES
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
            inputStream.Read(iv, 0, iv.Length);

            aes.Key = GetKey(password);
            aes.IV = iv;

            using (var cryptoStream = new CryptoStream(inputStream, aes.CreateDecryptor(), CryptoStreamMode.Read))
            using (var outputStream = File.Create(outputPath))
            {
                cryptoStream.CopyTo(outputStream);
            }
        }
    }

    // Para manipular strings diretamente (opcional)
    public string EncryptString(string plainText, string password)
    {
        string tempFile = Path.GetTempFileName();
        string encFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, plainText);
        EncryptFile(tempFile, encFile, password);
        string result = Convert.ToBase64String(File.ReadAllBytes(encFile));
        File.Delete(tempFile);
        File.Delete(encFile);
        return result;
    }

    public string DecryptString(string encryptedBase64, string password)
    {
        string tempFile = Path.GetTempFileName();
        string decFile = Path.GetTempFileName();
        File.WriteAllBytes(tempFile, Convert.FromBase64String(encryptedBase64));
        DecryptFile(tempFile, decFile, password);
        string result = File.ReadAllText(decFile);
        File.Delete(tempFile);
        File.Delete(decFile);
        return result;
    }
}

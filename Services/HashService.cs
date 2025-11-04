using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace BackupMonitor.Services;

public class HashService
{
    public string ComputeSha256(string filePath)
    {
        using (var sha256 = SHA256.Create())
        using (var stream = File.OpenRead(filePath))
        {
            byte[] hash = sha256.ComputeHash(stream);
            return BytesToHex(hash);
        }
    }

    private string BytesToHex(byte[] bytes)
    {
        StringBuilder hex = new StringBuilder(bytes.Length * 2);
        foreach (byte b in bytes)
            hex.AppendFormat("{0:x2}", b);
        return hex.ToString();
    }
}
namespace Soapbox.DataAccess.FileSystem.Encryption;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

public static class Encryptor
{
    public static string? Encrypt(object? value, string key = "Soapbox")
    {
        if (value == null)
            return null;

        return value switch
        {
            string stringValue => Encrypt(stringValue, key),
            _ => Encrypt(value?.ToString() ?? string.Empty, key)
        };
    }

    public static string? Encrypt(string value, string key = "Soapbox")
    {
        if (value == null)
            return null;

        using var aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(key.PadRight(32)[..32]);
        aes.GenerateIV();

        var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        var plainBytes = Encoding.UTF8.GetBytes(value);
        var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
        return Convert.ToBase64String(cipherBytes);
    }

    public static string? Decrypt(string value, string key = "Soapbox")
    {
        if (value == null)
            return null;

        var cipherBytes = File.ReadAllBytes(value);

        using var aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(key.PadRight(32)[..32]);
        aes.IV = [.. cipherBytes.Take(16)];

        var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 16, cipherBytes.Length);
        return Encoding.UTF8.GetString(plainBytes);
    }
}

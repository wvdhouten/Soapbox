namespace Soapbox.DataAccess.FileSystem.Encryption;

using Alkaline64.Injectable;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

[Injectable]
public class EncryptedFileHandler
{
    private string EncryptionKey { get; init; } = string.Empty;

    public EncryptedFileHandler(IConfiguration configuration)
    {
        EncryptionKey = configuration.GetValue<string>(nameof(EncryptionKey)) ?? "Soapbox";
    }

    public async Task WriteAllTextAsync(string filePath, string content)
    {
        using var aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(EncryptionKey.PadRight(32)[..32]);
        aes.GenerateIV();

        using var ms = new MemoryStream();
        ms.Write(aes.IV, 0, aes.IV.Length);

        using (var cryptoStream = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
        {
            byte[] jsonBytes = Encoding.UTF8.GetBytes(content);
            cryptoStream.Write(jsonBytes, 0, jsonBytes.Length);
        }

        await File.WriteAllBytesAsync(filePath, ms.ToArray());
    }

    public async Task<string> ReadAllTextAsync(string filePath)
    {
        byte[] encryptedContent = File.ReadAllBytes(filePath);

        using var aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(EncryptionKey.PadRight(32)[..32]);

        using var ms = new MemoryStream(encryptedContent);
        byte[] iv = new byte[16];
        ms.Read(iv, 0, iv.Length);
        aes.IV = iv;

        using var cryptoStream = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);
        using var sr = new StreamReader(cryptoStream, Encoding.UTF8);
        return await sr.ReadToEndAsync();
    }
}

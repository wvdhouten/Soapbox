namespace Soapbox.Application.Utils;

using System.Security.Cryptography;
using System.Text;

public static class Gravatar
{
    public static string GetHash(string email)
    {
        var data = Encoding.Default.GetBytes(email.ToLowerInvariant());

        using var md5 = MD5.Create();
        md5.TransformFinalBlock(data, 0, data.Length);
        return md5.Hash is not null
            ? string.Concat(md5.Hash.Select(b => b.ToString("x2")))
            : string.Empty;
    }
}

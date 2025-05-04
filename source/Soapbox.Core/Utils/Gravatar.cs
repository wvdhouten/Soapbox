namespace Soapbox.Application.Utils;

using System;
using System.Security.Cryptography;
using System.Text;

public static class Gravatar
{
    public static string GetHash(string email)
    {
        var hash = string.Empty;
        var data = Encoding.Default.GetBytes(email.ToLowerInvariant());
        using (var md5 = MD5.Create())
        {
            var enc = md5.TransformFinalBlock(data, 0, data.Length);
            foreach (var b in md5.Hash)
                hash += Convert.ToString(b, 16).ToLower().PadLeft(2, '0');
            md5.Clear();
        }

        return hash;
    }
}

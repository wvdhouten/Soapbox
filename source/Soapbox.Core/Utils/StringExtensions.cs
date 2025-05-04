namespace Soapbox.Application.Extensions;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

public static partial class StringExtensions
{
    public static string RemoveDiacritics(this string text)
    {
        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();

        foreach (var c in normalizedString)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
    }

    public static string RemoveReservedUrlCharacters(this string text)
    {
        var reservedCharacters = new List<string> { "!", "#", "$", "&", "'", "(", ")", "*", ",", "/", ":", ";", "=", "?", "@", "[", "]", "\"", "%", ".", "<", ">", "\\", "^", "_", "'", "{", "}", "|", "~", "`", "+" };

        foreach (var chr in reservedCharacters)
        {
            text = text.Replace(chr, string.Empty, StringComparison.OrdinalIgnoreCase);
        }

        return text;
    }

    public static string StripHtml(this string html, bool decode = true)
    {
        var expression = HtmlRegex();
        var stripped = expression.Replace(html, string.Empty);
        return decode ? HttpUtility.HtmlDecode(stripped) : stripped;
    }

    public static string Clip(this string text, int length)
    {
        if (text.Length < length)
        {
            return text;
        }

        text = text[..length];
        var lastSpaceIndex = text.LastIndexOf(' ');
        text = lastSpaceIndex > -1 ? text[..lastSpaceIndex] : text;
        return $"{text}…";
    }

    [GeneratedRegex("<[^>]+>", RegexOptions.IgnoreCase)]
    private static partial Regex HtmlRegex();
}

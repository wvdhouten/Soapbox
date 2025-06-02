namespace Soapbox.Application.Utils;

using Soapbox.Application.Extensions;
using System;

public class Slugifier
{
    public static string Slugify(string input)
    {
        input = input?.Trim().ToLowerInvariant().Replace(" ", "-", StringComparison.OrdinalIgnoreCase) ?? string.Empty;
        input = input.RemoveDiacritics();
        input = input.RemoveReservedUrlCharacters();

        return input.ToLowerInvariant();
    }
}

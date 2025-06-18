namespace Soapbox.DataAccess.FileSystem.Serialization;

using Soapbox.DataAccess.FileSystem.Serialization.Converters;
using System.Text.Json;

internal static class FileSystemSerialization
{
    internal static JsonSerializerOptions DefaultJsonSerializerOptions { get; }

    static FileSystemSerialization()
    {
        DefaultJsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        DefaultJsonSerializerOptions.Converters.Add(new UserLoginInfoJsonConverter());
    }
}

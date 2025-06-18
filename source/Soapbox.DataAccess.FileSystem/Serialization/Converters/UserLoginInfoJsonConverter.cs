namespace Soapbox.DataAccess.FileSystem.Serialization.Converters;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;

public class UserLoginInfoJsonConverter : JsonConverter<UserLoginInfo>
{
    public override UserLoginInfo? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException();

        string? loginProvider = null;
        string? providerKey = null;
        string? providerDisplayName = null;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                break;

            if (reader.TokenType != JsonTokenType.PropertyName)
                continue;

            string propertyName = reader.GetString()!;
            reader.Read();

            switch (propertyName)
            {
                case nameof(UserLoginInfo.LoginProvider):
                    loginProvider = reader.GetString();
                    break;
                case nameof(UserLoginInfo.ProviderKey):
                    providerKey = reader.GetString();
                    break;
                case nameof(UserLoginInfo.ProviderDisplayName):
                    providerDisplayName = reader.GetString();
                    break;
            }
        }

        if (loginProvider == null || providerKey == null)
            throw new JsonException("Missing required properties for UserLoginInfo.");

        return new UserLoginInfo(loginProvider, providerKey, providerDisplayName);
    }

    public override void Write(Utf8JsonWriter writer, UserLoginInfo value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString(nameof(UserLoginInfo.LoginProvider), value.LoginProvider);
        writer.WriteString(nameof(UserLoginInfo.ProviderKey), value.ProviderKey);
        writer.WriteString(nameof(UserLoginInfo.ProviderDisplayName), value.ProviderDisplayName);
        writer.WriteEndObject();
    }
}

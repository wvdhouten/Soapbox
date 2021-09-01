namespace Soapbox.Core.FileManagement
{
    using System;
    using System.IO;
    using System.Text.Json;

    public class ConfigFileService
    {
        public void SaveToFile<T>(T config, string path)
        {
            var configPath = Path.Combine(Environment.CurrentDirectory, "Config", path);

            var options = new JsonSerializerOptions { WriteIndented = true };

            using var stream = new FileStream(configPath, FileMode.Create, FileAccess.Write);
            using var writer = new Utf8JsonWriter(stream);
            writer.WriteStartObject();
            writer.WritePropertyName(typeof(T).Name);

            JsonSerializer.Serialize(writer, config, options);

            writer.WriteEndObject();
            writer.Flush();
        }
    }
}

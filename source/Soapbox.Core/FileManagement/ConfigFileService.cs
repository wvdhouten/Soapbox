namespace Soapbox.Core.FileManagement
{
    using System;
    using System.IO;
    using System.Text.Json;

    public class ConfigFileService
    {
        public void SaveToFile<T>(T config, string fileName)
        {
            var configPath = Path.Combine(Environment.CurrentDirectory, "Config", fileName);

            var options = new JsonWriterOptions { Indented = true };

            using var stream = new FileStream(configPath, FileMode.Create, FileAccess.Write);
            using var writer = new Utf8JsonWriter(stream, options);
            writer.WriteStartObject();
            writer.WritePropertyName(typeof(T).Name);

            JsonSerializer.Serialize(writer, config);

            writer.WriteEndObject();
            writer.Flush();
        }
    }
}

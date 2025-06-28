using System.Text.Json;

namespace MetricsServer.Models
{
    public class Issue
    {
        public string Key { get; set; } = string.Empty;
        public JsonElement Fields { get; set; }
        public JsonElement Changelog { get; set; }

        public Issue(JsonElement element)
        {
            if (element.TryGetProperty("key", out var key))
            {
                Key = key.GetString() ?? string.Empty;
            }

            if (element.TryGetProperty("fields", out var fields))
            {
                Fields = fields;
            }

            if (element.TryGetProperty("changelog", out var changelog))
            {
                Changelog = changelog;
            }
        }
    }
} 
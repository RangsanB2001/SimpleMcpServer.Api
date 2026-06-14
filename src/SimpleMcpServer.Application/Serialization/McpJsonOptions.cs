using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace SimpleMcpServer.Application.Serialization;

public static class McpJsonOptions
{
    public static JsonSerializerOptions Default { get; } = Build();

    private static JsonSerializerOptions Build()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false,
            TypeInfoResolver = new DefaultJsonTypeInfoResolver()
        };

        options.Converters.Add(new NullableDateTimeConverter());
        options.Converters.Add(new DateTimeConverter());

        return options;
    }
}

internal sealed class NullableDateTimeConverter : JsonConverter<DateTime?>
{
    public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        reader.TokenType == JsonTokenType.Null ? null : reader.GetDateTime();

    public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
    {
        if (!value.HasValue || value.Value == DateTime.MinValue)
        {
            writer.WriteNullValue();
            return;
        }

        DateTimeConverter.WriteCleanDate(writer, value.Value);
    }
}

internal sealed class DateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        reader.GetDateTime();

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        if (value == DateTime.MinValue)
        {
            writer.WriteNullValue();
            return;
        }

        WriteCleanDate(writer, value);
    }

    internal static void WriteCleanDate(Utf8JsonWriter writer, DateTime value)
    {
        if (value.TimeOfDay == TimeSpan.Zero)
        {
            writer.WriteStringValue(value.ToString("yyyy-MM-dd"));
            return;
        }

        writer.WriteStringValue(value.ToString("yyyy-MM-ddTHH:mm:ss"));
    }
}

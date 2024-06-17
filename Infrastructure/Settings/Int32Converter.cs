using System.Text.Json;
using System.Text.Json.Serialization;

namespace hh_analyzer.Infrastructure.Settings
{
    internal class Int32Converter : JsonConverter<int>
    {
        public override int Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                string stringValue = reader.GetString()!;
                if (int.TryParse(stringValue, out int intValue))
                    return intValue;

                throw new JsonException(
                    $"Unable to convert \"{stringValue}\" to integer.");
            }
            if (reader.TokenType == JsonTokenType.Number)
                return reader.GetInt32();

            throw new JsonException(
                $"Unexpected token parsing date. Expected String, got {reader.TokenType}.");
        }

        public override void Write(
            Utf8JsonWriter writer,
            int value,
            JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value);
        }
    }
}

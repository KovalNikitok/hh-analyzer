using System.Text.Json;
using System.Text.Json.Serialization;
using System.Globalization;

namespace hh_analyzer.Infrastructure.Settings
{
    public class DateTimeConverter : JsonConverter<DateTime>
    {
        // DateTime format at api.hh.ru responses
        private readonly string _dateFormat = "yyyy-MM-ddTHH:mm:sszzz";

        public override DateTime Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                string? dateString = reader.GetString();
                if(string.IsNullOrEmpty(dateString))
                    return DateTime.MinValue;
                if (DateTime.TryParseExact(
                        dateString, 
                        _dateFormat, 
                        CultureInfo.InvariantCulture, 
                        DateTimeStyles.None, 
                        out DateTime date))
                {
                    return date;
                }
                throw new JsonException(
                    $"Unable to convert \"{dateString}\" to DateTime using format \"{_dateFormat}\".");
            }
            throw new JsonException(
                $"Unexpected token parsing date. Expected String, got {reader.TokenType}.");
        }

        public override void Write(
            Utf8JsonWriter writer, 
            DateTime value, 
            JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(_dateFormat));
        }
    }
}

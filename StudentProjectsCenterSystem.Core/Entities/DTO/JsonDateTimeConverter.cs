using System.Text.Json;
using System.Text.Json.Serialization;

namespace StudentProjectsCenter.Core.Entities.DTO
{
    public class JsonDateTimeConverter : JsonConverter<DateTimeOffset?>
    {
        public override DateTimeOffset? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string? dateString = reader.GetString();
            if (string.IsNullOrEmpty(dateString))
            {
                return null;
            }

            if (DateTimeOffset.TryParse(dateString, out DateTimeOffset result))
            {
                return result;
            }

            return null;
        }

        public override void Write(Utf8JsonWriter writer, DateTimeOffset? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
            {
                // Customize the format as per your requirement
                writer.WriteStringValue(value.Value.ToString("yyyy-MM-ddTHH:mm:sszzz"));
            }
            else
            {
                writer.WriteNullValue();
            }
        }
    }
}

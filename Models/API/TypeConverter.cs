using System.Text.Json;
using System.Text.Json.Serialization;

namespace CarCareTracker.Models
{
    public class DummyType
    {

    }
    class InvariantConverter : JsonConverter<DummyType>
    {
        public override void Write(Utf8JsonWriter writer, DummyType value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
        public override DummyType? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
    class FromDateOptional: JsonConverter<string>
    {
        public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var tokenType = reader.TokenType;
            if (tokenType == JsonTokenType.String)
            {
                return reader.GetString();
            }
            else if (tokenType == JsonTokenType.Number)
            {
                if (reader.TryGetInt64(out long intInput))
                {
                    return DateTimeOffset.FromUnixTimeSeconds(intInput).Date.ToShortDateString();
                }
            }
            return reader.GetString();
        }
        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        {
            if (options.Converters.Any(x => x.Type == typeof(DummyType)))
            {
                try
                {
                    writer.WriteStringValue(DateTime.Parse(value).ToString("yyyy-MM-dd"));
                }
                catch
                {
                    writer.WriteStringValue(value);
                }
            }
            else
            {
                writer.WriteStringValue(value);
            }
        }
    }
    class FromDecimalOptional : JsonConverter<string>
    {
        public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var tokenType = reader.TokenType;
            if (tokenType == JsonTokenType.String)
            {
                return reader.GetString();
            }
            else if (tokenType == JsonTokenType.Number) {
                if (reader.TryGetDecimal(out decimal decimalInput))
                {
                    return decimalInput.ToString();
                }
            }
            return reader.GetString();
        }
        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        {
            if (options.Converters.Any(x=>x.Type == typeof(DummyType)))
            {
                writer.WriteNumberValue(decimal.Parse(value));
            } else
            {
                writer.WriteStringValue(value);
            }
        }
    }
    class FromIntOptional : JsonConverter<string>
    {
        public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var tokenType = reader.TokenType;
            if (tokenType == JsonTokenType.String)
            {
                return reader.GetString();
            }
            else if (tokenType == JsonTokenType.Number)
            {
                if (reader.TryGetInt32(out int intInput))
                {
                    return intInput.ToString();
                }
            }
            return reader.GetString();
        }
        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        {
            if (options.Converters.Any(x => x.Type == typeof(DummyType)))
            {
                writer.WriteNumberValue(int.Parse(value));
            }
            else
            {
                writer.WriteStringValue(value);
            }
        }
    }
    class FromBoolOptional : JsonConverter<string>
    {
        public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var tokenType = reader.TokenType;
            switch (tokenType)
            {
                case JsonTokenType.String: 
                    return reader.GetString();
                case JsonTokenType.True:
                    return "True";
                case JsonTokenType.False:
                    return "False";
                default:
                    return reader.GetString();
            }
        }
        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        {
            if (options.Converters.Any(x => x.Type == typeof(DummyType)))
            {
                writer.WriteBooleanValue(bool.Parse(value));
            }
            else
            {
                writer.WriteStringValue(value);
            }
        }
    }
}

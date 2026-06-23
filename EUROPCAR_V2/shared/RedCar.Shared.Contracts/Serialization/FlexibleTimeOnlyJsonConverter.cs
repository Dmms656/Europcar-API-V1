using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RedCar.Shared.Contracts.Serialization;

public sealed class FlexibleTimeOnlyJsonConverter : JsonConverter<TimeOnly>
{
    public override TimeOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.String:
                return FlexibleDateTimeParsing.ParseTimeOnly(reader.GetString()!);
            case JsonTokenType.StartObject:
                return ReadFromObject(ref reader);
            default:
                throw new JsonException(
                    $"No se pudo convertir el token {reader.TokenType} a TimeOnly.");
        }
    }

    public override void Write(Utf8JsonWriter writer, TimeOnly value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.ToString("HH:mm:ss", CultureInfo.InvariantCulture));

    private static TimeOnly ReadFromObject(ref Utf8JsonReader reader)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Se esperaba un objeto JSON para TimeOnly.");
        }

        int? hour = null;
        int? minute = null;
        int second = 0;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                break;
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                continue;
            }

            var prop = reader.GetString();
            reader.Read();

            switch (prop)
            {
                case "hour":
                    hour = reader.GetInt32();
                    break;
                case "minute":
                    minute = reader.GetInt32();
                    break;
                case "second":
                    second = reader.GetInt32();
                    break;
                default:
                    reader.Skip();
                    break;
            }
        }

        if (hour is null || minute is null)
        {
            throw new JsonException("El objeto TimeOnly debe incluir hour y minute.");
        }

        return new TimeOnly(hour.Value, minute.Value, second);
    }
}

public sealed class FlexibleNullableTimeOnlyJsonConverter : JsonConverter<TimeOnly?>
{
    private readonly FlexibleTimeOnlyJsonConverter _inner = new();

    public override TimeOnly? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        return _inner.Read(ref reader, typeof(TimeOnly), options);
    }

    public override void Write(Utf8JsonWriter writer, TimeOnly? value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }

        _inner.Write(writer, value.Value, options);
    }
}

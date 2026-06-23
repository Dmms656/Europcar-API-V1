using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RedCar.Shared.Contracts.Serialization;

public sealed class FlexibleDateOnlyJsonConverter : JsonConverter<DateOnly>
{
    public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.String:
                return FlexibleDateTimeParsing.ParseDateOnly(reader.GetString()!);
            case JsonTokenType.StartObject:
                return ReadFromObject(ref reader);
            default:
                throw new JsonException(
                    $"No se pudo convertir el token {reader.TokenType} a DateOnly.");
        }
    }

    public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));

    private static DateOnly ReadFromObject(ref Utf8JsonReader reader)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Se esperaba un objeto JSON para DateOnly.");
        }

        int? year = null;
        int? month = null;
        int? day = null;

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
                case "year":
                    year = reader.GetInt32();
                    break;
                case "month":
                    month = reader.GetInt32();
                    break;
                case "day":
                    day = reader.GetInt32();
                    break;
                default:
                    reader.Skip();
                    break;
            }
        }

        if (year is null || month is null || day is null)
        {
            throw new JsonException("El objeto DateOnly debe incluir year, month y day.");
        }

        return new DateOnly(year.Value, month.Value, day.Value);
    }
}

public sealed class FlexibleNullableDateOnlyJsonConverter : JsonConverter<DateOnly?>
{
    private readonly FlexibleDateOnlyJsonConverter _inner = new();

    public override DateOnly? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        return _inner.Read(ref reader, typeof(DateOnly), options);
    }

    public override void Write(Utf8JsonWriter writer, DateOnly? value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }

        _inner.Write(writer, value.Value, options);
    }
}

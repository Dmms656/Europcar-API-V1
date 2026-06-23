using System.Text.Json;
using System.Text.Json.Serialization;

namespace RedCar.Shared.Contracts.Serialization;

public static class JsonSerializerOptionsExtensions
{
    public static JsonSerializerOptions AddFlexibleDateTimeConverters(this JsonSerializerOptions options)
    {
        options.Converters.Add(new FlexibleDateOnlyJsonConverter());
        options.Converters.Add(new FlexibleNullableDateOnlyJsonConverter());
        options.Converters.Add(new FlexibleTimeOnlyJsonConverter());
        options.Converters.Add(new FlexibleNullableTimeOnlyJsonConverter());
        return options;
    }
}

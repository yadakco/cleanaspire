using System.Text.Json;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CleanAspire.Infrastructure.Persistence.Conversions;
#nullable disable warnings
public static class ValueConversionExtensions
{
    private readonly static JsonSerializerOptions DefaultJsonSerializerOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
    public static PropertyBuilder<T> HasJsonConversion<T>(this PropertyBuilder<T> propertyBuilder)
    {
        var converter = new ValueConverter<T, string>(
            v => JsonSerializer.Serialize(v, DefaultJsonSerializerOptions),
            v => string.IsNullOrEmpty(v)
                ? default
                : JsonSerializer.Deserialize<T>(v, DefaultJsonSerializerOptions));

        var comparer = new ValueComparer<T>(
            (l, r) => JsonSerializer.Serialize(l, DefaultJsonSerializerOptions) ==
                      JsonSerializer.Serialize(r, DefaultJsonSerializerOptions),
            v => v == null ? 0 : JsonSerializer.Serialize(v, DefaultJsonSerializerOptions).GetHashCode(),
            v => JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(v, DefaultJsonSerializerOptions),
                DefaultJsonSerializerOptions));

        propertyBuilder.HasConversion(converter);
        propertyBuilder.Metadata.SetValueConverter(converter);
        propertyBuilder.Metadata.SetValueComparer(comparer);
        return propertyBuilder;
    }

    public static PropertyBuilder<List<string>> HasStringListConversion(
        this PropertyBuilder<List<string>> propertyBuilder)
    {
        var converter = new ValueConverter<List<string>, string>(
            v => JsonSerializer.Serialize(v, DefaultJsonSerializerOptions),
            v => string.IsNullOrEmpty(v)
                ? default
                : JsonSerializer.Deserialize<List<string>>(v, DefaultJsonSerializerOptions));

        var comparer = new ValueComparer<List<string>>(
            (l, r) => JsonSerializer.Serialize(l, DefaultJsonSerializerOptions) ==
                      JsonSerializer.Serialize(r, DefaultJsonSerializerOptions),
            v => v == null ? 0 : JsonSerializer.Serialize(v, DefaultJsonSerializerOptions).GetHashCode(),
            v => JsonSerializer.Deserialize<List<string>>(
                JsonSerializer.Serialize(v, DefaultJsonSerializerOptions),
                DefaultJsonSerializerOptions));

        propertyBuilder.HasConversion(converter);
        propertyBuilder.Metadata.SetValueConverter(converter);
        propertyBuilder.Metadata.SetValueComparer(comparer);
        return propertyBuilder;
    }
}
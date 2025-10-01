[global::System.ComponentModel.TypeConverter(typeof(TYPE_NAMETypeConverter))]
[global::System.Text.Json.Serialization.JsonConverter(typeof(TYPE_NAMESystemTextJsonConverter))]
partial TYPE_KEYWORD TYPE_NAME :
    global::System.ISpanFormattable,
    global::System.IParsable<TYPE_NAME>,
    global::System.ISpanParsable<TYPE_NAME>,
    global::System.IComparable<TYPE_NAME>,
    global::System.IEquatable<TYPE_NAME>,
    global::System.IFormattable
{
    public string Value { get; }

    public TYPE_NAME(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        Value = value;
    }

    public static readonly TYPE_NAME Empty = new TYPE_NAME(string.Empty);

    /// <inheritdoc cref="IEquatable{T}"/>
    public bool Equals(TYPE_NAME? other) 
        => (Value, other?.Value) switch
        {
            (_, null) => false,
            (_, _) => Value.Equals(other.Value.Value, StringComparison.Ordinal),
        };

    public override int GetHashCode() => Value.GetHashCode();

    public override string ToString() => Value;

    public static bool operator >(TYPE_NAME a, TYPE_NAME b) => a.CompareTo(b) > 0;
    public static bool operator <(TYPE_NAME a, TYPE_NAME b) => a.CompareTo(b) < 0;
    public static bool operator >=(TYPE_NAME a, TYPE_NAME b) => a.CompareTo(b) >= 0;
    public static bool operator <=(TYPE_NAME a, TYPE_NAME b) => a.CompareTo(b) <= 0;

    /// <inheritdoc cref="IComparable{TSelf}"/>
    public int CompareTo(TYPE_NAME other)
        => (Value, other.Value) switch
        {
            (_, null) => 1,
            (_, _) => string.CompareOrdinal(Value, other.Value),
        };

    public static TYPE_NAME Parse(string input) => new(input);

    /// <inheritdoc cref="IParsable{TSelf}"/>
    public static TYPE_NAME Parse(string input, IFormatProvider? provider) => new(input);

    /// <inheritdoc cref="IParsable{TSelf}"/>
    public static bool TryParse([global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)] string? input, IFormatProvider? provider, out TYPE_NAME result)
    {
        if (input is null)
        {
            result = default;
            return false;
        }

        result = new(input);
        return true;
    }

    /// <inheritdoc cref="IFormattable"/>
    public string ToString(string? format, IFormatProvider? formatProvider) => Value;

    public static TYPE_NAME Parse(ReadOnlySpan<char> input) => new(input.ToString());

    /// <inheritdoc cref="ISpanParsable{TSelf}"/>
    public static TYPE_NAME Parse(ReadOnlySpan<char> input, IFormatProvider? provider) => new(input.ToString());

    /// <inheritdoc cref="ISpanParsable{TSelf}"/>
    public static bool TryParse(ReadOnlySpan<char> input, IFormatProvider? provider, out TYPE_NAME result)
    {
        result = new(input.ToString());
        return true;
    }

    /// <inheritdoc cref="ISpanFormattable"/>
    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider) => TryFormat(destination, out charsWritten, format);

    /// <inheritdoc cref="ISpanFormattable"/>
    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default)
    {
        if (destination.Length > Value.Length)
        {
            MemoryExtensions.AsSpan(Value).CopyTo(destination);
            charsWritten = Value.Length;
            return true;
        }

        charsWritten = default;
        return false;
    }

    public partial class TYPE_NAMETypeConverter : global::System.ComponentModel.TypeConverter
    {
        public override bool CanConvertFrom(global::System.ComponentModel.ITypeDescriptorContext? context, Type sourceType)
        {
            return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
        }

        public override object? ConvertFrom(global::System.ComponentModel.ITypeDescriptorContext? context, global::System.Globalization.CultureInfo? culture, object value)
        {
            if (value is string stringValue)
            {
                return new TYPE_NAME(stringValue);
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override bool CanConvertTo(global::System.ComponentModel.ITypeDescriptorContext? context, Type? sourceType)
        {
            return sourceType == typeof(string) || base.CanConvertTo(context, sourceType);
        }

        public override object? ConvertTo(global::System.ComponentModel.ITypeDescriptorContext? context, global::System.Globalization.CultureInfo? culture, object? value, Type destinationType)
        {
            if (value is TYPE_NAME idValue)
            {
                if (destinationType == typeof(string))
                {
                    return idValue.Value;
                }
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

    public partial class TYPE_NAMESystemTextJsonConverter : global::System.Text.Json.Serialization.JsonConverter<TYPE_NAME>
    {
        public override TYPE_NAME Read(ref global::System.Text.Json.Utf8JsonReader reader, Type typeToConvert, global::System.Text.Json.JsonSerializerOptions options) => new(reader.GetString()!);

        public override void Write(global::System.Text.Json.Utf8JsonWriter writer, TYPE_NAME value, global::System.Text.Json.JsonSerializerOptions options) => writer.WriteStringValue(value.Value);

        public override TYPE_NAME ReadAsPropertyName(ref global::System.Text.Json.Utf8JsonReader reader, Type typeToConvert, global::System.Text.Json.JsonSerializerOptions options) => new(reader.GetString() ?? throw new FormatException("The string for the TYPE_NAME property was null"));

        public override void WriteAsPropertyName(global::System.Text.Json.Utf8JsonWriter writer, TYPE_NAME value, global::System.Text.Json.JsonSerializerOptions options) => writer.WritePropertyName(value.Value);
    }
}
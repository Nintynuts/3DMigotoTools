namespace Migoto.Log.Parser;

class HashTypeConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
        => sourceType == typeof(string);

    public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
        => destinationType == typeof(uint);

    public override object ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
        => uint.Parse((string)value, NumberStyles.HexNumber);

    public override object ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type? destinationType)
        => ((uint)(value ?? throw new ArgumentNullException(nameof(value)))).ToString("X");
}

class LongHashTypeConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
        => sourceType == typeof(string);

    public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
        => destinationType == typeof(ulong);

    public override object ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
        => ulong.Parse((string)value, NumberStyles.HexNumber);

    public override object ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type? destinationType)
        => ((ulong)(value ?? throw new ArgumentNullException(nameof(value)))).ToString("X");
}
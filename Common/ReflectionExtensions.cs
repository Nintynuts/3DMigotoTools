namespace System.Reflection;

public static class ReflectionExtensions
{
    public static object Construct(this Type type, params object[] args) => ThrowIfNull(Activator.CreateInstance(type, args));

    public static T Construct<T>(this Type type, params object[] args) => ThrowIfNot<T>(Activator.CreateInstance(type, args));

    #region Static
    public static void Set(this PropertyInfo prop, object value) => prop.SetValue(null, value);

    public static T Get<T>(this PropertyInfo prop) => ThrowIfNot<T>(prop.GetValue(null));

    public static object? Get(this PropertyInfo prop) => prop.GetValue(null);

    public static void Call(this MethodInfo method, params object[] args) => method.Invoke(null, args);

    public static T Call<T>(this MethodInfo method, params object[] args) => ThrowIfNot<T>(method.Invoke(null, args));
    #endregion

    #region Instance
    public static void SetTo(this PropertyInfo prop, object target, object value) => prop.SetValue(target, value);

    public static T GetFrom<T>(this PropertyInfo prop, object target) => ThrowIfNot<T>(prop.GetValue(target));

    public static object? GetFrom(this PropertyInfo prop, object target) => prop.GetValue(target);

    public static void CallOn(this MethodInfo method, object target, params object[] args) => method.Invoke(target, args);

    public static T CallOn<T>(this MethodInfo method, object target, params object[] args) => ThrowIfNot<T>(method.Invoke(target, args));

    #endregion

    private static T ThrowIfNot<T>(object? value) => value is T result ? result : throw new InvalidCastException();

    private static T ThrowIfNull<T>(T? value) => ThrowIfNot<T>(value);

    public static void SetFromString(this object target, string name, string value)
    {
        var prop = ThrowIfNull(target.GetType().GetProperty(name));
        var converter = TypeDescriptor.GetProperties(target.GetType()).Find(name, false);
        prop.SetTo(target, converter?.Converter.ConvertFromString(value) ?? throw new InvalidDataException($"Couldn't parse {value} to {prop.PropertyType}"));
    }

    public static void Add<T>(this PropertyInfo list, object target, T value) where T : notnull
    {
        var add = ThrowIfNull(list.PropertyType.GetMethod(nameof(ICollection<T>.Add)));
        var listObj = ThrowIfNull(list.GetFrom(target));
        add.CallOn(listObj, value);
    }

    public static bool IsGeneric(this PropertyInfo prop, Type genericType)
        => prop.PropertyType.IsGenericType && genericType == prop.PropertyType.GetGenericTypeDefinition();

    public static Type FirstType(this PropertyInfo prop)
        => prop.PropertyType.GetGenericArguments()[0];

    public static IEnumerable<PropertyInfo> OfType<T>(this PropertyInfo[] properties)
        => properties.Where(p => p.PropertyType.Is<T>());

    public static bool Is<T>(this Type type)
        => typeof(T).IsAssignableFrom(type);
}

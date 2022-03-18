using jasMIN.Net2TypeScript.SettingsModel;
using System.Collections;
using System.Globalization;
using System.Text;

namespace jasMIN.Net2TypeScript.Utils;

static class TypeExtensions
{
    static void ThrowIfNullable(Type type)
    {
        if (type.IsDotNetNullableValueType())
        {
            throw new InvalidOperationException("Nullable types are not allowed at this point");
        }
    }

    public static bool IsDotNetNullableValueType(this Type type)
    {
        return type.IsValueType && Nullable.GetUnderlyingType(type) != null;
    }

    public static bool DerivesFromClass(this Type type, Type baseType)
    {
        var curr = type;
        while (curr != null && curr != baseType)
        {
            curr = curr.BaseType;
        }
        return curr == baseType;
    }


    public static bool ImplementsInterface(this Type type, Type interfaceType)
    {
        return type.GetInterfaces().Any(i =>
            interfaceType.IsGenericType
                ? i.IsGenericType && i.GetGenericTypeDefinition() == interfaceType
                : i == type);
    }

    public static bool IsTypeScriptNumberType(this Type type)
    {
        ThrowIfNullable(type);

        return new[] {
                typeof(short),
                typeof(int),
                typeof(long),
                typeof(decimal),
                typeof(float),
                typeof(double)
            }.Any(t => t == type);
    }

    public static bool IsTypeScriptArrayType(this Type type)
    {
        ThrowIfNullable(type);
        return typeof(IEnumerable).IsAssignableFrom(type) && !type.IsEnum && type != typeof(string) && !type.IsTypeScriptRecordType();
    }

    public static bool IsTypeScriptRecordType(this Type type)
    {
        ThrowIfNullable(type);
        return type.IsDictionaryInterface() || type.GetInterfaces().Any(i => i.IsDictionaryInterface());
    }

    public static bool IsTypeScriptDateType(this Type type)
    {
        ThrowIfNullable(type);

        return new[] {
                typeof(DateTime),
                typeof(DateTimeOffset)
            }.Any(t => t == type);
    }

    public static bool IsTypeScriptBoolType(this Type type)
    {
        ThrowIfNullable(type);

        return type == typeof(bool);
    }

    public static bool IsTypeScriptStringType(this Type type)
    {
        ThrowIfNullable(type);

        return new[] {
                typeof(string),
                typeof(Guid),
                typeof(byte),
                typeof(byte[]),
                typeof(TimeSpan)
            }.Any(t => t == type);
    }

    public static bool IsTypeScriptInterfaceType(this Type type)
    {
        return (type.IsClass || type.IsInterface) && type != typeof(string);
    }

    public static bool IsTypeScriptNullableType(this Type type, Settings settings, bool isArrayItem, bool isDictionaryValue)
    {
        ThrowIfNullable(type);

        var result = false;

        if (type.IsTypeScriptArrayType())
        {
            result = settings.NonNullableArrays != true;
        }
        else if (type.IsTypeScriptRecordType())
        {
            result = settings.NonNullableDictionaries != true;
        }
        else if (type.IsTypeScriptInterfaceType())
        {
            if (isArrayItem)
            {
                result = settings.NonNullableArrayEntityItems != true;
            }
            else if (isDictionaryValue)
            {
                result = settings.NonNullableDictionaryEntityValues != true;
            }
            else
            {
                result = settings.NonNullableEntities != true;
            }
        }
        else if (type.IsTypeScriptStringType() && type != typeof(Guid) && type != typeof(TimeSpan))
        {
            result = true;
        }

        return result;
    }

    public static bool IsTypeScriptObservableType(this Type type, Settings settings)
    {
        return settings.KnockoutMapping == KnockoutMappingOptions.All || settings.KnockoutMapping == KnockoutMappingOptions.ValueTypes && !type.IsTypeScriptInterfaceType();

    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
    public static bool TryGetTypeScriptNamespaceName(this Type type, Settings settings, out string outputString)
    {
        var result = true;
        try
        {
            outputString = settings.ToTsFullName(type.Namespace ?? string.Empty);
        }
        catch (Exception)
        {
            result = false;
            outputString = null!;
        }
        return result;
    }

    private static bool IsDictionaryInterface(this Type type)
    {
        var result = false;
        if (type.IsGenericType && type.IsInterface)
        {
            var genTypeDef = type.GetGenericTypeDefinition();
            result = genTypeDef == typeof(IDictionary<,>) || genTypeDef == typeof(IReadOnlyDictionary<,>);
        }
        return result;
    }
}

static class SettingsExtensions
{
    public static string ToTsFullName(this Settings settings, string dotNetFullName)
    {
        var namespaceSegment = dotNetFullName.Remove(0, Math.Min(dotNetFullName.Length, settings.DotNetRootNamespace.Length + 1));
        return string.IsNullOrWhiteSpace(settings.TsRootNamespace) ? namespaceSegment : $"{settings.TsRootNamespace}.{namespaceSegment}";
    }
}

static class StringExtensions
{
    public static string ToCamelCase(this string str)
    {
        if (!string.IsNullOrEmpty(str))
        {
            str = string.Concat(str[..1].ToLowerInvariant(), str.AsSpan(1));
        }
        return str;
    }

    public static string GetRelativePathTo(this string absPath, string relTo, bool backSlash = false)
    {
        var parsed = Path.GetDirectoryName(absPath);
        if (parsed == null)
        {
            throw new ArgumentException("Not a valid directory path", nameof(absPath));
        }

        string[] absDirs = absPath.Split('\\');
        string[] relDirs = relTo.Split('\\');
        // Get the shortest of the two paths 
        int len = absDirs.Length < relDirs.Length ? absDirs.Length : relDirs.Length;
        // Use to determine where in the loop we exited 
        int lastCommonRoot = -1; int index;
        // Find common root 
        for (index = 0; index < len; index++)
        {
            if (absDirs[index] == relDirs[index])
                lastCommonRoot = index;
            else break;
        }
        // If we didn't find a common prefix then throw 
        if (lastCommonRoot == -1)
        {
            throw new ArgumentException("Paths do not have a common base");
        }
        // Build up the relative path 
        var relativePath = new StringBuilder();
        // Add on the .. 
        for (index = lastCommonRoot + 1; index < absDirs.Length; index++)
        {
            if (absDirs[index].Length > 0) relativePath.Append("..\\");
        }
        // Add on the folders 
        for (index = lastCommonRoot + 1; index < relDirs.Length - 1; index++)
        {
            relativePath.Append(relDirs[index] + "\\");
        }
        relativePath.Append(relDirs[^1]);

        var result = relativePath.ToString();
        if (!backSlash)
        {
            result = result.Replace("\\", "/", StringComparison.Ordinal);
        }

        return result;
    }
}

static class StreamWriterExtensions
{
    public static StreamWriter WriteFormat(this StreamWriter sw, string interpolationString, params object[] args)
    {
        sw.Write(string.Format(CultureInfo.InvariantCulture, interpolationString, args));
        return sw;
    }
}

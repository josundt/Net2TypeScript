using jasMIN.Net2TypeScript.SettingsModel;
using System.Collections;
using System.Globalization;
using System.Text;

namespace jasMIN.Net2TypeScript.Utils;

internal static class TypeExtensions
{
    internal static void ThrowIfNullable(Type type)
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
        return Array.Exists(
            type.GetInterfaces(),
            i => interfaceType.IsGenericType
                ? i.IsGenericType && i.GetGenericTypeDefinition() == interfaceType
                : i == type
        );
    }

    public static bool IsTypeScriptNumberType(this Type type)
    {
        ThrowIfNullable(type);

        return
            type == typeof(short) ||
            type == typeof(int) ||
            type == typeof(long) ||
            type == typeof(decimal) ||
            type == typeof(float) ||
            type == typeof(double);
    }

    public static bool IsTypeScriptArrayType(this Type type)
    {
        ThrowIfNullable(type);
        return typeof(IEnumerable).IsAssignableFrom(type) && !type.IsEnum && type != typeof(string) && !type.IsTypeScriptRecordType();
    }

    public static bool IsTypeScriptRecordType(this Type type)
    {
        ThrowIfNullable(type);
        return
            type.IsDictionaryInterface() ||
            Array.Exists(type.GetInterfaces(), i => i.IsDictionaryInterface());
    }

    public static bool IsTypeScriptDateType(this Type type)
    {
        ThrowIfNullable(type);

        return type == typeof(DateTime) || type == typeof(DateTimeOffset);
    }

    public static bool IsTypeScriptBoolType(this Type type)
    {
        ThrowIfNullable(type);

        return type == typeof(bool);
    }

    public static bool IsTypeScriptStringType(this Type type)
    {
        ThrowIfNullable(type);

        return
            type == typeof(string) ||
            type == typeof(Guid) ||
            type == typeof(byte) ||
            type == typeof(byte[]) ||
            type == typeof(TimeSpan);
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
            else
            {
                result = isDictionaryValue ? settings.NonNullableDictionaryEntityValues != true : settings.NonNullableEntities != true;
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
        return settings.KnockoutMapping == KnockoutMappingOptions.All || (settings.KnockoutMapping == KnockoutMappingOptions.ValueTypes && !type.IsTypeScriptInterfaceType());

    }

    public static bool TryGetTypeScriptNamespaceName(this Type type, Settings settings, out string outputString)
    {
        string? parsed;
#pragma warning disable CA1031 // Do not catch general exception types
        try
        {
            parsed = settings.ToTsFullName(type.Namespace ?? string.Empty);
            if (string.IsNullOrEmpty(parsed))
            {
                parsed = null;
            }
        }
        catch (Exception)
        {
            parsed = null!;
        }
#pragma warning restore CA1031 // Do not catch general exception types

        outputString = parsed!;
        return outputString != null;
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

internal static class SettingsExtensions
{
    public static string ToTsFullName(this Settings settings, string dotNetFullName)
    {
        var namespaceSegment = dotNetFullName.Remove(0, Math.Min(dotNetFullName.Length, settings.DotNetRootNamespace.Length + 1));
        return string.IsNullOrWhiteSpace(settings.TsRootNamespace) ? namespaceSegment : $"{settings.TsRootNamespace}.{namespaceSegment}";
    }
}

internal static class StringExtensions
{
    public static string ToCamelCase(this string str)
    {
        if (!string.IsNullOrEmpty(str))
        {
            str = string.Concat(str[..1].ToLowerInvariant(), str.AsSpan(1));
        }
        return str;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0059:Unnecessary assignment of a value", Justification = "<Pending>")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell", "S1481:Unused local variables should be removed", Justification = "<Pending>")]
    public static string GetRelativePathTo(this string absPath, string relTo, bool backSlash = false)
    {
        var parsed = Path.GetDirectoryName(absPath) ?? throw new ArgumentException("Not a valid directory path", nameof(absPath));

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
            {
                lastCommonRoot = index;
            }
            else
            {
                break;
            }
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
            if (absDirs[index].Length > 0)
            {
                relativePath.Append("..\\");
            }
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

internal static class StreamWriterExtensions
{
    public static StreamWriter WriteFormat(this StreamWriter sw, string interpolationString, params object[] args)
    {
        sw.Write(string.Format(CultureInfo.InvariantCulture, interpolationString, args));
        return sw;
    }
}

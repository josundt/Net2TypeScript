using System;
using System.Collections;

namespace jasMIN.Net2TypeScript.Model
{
    static class ExtensionMethods
    {
        public static bool IsNumericType(this Type type)
        {
            return
                type == typeof(short) ||
                type == typeof(int) ||
                type == typeof(long) ||
                type == typeof(decimal) ||
                type == typeof(float) ||
                type == typeof(double);
        }

        public static bool IsNullableType(this Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        public static string GetTypeScriptNamespaceName(this Type type, Settings settings)
        {
            return (settings.tsRootNamespace + type.Namespace.Remove(0, settings.clrRootNamespace.Length));
        }

        //public static string GetRelativeNamespaceName(this Type propertyType, Type ownerType, Settings settings)
        //{
        //    return propertyType.GetTypeScriptNamespaceName(settings);
        //}

        public static string GetTypeScriptTypeName(this Type propertyType, Type ownerType, Settings settings, bool skipKnockoutObservableWrapper = false)
        {
            var isNullableType = propertyType.IsNullableType();
            if (isNullableType)
            {
                propertyType = propertyType.GetGenericArguments()[0];
            }

            string tsType = null;

            if (IsNumericType(propertyType))
            {
                tsType = "number";
            }
            else if (propertyType == typeof(DateTime) || propertyType == typeof(DateTimeOffset))
            {
                tsType = "Date";
            }
            else if (propertyType == typeof(bool))
            {
                tsType = "boolean";
            }
            else if (propertyType == typeof(Guid) || propertyType == typeof(string))
            {
                tsType = "string";
            }
            else if (propertyType == typeof(byte[]) || propertyType == typeof(byte))
            {
                tsType = "string";
            }
            else if (propertyType.IsEnum)
            {
                if (settings.enumType == "enum" || settings.enumType == "stringliteral")
                {
                    var propertyTypeTsNs = propertyType.GetTypeScriptNamespaceName(settings);
                    tsType = $"{propertyTypeTsNs}.{propertyType.Name}";
                }
                else
                {
                    tsType = settings.enumType;
                }
            }
            else if (typeof(IEnumerable).IsAssignableFrom(propertyType))
            {
                tsType = string.Format(
                    "{0}[]",
                    propertyType.IsGenericType
                        ? GetTypeScriptTypeName(propertyType.GenericTypeArguments[0], ownerType, settings)
                        : "any");
            }
            else if (propertyType.IsClass || propertyType.IsInterface)
            {
                if (propertyType == typeof(Object))
                {
                    tsType = "Object";
                }
                else
                {
                    var propertyTypeTsNs = propertyType.GetTypeScriptNamespaceName(settings);
                    tsType = $"{propertyTypeTsNs}.{propertyType.Name}";
                }
            }

            if (settings.useKnockout && !skipKnockoutObservableWrapper)
            {
                if (typeof(IEnumerable).IsAssignableFrom(propertyType) && !propertyType.IsEnum && propertyType != typeof(string))
                {
                    tsType = string.Format(
                        "KnockoutObservableArray<{0}>",
                        propertyType.IsGenericType
                            ? GetTypeScriptTypeName(propertyType.GenericTypeArguments[0], ownerType, settings, true)
                            : "any");
                }
                else
                {
                    tsType = string.Format("KnockoutObservable<{0}>", tsType);
                }
            }

            return tsType;
        }
    }

    static class StringExtensions
    {
        public static string ToCamelCase(this string str)
        {
            if (!string.IsNullOrEmpty(str))
            {
                str = str.Substring(0, 1).ToLowerInvariant() + str.Substring(1);
            }
            return str;
        }
    }
}

using System;
using System.Collections;
using System.IO;
using System.Text;

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

        public static bool TryGetTypeScriptNamespaceName(this Type type, Settings settings, out string outputString)
        {
            var result = true;
            try
            {
                outputString = (settings.tsRootNamespace + type.Namespace.Remove(0, settings.clrRootNamespace.Length));
            }
            catch(Exception)
            {
                result = false;
                outputString = null;
            }
            return result;
        }

        //public static string GetRelativeNamespaceName(this Type propertyType, Type ownerType, Settings settings)
        //{
        //    return propertyType.GetTypeScriptNamespaceName(settings);
        //}

        public static string GetTypeScriptTypeName(this Type propertyType, bool isNullableType, Type ownerType, Settings settings, bool skipKnockoutObservableWrapper = false)
        {
            if (propertyType.IsNullableType())
            {
                throw new Exception("Nullable types should be unwrapped before this point");
            }
            //if (isNullableType)
            //{
            //    propertyType = propertyType.GetGenericArguments()[0];
            //}

            string tsType = null;

            if (IsNumericType(propertyType))
            {
                tsType = "number";
                if (settings.strictNullChecks && isNullableType)
                {
                    tsType += " | null";
                }
            }
            else if (propertyType == typeof(DateTime) || propertyType == typeof(DateTimeOffset))
            {
                tsType = "Date";
                if (settings.strictNullChecks && isNullableType)
                {
                    tsType += " | null";
                }
            }
            else if (propertyType == typeof(bool))
            {
                tsType = "boolean";
                if (settings.strictNullChecks && isNullableType)
                {
                    tsType += " | null";
                }
            }
            else if (propertyType == typeof(Guid) || propertyType == typeof(string))
            {
                tsType = "string";
                if (settings.strictNullChecks)
                {
                    tsType += " | null";
                }
            }
            else if (propertyType == typeof(byte[]) || propertyType == typeof(byte))
            {
                tsType = "string";
                if (settings.strictNullChecks && isNullableType)
                {
                    tsType += " | null";
                }
            }
            else if (propertyType.IsEnum)
            {
                if (settings.enumType == "enum" || settings.enumType == "stringliteral")
                {
                    string propertyTypeTsNs;
                    if (propertyType.TryGetTypeScriptNamespaceName(settings, out propertyTypeTsNs))
                    {
                        tsType = $"{propertyTypeTsNs}.{propertyType.Name}";
                    }
                    else
                    {
                        tsType = "Object";
                    }
                }
                else
                {
                    tsType = settings.enumType;
                }

                if (settings.strictNullChecks && isNullableType)
                {
                    tsType += " | null";

                }

            }
            else if (typeof(IEnumerable).IsAssignableFrom(propertyType))
            {
                var itemType = propertyType.GenericTypeArguments[0];
                var itemTypeIsNullable = itemType.IsNullableType();
                if (itemTypeIsNullable)
                {
                    itemType = itemType.GetGenericArguments()[0];
                }

                tsType = string.Format(
                    "{0}[]",
                    propertyType.IsGenericType
                        ? GetTypeScriptTypeName(itemType, itemTypeIsNullable, ownerType, settings)
                        : "any");
                if (settings.strictNullChecks)
                {
                    tsType += " | null";
                }
            }
            else if (propertyType.IsClass || propertyType.IsInterface)
            {
                string propertyTypeTsNs;
                if (propertyType.TryGetTypeScriptNamespaceName(settings, out propertyTypeTsNs))
                {
                    tsType = $"{propertyTypeTsNs}.{propertyType.Name}";
                }
                else
                {
                    tsType = "Object";
                }
                if (settings.strictNullChecks)
                {
                    tsType += " | null";
                }
            }

            if (settings.useKnockout && !skipKnockoutObservableWrapper)
            {
                if (typeof(IEnumerable).IsAssignableFrom(propertyType) && !propertyType.IsEnum && propertyType != typeof(string))
                {
                    var itemType = propertyType.GenericTypeArguments[0];
                    tsType = string.Format(
                        "KnockoutObservableArray<{0}>",
                        propertyType.IsGenericType
                            ? GetTypeScriptTypeName(itemType, false, ownerType, settings, true)
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
   
        public static string GetRelativePathTo(this string absPath, string relTo)
        {
            absPath = Path.GetDirectoryName(absPath);

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
            StringBuilder relativePath = new StringBuilder();
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
            relativePath.Append(relDirs[relDirs.Length - 1]);
            return relativePath.ToString();
        }
    }
}

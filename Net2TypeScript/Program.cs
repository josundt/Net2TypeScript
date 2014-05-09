using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;

namespace jasMIN.Net2TypeScript
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            int result = 0;

            Console.WriteLine("Converting .NET entities to TypeScript interfaces.");

            try
            {
                var settings = GetSettingsFromJson();
                MergeCmdArgsWithSettings(args, settings);
                settings.Validate();
 
                var sb = new StringBuilder();
                sb.AddModule(settings);

                File.WriteAllText(settings.outputPath, sb.ToString(), Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: " + ex.Message);
                result = -1;
            }

            if (Debugger.IsAttached)
            {
                Console.WriteLine();
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }

            return result;
        }

        private static Settings GetSettingsFromJson()
        {
            string settingsPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "settings.json");

            if (!File.Exists(settingsPath)) { throw new FileNotFoundException("Settings file ('settings.json') not found."); }

            string jsonSettings = File.ReadAllText(settingsPath, Encoding.UTF8);

            var deserializer = new JavaScriptSerializer();
            var settings = (Settings)deserializer.Deserialize(jsonSettings, typeof(Settings));

            return settings;
        }

        private static void MergeCmdArgsWithSettings(string[] args, Settings settings)
        {
            if (args.Length % 2 != 0)
            {
                throw new ArgumentException("Wrong command line arguments.");
            }

            for (int i = 0; i < args.Length; i = i + 2)
            {
                if (!args[i].StartsWith("--"))
                {
                    throw new ArgumentException(string.Format("Unknown command line argument: '{0}'", args[i]));
                }

                string settingsProp = args[i].Substring(2);

                PropertyInfo propInfo = typeof(Settings).GetProperties().SingleOrDefault(pi => pi.Name == settingsProp);

                if (propInfo == null)
                {
                    throw new ArgumentException(string.Format("Unknown command line argument: '{0}'", args[i]));
                }
                else
                {
                    if (propInfo.PropertyType == typeof(bool?) || propInfo.PropertyType == typeof(bool))
                    {
                        bool boolValue = false;
                        if (!bool.TryParse(args[i + 1], out boolValue))
                        {
                            throw new ArgumentException(string.Format("Not a boolean value: '{0}'", args[i + 1]));
                        }
                        else
                        {
                            propInfo.SetValue(settings, boolValue);
                        }
                    }
                    else if (propInfo.PropertyType == typeof(string))
                    {
                        propInfo.SetValue(settings, args[i + 1]);
                    }
                }

            }
        }
    }

    internal static class Extensions
    {
        public static string ToCamelCase(this string str)
        {
            if (!string.IsNullOrEmpty(str))
            {
                str = str.Substring(0, 1).ToLowerInvariant() + str.Substring(1);
            }
            return str;
        }

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
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof (Nullable<>);
        }

        public static string GetTypeScriptTypeName(this Type propertyType, Settings settings, bool skipKnockoutObservableWrapper = false)
        {
            var isNullableType = propertyType.IsNullableType();
            if (isNullableType)
            {
                propertyType = propertyType.GetGenericArguments()[0];
            }

            string tsType = "UNDEFINED";

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
                if (settings.enumType != "enum")
                {
                    tsType = settings.enumType;
                }
                else 
                {
                    tsType = propertyType.Name;
                }
            }
            else if (typeof(IEnumerable).IsAssignableFrom(propertyType))
            {
                tsType = string.Format(
                    "{0}[]",
                    propertyType.IsGenericType
                        ? GetTypeScriptTypeName(propertyType.GenericTypeArguments[0], settings)
                        : "any");
            }
            else if (propertyType.IsClass || propertyType.IsInterface)
            {
                tsType = propertyType.Name;
            }
            else
            {
                var test = "hei";
            }

            if (settings.useKnockout && !skipKnockoutObservableWrapper)
            {
                if (typeof(IEnumerable).IsAssignableFrom(propertyType) && !propertyType.IsEnum && propertyType != typeof(string))
                {
                    tsType = string.Format(
                        "KnockoutObservableArray<{0}>",
                        propertyType.IsGenericType
                            ? GetTypeScriptTypeName(propertyType.GenericTypeArguments[0], settings, true)
                            : "any");
                }
                else
                {
                    tsType = string.Format("KnockoutObservable<{0}>", tsType);
                }
            }

            return tsType;
        }

        public static void Validate(this Settings settings)
        {
            // TODO: Validate that settings object has the expected properties

            if (!File.Exists(settings.assemblyPath))
            {
                throw new FileNotFoundException(string.Format("Assembly '{0}' not found.", settings.assemblyPath));
            }
            var outputDir = Path.GetDirectoryName(settings.outputPath);
            if (outputDir == null || !Directory.Exists(outputDir))
            {
                throw new FileNotFoundException(string.Format("Output directory '{0}' not found.", settings.outputPath));
            }

        }

        private static void AddDefinitelyTypedReferences(this StringBuilder sb, Settings settings)
        {
            if (settings.useKnockout)
            {
                sb.AppendFormat("/// <reference path=\"{0}\"/>\r\n", Path.Combine(settings.definitelyTypedRelPath, "knockout/knockout.d.ts"));
            }
            if (settings.useBreeze)
            {
                sb.AppendFormat("/// <reference path=\"{0}\"/>\r\n", Path.Combine(settings.definitelyTypedRelPath, "breeze/breeze.d.ts"));
            }
        }

        public static void AddModule(this StringBuilder sb, Settings settings)
        {
            sb.AddDefinitelyTypedReferences(settings);

            sb.AppendFormat("declare module {0} {{\r\n", settings.moduleName);

            Assembly assembly = Assembly.LoadFrom(settings.assemblyPath);
            assembly.GetReferencedAssemblies();

            List<Type> classTypes = assembly.GetTypes()
                .Where(t => t.IsClass && t.IsPublic && t.Namespace.StartsWith(settings.rootNamespace))
                .ToList();
            classTypes.Sort(delegate(Type type1, Type type2) { return type1.Name.CompareTo(type2.Name); });

            foreach (Type classType in classTypes)
            {
                sb.AddClass(classType, settings);
            }

            //if (settings.enumType == "enum")
            //{
            //    List<Type> enumTypes = assembly.GetTypes().Where(t => t.IsEnum && t.IsPublic && t.Namespace.StartsWith(settings.rootNamespace)).ToList();
            //    enumTypes.Sort(delegate(Type type1, Type type2) { return type1.Name.CompareTo(type2.Name); });

            //    foreach (Type enumType in enumTypes)
            //    {
            //        sb.AddEnum(enumType, settings);
            //    }
            //}

            //Console.Write(sb);

            sb.AppendLine("}");
        }

        public static void AddClass(this StringBuilder sb, Type classType, Settings settings)
        {
            if (!classType.IsClass || !classType.IsPublic)
            {
                throw new InvalidOperationException("Not a class type");
            }

            sb.AppendFormat("\r\n{0}/** Class: {1}.{2} ({3}) */\r\n", 
                settings.tab, 
                settings.moduleName, 
                classType.Name, 
                classType.FullName);
            
            sb.AppendFormat("{0}interface {1}{2} {{\r\n", 
                settings.tab, 
                classType.Name,
                settings.useBreeze ? " extends breeze.Entity" : string.Empty);

            // TODO: Filter non-public props
            foreach (PropertyInfo propertyInfo in classType.GetProperties())
            {
                sb.AddProperty(propertyInfo, settings);
            }

            if (settings.globalExtensions != null)
            {
                foreach (var prop in settings.globalExtensions)
                {
                    var tsPropName = prop.ToString().Substring(1, prop.ToString().Length - 2).Split(',')[0].Trim();
                    var tsTypeName = prop.ToString().Substring(1, prop.ToString().Length - 2).Split(',')[1].Trim();

                    sb.AppendFormat("{0}{1}: {2};\r\n",
                                    settings.tab + settings.tab,
                                    tsPropName,
                                    tsTypeName);

                }
            }

            sb.AppendFormat("{0}}}\r\n", settings.tab);
        }

        public static void AddEnum(this StringBuilder sb, Type enumType, Settings settings)
        {
            // PS ! Not used or working
            if (settings.enumType == "enum")
            {
                sb.AppendFormat(
                    "/** {0} **/\r\n",
                    enumType.FullName);

                sb.AppendFormat(
                    "declare enum {0} {{\r\n",
                    enumType.Name);

                var valueIterator = 0;
                var enumKeys = Enum.GetNames(enumType);
                var enumValues = Enum.GetValues(enumType);
                foreach (object enumValue in enumValues)
                {
                    sb.AppendFormat(
                        "{0}{1} = {2}{3}\r\n",
                        settings.tab,
                        enumKeys[valueIterator].ToCamelCase(),
                        Convert.ChangeType(enumValue, enumType.GetEnumUnderlyingType()),
                        valueIterator == enumKeys.Length - 1 ? null : ",");

                    valueIterator++;
                }

                sb.AppendLine("}\r\n");
            }


        }

        private static void AddProperty(this StringBuilder sb, PropertyInfo propertyInfo, Settings settings)
        {
            Type propertyType = propertyInfo.PropertyType;

            var isNullableType = propertyType.IsNullableType();
            if (isNullableType)
            {
                propertyType = propertyType.GetGenericArguments()[0];
            }

            var typeScriptTypeName = propertyType.GetTypeScriptTypeName(settings);

            var faultyProperty = false;

            if (typeScriptTypeName.Contains("UNDEFINED"))
            {
                faultyProperty = true;
                Console.WriteLine("WARNING: Unconvertable type: {0}", propertyType.FullName);
            }

            if (!faultyProperty)
            {
                if (propertyType.IsEnum)
                {
                    sb.AppendFormat(
                        "{0}/** Enum{1}: {2} ({3}) */\r\n",
                        settings.tab + settings.tab,
                        isNullableType ? " (NULLABLE)" : null,
                        propertyType.Name.ToCamelCase(),
                        propertyType.FullName);
                }
                else if(isNullableType)
                {
                    sb.AppendFormat(
                        "{0}/** NULLABLE */\r\n",
                        settings.tab + settings.tab);
                }

                sb.AppendFormat(
                    "{0}{1}: {2};\r\n",
                    settings.tab + settings.tab,
                    settings.camelCase ? propertyInfo.Name.ToCamelCase() : propertyInfo.Name,
                    propertyType.GetTypeScriptTypeName(settings));
            }

        }
    }

}

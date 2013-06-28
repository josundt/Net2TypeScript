using System;
using System.Collections;
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
        private static void Main(string[] args)
        {
            if (!File.Exists("settings.json"))
            {
                throw new FileNotFoundException("Settings file ('settings.json') not found.");
            }

            string jsonSettings;
            using (var streamReader = new StreamReader("settings.json", Encoding.UTF8))
            {
                jsonSettings = streamReader.ReadToEnd();
                streamReader.Close();
            }
            var serializer = new JavaScriptSerializer();
            var settings = (Settings)serializer.Deserialize(jsonSettings, typeof(Settings));

            ValidateSettings(settings);

            Assembly assembly = Assembly.LoadFrom(settings.assemblyPath);

            var sb = new StringBuilder();

            AddDefinitelyTypedReferences(sb, settings);

            sb.AppendLine();

            foreach (Type classType in assembly.GetTypes().Where(t => t.IsClass && t.IsPublic && t.Namespace.StartsWith(settings.rootNamespace)))
            {
                AddClass(classType, sb, settings);
                //sb.AppendLine();
            }

            Console.Write(sb);

            File.WriteAllText(settings.outputPath, sb.ToString(), Encoding.UTF8);

            if (Debugger.IsAttached)
            {
                Console.WriteLine();
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }

        private static void AddDefinitelyTypedReferences(StringBuilder sb, Settings settings)
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
        
        private static void AddClass(Type classType, StringBuilder sb, Settings settings)
        {
            if (!classType.IsClass || !classType.IsPublic)
            {
                throw new InvalidOperationException("Not a class type");
            }

            sb.AppendFormat("interface {0} {{\r\n", classType.Name);

            // TODO: Filter non-public props
            foreach (PropertyInfo propertyInfo in classType.GetProperties())
            {
                AddProperty(propertyInfo, sb, settings);
            }

            if (settings.useBreeze)
            {
                sb.AppendFormat("{0}entityAspect: breeze.EntityAspect;\r\n", settings.tab);
                sb.AppendFormat("{0}entityType: breeze.EntityType;\r\n", settings.tab);
            }

            sb.AppendLine("}");
        }
        
        private static void AddProperty(PropertyInfo propertyInfo, StringBuilder sb, Settings settings)
        {
            Type propertyType = propertyInfo.PropertyType;

            var typeScriptTypeName = GetTypeScriptTypeName(propertyType, settings);

            var faultyProperty = false;

            if (typeScriptTypeName.Contains("UNKNOWN"))
            {
                faultyProperty = true;   
                Console.WriteLine("WARNING: Unconvertable type: {0}", propertyType.FullName);
            } 

            if (!faultyProperty)
            {
                sb.AppendFormat(
                    "{0}{1}: {2};\r\n",
                    settings.tab,
                    settings.camelCase ? ToCamelCase(propertyInfo.Name) : propertyInfo.Name,
                    GetTypeScriptTypeName(propertyType, settings));
            }

        }
       
        private static void ValidateSettings(Settings settings)
        {
            // TODO: Validate that settings object has the expected properties

            if (!File.Exists(settings.assemblyPath))
            {
                throw new FileNotFoundException(string.Format("Assembly '{0}' not found.", settings.assemblyPath));
            }
            var outputDir = Path.GetDirectoryName(settings.outputPath);
            if(outputDir == null || !Directory.Exists(outputDir))
            {
                throw new FileNotFoundException(string.Format("Output directory '{0}' not found.", settings.outputPath));
            }

        }
        private static string ToCamelCase(string str)
        {
            if (!string.IsNullOrEmpty(str))
            {
                str = str.Substring(0, 1).ToLowerInvariant() + str.Substring(1);
            }
            return str;
        }
        
        private static string GetTypeScriptTypeName(Type propertyType, Settings settings, bool skipKnockoutObservableWrapper = false)
        {
            string tsType = "UNKNOWN";

            if (IsNumericType(propertyType))
            {
                tsType = "number";
            }
            else if (propertyType == typeof(DateTime) || propertyType == typeof(DateTime?) || propertyType == typeof(DateTimeOffset) || propertyType == typeof(DateTimeOffset?))
            {
                tsType = "Date";
            }
            else if (propertyType == typeof (bool) || propertyType == typeof (bool?))
            {
                tsType = "boolean";
            }
            else if (propertyType == typeof(Guid) || propertyType == typeof(Guid?) || propertyType == typeof(string))
            {
                tsType = "string";
            }
            else if (propertyType.IsEnum)
            {
                tsType = settings.enumType;
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
                if (typeof (IEnumerable).IsAssignableFrom(propertyType))
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

        private static bool IsNumericType(Type type)
        {
            return
                type == typeof (short) || type == typeof (short?) ||
                type == typeof (int) || type == typeof (int?) ||
                type == typeof (long) || type == typeof (long?) ||
                type == typeof (decimal) || type == typeof (decimal?) ||
                type == typeof (float) || type == typeof (float?) ||
                type == typeof (double) || type == typeof (double?);
        }
    }

}

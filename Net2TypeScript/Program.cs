using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;
using jasMIN.Net2TypeScript.Model;

namespace jasMIN.Net2TypeScript
{
    internal class Program
    {
        static int Main(string[] args)
        {
            int result = 0;

            Console.WriteLine("Converting .NET entities to TypeScript interfaces.");

            //try
            //{
                var settings = GetSettingsFromJson();
                MergeCmdArgsWithSettings(args, settings);
                settings.Validate();

                var ts = TypeScriptGenerator.GenerateTypeScript(settings);

                File.WriteAllText(settings.outputPath, ts, Encoding.UTF8);
                
                //var sb = new StringBuilder();
                //sb.AddRootNamespace(settings);

                //File.WriteAllText(settings.outputPath, sb.ToString(), Encoding.UTF8);

            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine("ERROR: " + ex.Message);
            //    result = -1;
            //}

            if (Debugger.IsAttached)
            {
                Console.WriteLine();
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }

            return result;
        }

        static Settings GetSettingsFromJson()
        {
            string settingsPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "settings.json");

            if (!File.Exists(settingsPath)) { throw new FileNotFoundException("Settings file ('settings.json') not found."); }

            string jsonSettings = File.ReadAllText(settingsPath, Encoding.UTF8);

            var deserializer = new JavaScriptSerializer();
            var settings = (Settings)deserializer.Deserialize(jsonSettings, typeof(Settings));

            return settings;
        }

        static void MergeCmdArgsWithSettings(string[] args, Settings settings)
        {
            if (args.Length % 2 != 0)
            {
                throw new ArgumentException("Wrong command line arguments.");
            }

            for (int i = 0; i < args.Length; i = i + 2)
            {
                if (!args[i].StartsWith("--", StringComparison.Ordinal))
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

    static class StringBuilderExtension
    {
        static void AddDefinitelyTypedReferences(this StringBuilder sb, Settings settings)
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

        public static void AddRootNamespace(this StringBuilder sb, Settings settings)
        {
            sb.AddDefinitelyTypedReferences(settings);

            sb.AppendLine($"declare namespace {settings.tsRootNamespace} {{");

            Assembly assembly = Assembly.LoadFrom(settings.assemblyPath);
            assembly.GetReferencedAssemblies();

            List<Type> classTypes = assembly.GetTypes()
                .Where(t => t.IsClass && t.IsPublic && t.Namespace.StartsWith(settings.clrRootNamespace, StringComparison.Ordinal))
                .ToList();

            classTypes.Sort(delegate(Type type1, Type type2) { return string.Compare(type1.Name, type2.Name, StringComparison.Ordinal); });

            foreach (Type classType in classTypes)
            {
                sb.AddClass(classType, settings);
            }

            // Adding enum types
            if(settings.enumType == "enum" || settings.enumType == "stringliteral")
            {
                sb.AppendLine();
                sb.AppendLine($"{settings.tab}namespace Enums {{");

                List<Type> enumTypes = assembly.GetTypes().Where(t => t.IsEnum && t.IsPublic /* && t.Namespace.StartsWith(settings.rootNamespace, StringComparison.Ordinal)*/).ToList();
                enumTypes.Sort(delegate (Type type1, Type type2) { return string.Compare(type1.Name, type2.Name, StringComparison.Ordinal); });

                foreach (Type enumType in enumTypes)
                {
                    sb.AddEnum(enumType, settings);
                }

                sb.AppendLine();
                sb.AppendLine($"{settings.tab}}}");
            }

            sb.AppendLine();
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
                settings.tsRootNamespace, 
                classType.Name, 
                classType.FullName);
            
            sb.AppendFormat("{0}interface {1}{2} {{\r\n", 
                settings.tab, 
                classType.Name,
                settings.useBreeze ? " extends breeze.Entity" : string.Empty);

            // TODO: Filter non-public props
            foreach (PropertyInfo propertyInfo in classType.GetProperties())
            {
                sb.AddProperty(propertyInfo, classType, settings);
            }

            if (settings.globalExtensions != null)
            {
                foreach (KeyValuePair<string, object> prop in settings.globalExtensions)
                {
                    AddExtensionProperty(sb, prop, settings);
                }
            }

            if (settings.perTypeExtensions != null)
            {
                foreach (KeyValuePair<string, object> typeExtensionProp in settings.perTypeExtensions)
                {
                    var targetClassType = typeExtensionProp.ToString().Substring(1, typeExtensionProp.ToString().Length - 2).Split(',')[0].Trim();
                    if (classType.Name.Equals(targetClassType))
                    {
                        foreach (var prop in (Dictionary<string, object>)typeExtensionProp.Value)
                        {
                            AddExtensionProperty(sb, prop, settings);
                        }
                    }
                }
            }

            sb.AppendLine($"{settings.tab}}}");
        }

        public static void AddEnum(this StringBuilder sb, Type enumType, Settings settings)
        {
            if (settings.enumType == "stringliteral")
            {
                sb.AppendLine();

                sb.AppendLine($"{settings.tab}{settings.tab}/** {enumType.FullName} **/");

                var legalValues = string.Join("|",
                        enumType
                            .GetMembers(BindingFlags.Public | BindingFlags.Static)
                            .Select(m => $@"""{m.Name}"""));

                sb.AppendLine($"{settings.tab}{settings.tab}type {enumType.Name} = {legalValues};");

            }

            // PS ! Not used or working
            //if (settings.enumType == "enum")
            //{
            //    sb.AppendFormat(
            //        "/** {0} **/\r\n",
            //        enumType.FullName);

            //    sb.AppendFormat(
            //        "declare enum {0} {{\r\n",
            //        enumType.Name);

            //    var valueIterator = 0;
            //    var enumKeys = Enum.GetNames(enumType);
            //    var enumValues = Enum.GetValues(enumType);
            //    foreach (object enumValue in enumValues)
            //    {
            //        sb.AppendFormat(
            //            "{0}{1} = {2}{3}\r\n",
            //            settings.tab,
            //            enumKeys[valueIterator].ToCamelCase(),
            //            Convert.ChangeType(enumValue, enumType.GetEnumUnderlyingType()),
            //            valueIterator == enumKeys.Length - 1 ? null : ",");

            //        valueIterator++;
            //    }

            //    sb.AppendLine("}\r\n");
            //}


        }

        static void AddProperty(this StringBuilder sb, PropertyInfo propertyInfo, Type ownerType, Settings settings)
        {
            Type propertyType = propertyInfo.PropertyType;

            var isNullableType = propertyType.IsNullableType();
            if (isNullableType)
            {
                propertyType = propertyType.GetGenericArguments()[0];
            }

            var typeScriptTypeName = propertyType.GetTypeScriptTypeName(ownerType, settings);

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
                else if (isNullableType)
                {
                    sb.AppendLine($"{settings.tab}{settings.tab}/** NULLABLE */");
                }

                sb.AppendFormat(
                    "{0}{1}: {2};\r\n",
                    settings.tab + settings.tab,
                    settings.camelCase ? propertyInfo.Name.ToCamelCase() : propertyInfo.Name,
                    propertyType.GetTypeScriptTypeName(ownerType, settings));
            }

        }

        static void AddExtensionProperty(this StringBuilder sb, KeyValuePair<string, object> prop, Settings settings)
        {
            var tsPropName = prop.Key;
            var tsTypeName = prop.Value.ToString();

            sb.AppendFormat("{0}{1}: {2};\r\n",
                            settings.tab + settings.tab,
                            tsPropName,
                            tsTypeName);


        }
    }

}

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Text;
using jasMIN.Net2TypeScript.Model;
using System.Collections.Generic;

namespace jasMIN.Net2TypeScript
{
    internal class Program
    {
        static int Main(string[] args)
        {
            int result = 0;

            Console.WriteLine("Converting .NET entities to TypeScript interfaces.");

            try
            {
                var settingsMap = ArgsToDictionary(args);

                string settingsFile = null;
                if (settingsMap.ContainsKey("settings")) {
                    settingsFile = settingsMap["settings"];
                    settingsMap.Remove("settings");
                }
                var settings = TypeScriptGenerator.GetSettingsFromJson(settingsFile);

                MergeCmdArgsWithSettings(settingsMap, settings);

                TypeScriptGenerator.ValidateSettings(settings);

                var generatorResult = TypeScriptGenerator.GenerateTypeScript(settings);

                if (!string.IsNullOrEmpty(generatorResult.Declarations))
                {
                    File.WriteAllText(settings.declarationsOutputPath, generatorResult.Declarations, Encoding.UTF8);
                }
                if (!string.IsNullOrEmpty(generatorResult.EnumModel))
                {
                    File.WriteAllText(settings.modelModuleOutputPath, generatorResult.EnumModel, Encoding.UTF8);
                }
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

        static Dictionary<string, string> ArgsToDictionary(string[] args)
        {
            var dict = new Dictionary<string, string>();

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

                dict.Add(args[i].Substring(2), args[i + 1]);
            }

            return dict;
        }

        static void MergeCmdArgsWithSettings(Dictionary<string, string> settingsMap, Settings settings)
        {
            foreach (var kvp in settingsMap)
            {
                PropertyInfo propInfo = typeof(Settings).GetProperties().SingleOrDefault(pi => pi.Name == kvp.Key);

                if (propInfo == null)
                {
                    throw new ArgumentException(string.Format("Unknown command line argument: '{0}'", kvp.Key));
                }
                else
                {
                    if (propInfo.PropertyType == typeof(bool?) || propInfo.PropertyType == typeof(bool))
                    {
                        bool boolValue = false;
                        if (!bool.TryParse(kvp.Value, out boolValue))
                        {
                            throw new ArgumentException(string.Format("Not a boolean value: '{0}'", kvp.Value));
                        }
                        else
                        {
                            propInfo.SetValue(settings, boolValue);
                        }
                    }
                    else if (propInfo.PropertyType == typeof(string))
                    {
                        propInfo.SetValue(settings, kvp.Value);
                    }
                }

            }
        }
    }
}

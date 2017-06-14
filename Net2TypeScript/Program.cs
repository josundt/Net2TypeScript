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
                var settingsFilePath = ExtractSettingsFilePath(settingsMap);
                var globalSettings = TypeScriptGenerator.GetGlobalSettingsFromJson(settingsFilePath);

                MergeCmdArgsWithSettings(settingsMap, globalSettings);

                TypeScriptGenerator.NormalizeAndValidateSettings(globalSettings, Path.GetDirectoryName(settingsFilePath));

                var output = TypeScriptGenerator.GenerateTypeScript(globalSettings);

                if (!string.IsNullOrEmpty(output))
                {
                    File.WriteAllText(globalSettings.outputPath, output, Encoding.UTF8);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: " + ex.Message);
#if DEBUG
                Console.WriteLine(ex.StackTrace);
#endif
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

        static string ExtractSettingsFilePath(Dictionary<string, string> settingsMap)
        {
            string path = null;
            if (settingsMap.ContainsKey("settings"))
            {
                path = settingsMap["settings"];
                settingsMap.Remove("settings");
            }
            var cwd = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            if (path == null)
            {
                path = Path.Combine(cwd, "settings.json");
            }
            else if (!Path.IsPathRooted(path)) // If filename only
            {
                path = Path.Combine(cwd, path);
            }
            return path;
        }
    }
}

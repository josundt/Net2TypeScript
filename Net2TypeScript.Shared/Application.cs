using jasMIN.Net2TypeScript.Shared.SettingsModel;
using System.Diagnostics;
using System.Reflection;

namespace jasMIN.Net2TypeScript.Shared;

public static class Application
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
    public static int Main(string[] args)
    {
        if (args is null)
        {
            throw new ArgumentNullException(nameof(args));
        }

        int result = 0;

        Console.WriteLine("Converting .NET entities to TypeScript interfaces.");

        try
        {
            var settingsMap = ArgsToDictionary(args);
            var settingsFilePath = ExtractSettingsFilePath(settingsMap);
            var globalSettings = TypeScriptGenerator.GetGlobalSettingsFromJson(settingsFilePath);

            MergeCmdArgsWithSettings(settingsMap, globalSettings);

            TypeScriptGenerator.NormalizeAndValidateSettings(globalSettings, Path.GetDirectoryName(settingsFilePath)!);

            using var outStream = File.Create(globalSettings.OutputPath);
            TypeScriptGenerator.GenerateTypeScript(globalSettings, outStream);
        }
        catch (Exception ex)
        {
            Console.WriteLine("ERROR: " + ex.Message);
//#if DEBUG
            Console.WriteLine(ex.StackTrace);
//#endif
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

        for (int i = 0; i < args.Length; i += 2)
        {
            if (!args[i].StartsWith("--", StringComparison.Ordinal))
            {
                throw new ArgumentException($"Unknown command line argument: '{args[i]}'");
            }

            dict.Add(args[i][2..], args[i + 1]);
        }

        return dict;
    }

    static void MergeCmdArgsWithSettings(Dictionary<string, string> settingsMap, Settings settings)
    {
        foreach (var kvp in settingsMap)
        {
            PropertyInfo? propInfo = typeof(Settings).GetProperties().SingleOrDefault(pi => pi.Name == kvp.Key);

            if (propInfo == null)
            {
                throw new ArgumentException($"Unknown command line argument: '{kvp.Key}'");
            }
            else
            {
                if (propInfo.PropertyType == typeof(bool?) || propInfo.PropertyType == typeof(bool))
                {
                    if (!bool.TryParse(kvp.Value, out bool boolValue))
                    {
                        throw new ArgumentException($"Not a boolean value: '{kvp.Value}'");
                    }
                    else
                    {
                        propInfo.SetValue(settings, false);
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
        string? path = null;
        if (settingsMap.ContainsKey("settings"))
        {
            path = settingsMap["settings"];
            settingsMap.Remove("settings");
        }
        var cwd = AppContext.BaseDirectory;
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

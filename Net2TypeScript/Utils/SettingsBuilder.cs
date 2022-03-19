using jasMIN.Net2TypeScript.SettingsModel;
using System.Reflection;
using System.Text;

namespace jasMIN.Net2TypeScript.Utils;

internal static class SettingsBuilder
{
    public static GlobalSettings BuildGlobalSettings(string[] commandLineArgs)
    {
        var settingsMap = ArgsToDictionary(commandLineArgs);
        var settingsFilePath = ExtractSettingsFilePath(settingsMap);
        var buildConfiguration = ExtractBuildConfiguration(settingsMap);
        var globalSettings = GetGlobalSettingsFromJson(settingsFilePath);

        MergeCmdArgsWithSettings(settingsMap, globalSettings);

        NormalizeAndValidateSettings(globalSettings, Path.GetDirectoryName(settingsFilePath)!, buildConfiguration);

        return globalSettings;
    }

    private static Dictionary<string, string> ArgsToDictionary(string[] args)
    {
        var dict = new Dictionary<string, string>();

        if (args.Length % 2 != 0)
        {
            throw new ArgumentException("Wrong command line arguments.");
        }

        for (int i = 0; i < args.Length; i += 2)
        {
            if (args[i].StartsWith("--", StringComparison.Ordinal))
            {
                dict.Add(args[i][2..], args[i + 1]);
            }
            else if (args[i].StartsWith("-", StringComparison.Ordinal))
            {
                dict.Add(args[i][1..], args[i + 1]);
            }
            else
            {
                throw new ArgumentException($"Unknown command line argument: '{args[i]}'");
            }

        }

        return dict;
    }

    private static void MergeCmdArgsWithSettings(Dictionary<string, string> settingsMap, Settings settings)
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

    private static string ExtractSettingsFilePath(Dictionary<string, string> settingsMap)
    {
        string? path = null;
        if (settingsMap.ContainsKey("s"))
        {
            path = settingsMap["s"];
            settingsMap.Remove("s");
        }
        if (settingsMap.ContainsKey("settings"))
        {
            path ??= settingsMap["settings"];
            settingsMap.Remove("settings");
        }
        var cwd = Directory.GetCurrentDirectory(); // AppContext.BaseDirectory -> Executable directory;
        if (path == null)
        {
            path = Path.Combine(cwd, "settings.json");
        }
        else if (!Path.IsPathRooted(path)) // If filename only
        {
            path = Path.GetFullPath(Path.Combine(cwd, path));
        }
        return path;
    }

    private static string ExtractBuildConfiguration(Dictionary<string, string> settingsMap)
    {
        string? result = null;
        if (settingsMap.ContainsKey("c"))
        {
            result= settingsMap["c"];
            settingsMap.Remove("c");
        }
        if (settingsMap.ContainsKey("configuration"))
        {
            result ??= settingsMap["configuration"];
            settingsMap.Remove("configuration");
        }
        return result ?? "Debug";
    }

    private static GlobalSettings GetGlobalSettingsFromJson(string settingsPath)
    {
        if (!File.Exists(settingsPath)) { throw new FileNotFoundException($@"Settings file ""{settingsPath}"" not found."); }

        string jsonSettings = File.ReadAllText(settingsPath, Encoding.UTF8);

        var settings = JsonUtil.Deserialize<GlobalSettings>(jsonSettings)!;

        return settings;
    }

    private static void NormalizeAndValidateSettings(GlobalSettings settings, string cwd, string buildConfiguration)
    {
        if (cwd is null)
        {
            throw new ArgumentNullException(nameof(cwd));
        }
        // TODO: Validate that settings object has the expected properties


        settings.AssemblyPaths = settings.AssemblyPaths.Select(ap => ResolvePath(ap, cwd, buildConfiguration)).ToList();
        settings.OutputPath = ResolvePath(settings.OutputPath, cwd, buildConfiguration);

        ValidateGeneratorSettings(settings);
        settings.NamespaceOverrides.ToList().ForEach(kvp => ValidateGeneratorSettings(kvp.Value));
        settings.ClassOverrides.ToList().ForEach(kvp => ValidateGeneratorSettings(kvp.Value));

        var missingAssemblyPaths = settings.AssemblyPaths.Where(ap => !File.Exists(ap));
        if (missingAssemblyPaths.Any())
        {
            throw new FileNotFoundException($"Assemblies not found: {string.Join(", ", missingAssemblyPaths)}");
        }

        var outputPath = Path.GetDirectoryName(settings.OutputPath);
        if (outputPath == null || !Directory.Exists(outputPath))
        {
            throw new FileNotFoundException($"Output directory '{settings.OutputPath}' not found.");
        }
    }

    private static void ValidateGeneratorSettings(GeneratorSettings genSettings)
    {
        if (!new[] {
                    null,
                    KnockoutMappingOptions.None,
                    KnockoutMappingOptions.All,
                    KnockoutMappingOptions.ValueTypes
                }.Any(s => s == genSettings.KnockoutMapping)
        )
        {
            throw new InvalidOperationException($"Invalid knockoutMappingOptions value: {genSettings.KnockoutMapping}");
        }
    }

    private static string ResolvePath(string path, string cwd, string buildConfiguration = null)
    {
        path = path.Replace("/", "\\", StringComparison.Ordinal);

        if (buildConfiguration != null)
        {
            path = path.Replace("$(buildconfiguration)", buildConfiguration, StringComparison.OrdinalIgnoreCase);
        }

        if (!Path.IsPathRooted(path))
        {
            path = Path.GetFullPath(Path.Combine(cwd, path));
        }
        return path;
    }


}

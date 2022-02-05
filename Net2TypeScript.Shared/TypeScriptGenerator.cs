using jasMIN.Net2TypeScript.Shared.DotNetModel;
using jasMIN.Net2TypeScript.Shared.SettingsModel;
using System.Reflection;
using System.Text;

namespace jasMIN.Net2TypeScript.Shared;

static class TypeScriptGenerator
{
    public static void GenerateTypeScript(GlobalSettings globalSettings, Stream outStream)
    {

        var ns = new NamespaceModel(globalSettings.DotNetRootNamespace, new NullabilityInfoContext(), globalSettings);

        using var sw = new StreamWriter(outStream);

        WriteEslintDisables(sw);

        WriteTypesReferences(sw, globalSettings);

        var indent = WriteTsRootNamespacesOpen(sw, globalSettings);

        ns.WriteTs(sw, indent);

        WriteTsRootNamespacesClose(sw, indent, globalSettings);

    }

    public static GlobalSettings GetGlobalSettingsFromJson(string settingsPath)
    {
        if (!File.Exists(settingsPath)) { throw new FileNotFoundException($@"Settings file ""{settingsPath}"" not found."); }

        string jsonSettings = File.ReadAllText(settingsPath, Encoding.UTF8);

        var settings = JsonUtil.Deserialize<GlobalSettings>(jsonSettings)!;

        return settings;
    }

    public static void NormalizeAndValidateSettings(GlobalSettings settings, string cwd)
    {
        if (cwd is null)
        {
            throw new ArgumentNullException(nameof(cwd));
        }
        // TODO: Validate that settings object has the expected properties


        settings.AssemblyPaths = settings.AssemblyPaths.Select(ap => ResolvePath(ap, cwd)).ToList();
        settings.OutputPath = ResolvePath(settings.OutputPath, cwd);

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

    private static string ResolvePath(string path, string cwd)
    {
        path = path.Replace("/", "\\", StringComparison.Ordinal);

        if (!Path.IsPathRooted(path))
        {
            path = Path.GetFullPath(Path.Combine(cwd, path));
        }
        return path;
    }

    private static void WriteEslintDisables(StreamWriter sw)
    {
        var eslintDisables = new[]
{
            "@typescript-eslint/no-unnecessary-qualifier",
            "@typescript-eslint/array-type"
        };
        sw.Write($"/* eslint-disable {string.Join(", ", eslintDisables)} */");
    }

    private static void WriteTypesReferences(StreamWriter sw, GlobalSettings globalSettings)
    {
        List<GeneratorSettings> allGeneratorSettings = new()
        {
            globalSettings
        };
        allGeneratorSettings.AddRange(globalSettings.NamespaceOverrides.Values);
        allGeneratorSettings.AddRange(globalSettings.ClassOverrides.Values);

        if (
            allGeneratorSettings.Any(gs => gs.KnockoutMapping != KnockoutMappingOptions.None && gs.KnockoutMapping != null) &&
            globalSettings.TypingsPaths?.Knockout != null
        )
        {
            sw.WriteFormat(
                "/// <reference path=\"{0}\"/>{1}",
                globalSettings.TypingsPaths.Knockout,
                Environment.NewLine
            );
        }

        if (
            allGeneratorSettings.Any(gs => gs.UseBreeze == true) &&
            globalSettings.TypingsPaths?.Breeze != null
        )
        {
            sw.WriteFormat(
                "/// <reference path=\"{0}\"/>{1}",
                globalSettings.TypingsPaths.Breeze,
                Environment.NewLine
            );
        }

    }

    private static int WriteTsRootNamespacesOpen(StreamWriter sw, GlobalSettings globalSettings)
    {

        var i = 0;

        if (!string.IsNullOrEmpty(globalSettings.TsRootNamespace))
        {
            foreach (var nsPart in globalSettings.TsRootNamespace.Split('.'))
            {
                sw.WriteFormat(
                    "{2}{0}export namespace {1} {{{2}",
                    string.Concat(Enumerable.Repeat(globalSettings.Indent, i)),
                    nsPart,
                    Environment.NewLine
                );
                i++;
            }
        }

        return i;

    }

    private static void WriteTsRootNamespacesClose(StreamWriter sw, int indentCount, GlobalSettings globalSettings)
    {
        if (!string.IsNullOrEmpty(globalSettings.TsRootNamespace))
        {

            for (; indentCount >= 0; --indentCount)
            {
                sw.WriteFormat(
                    "{0}}}{1}",
                    string.Concat(Enumerable.Repeat(globalSettings.Indent, indentCount)),
                    Environment.NewLine
                );
            }
        }
    }

}

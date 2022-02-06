using jasMIN.Net2TypeScript.DotNetModel;
using jasMIN.Net2TypeScript.SettingsModel;
using jasMIN.Net2TypeScript.Utils;
using System.Reflection;

namespace jasMIN.Net2TypeScript;

internal class Generator
{
    private GlobalSettings GlobalSettings { get; }

    public Generator(GlobalSettings globalSettings)
    {
        this.GlobalSettings = globalSettings;
    }

    public void GenerateTypeScript(Func<Stream> openwriteStream)
    {
        var ns = new Namespace(this.GlobalSettings.DotNetRootNamespace, new NullabilityInfoContext(), this.GlobalSettings);

        using var sw = new StreamWriter(openwriteStream());

        WriteEslintDisables(sw);

        WriteTypesReferences(sw, this.GlobalSettings);

        var indent = WriteTsRootNamespacesOpen(sw, this.GlobalSettings);

        ns.WriteTs(sw, indent);

        WriteTsRootNamespacesClose(sw, indent, this.GlobalSettings);

    }

    private static void WriteEslintDisables(StreamWriter sw)
    {
        var eslintDisables = new[]
        {
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

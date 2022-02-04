using jasMIN.Net2TypeScript.Shared.Model;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace jasMIN.Net2TypeScript.Shared
{
    static class TypeScriptGenerator
    {
        public static void GenerateTypeScript(GlobalSettings globalSettings, Stream outStream)
        {
            var ns = new NamespaceModel(globalSettings, globalSettings.ClrRootNamespace);

            var eslintDisables = new[]
            {
                "@typescript-eslint/no-unnecessary-qualifier",
                "@typescript-eslint/array-type",
                "@typescript-eslint/indent",
                "@typescript-eslint/no-unnecessary-qualifier"
            };

            using var sw = new StreamWriter(outStream);

            sw.Write($"/* eslint-disable {string.Join(", ", eslintDisables)} */");

            ns.WriteTs(sw);

        }

        public static GlobalSettings GetGlobalSettingsFromJson(string settingsPath)
        {
            if (!File.Exists(settingsPath)) { throw new FileNotFoundException($@"Settings file ""{settingsPath}"" not found."); }

            string jsonSettings = File.ReadAllText(settingsPath, Encoding.UTF8);

            var settings = Json.Deserialize<GlobalSettings>(jsonSettings);

            return settings;
        }

        public static void NormalizeAndValidateSettings(GlobalSettings settings, string cwd)
        {
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
                throw new FileNotFoundException(string.Format("Output directory '{0}' not found.", settings.OutputPath));
            }
        }

        static void ValidateGeneratorSettings(GeneratorSettings genSettings)
        {
            if (!new[] {
                    null,
                    KnockoutMappingOptions.None,
                    KnockoutMappingOptions.All,
                    KnockoutMappingOptions.ValueTypes
                }.Any(s => s == genSettings.KnockoutMapping)
            )
            {
                throw new Exception($"Invalid knockoutMappingOptions value: {genSettings.KnockoutMapping}");
            }
        }

        static string ResolvePath(string path, string cwd)
        {
            path = path.Replace("/", "\\");

            if (!Path.IsPathRooted(path))
            {
                path = Path.GetFullPath(Path.Combine(cwd, path));
            }
            return path;
        }
    }
}

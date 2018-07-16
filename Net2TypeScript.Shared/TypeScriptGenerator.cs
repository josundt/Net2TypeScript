using jasMIN.Net2TypeScript.Shared.Model;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace jasMIN.Net2TypeScript.Shared
{
    static class TypeScriptGenerator
    {
        public static string GenerateTypeScript(GlobalSettings globalSettings)
        {
            var ns = new NamespaceModel(globalSettings, globalSettings.ClrRootNamespace);

            var tslintDisables = new[] {
                "no-unnecessary-qualifier",
                "array-type",
                "no-shadowed-variable"
            };

            var sb = new StringBuilder();

            sb.Append(string.Join("\r\n", tslintDisables.Select(td => $"// tslint:disable:{td}")));

            ns.AppendTs(sb);

            sb.Append(string.Join("", tslintDisables.Select(td => $"// tslint:enable:{td}\r\n")));

            return sb.ToString();
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

            if (settings.AssemblyPaths.Any(ap => !File.Exists(ap)))
            {
                throw new FileNotFoundException("Some of the specified assemblies could not be found.");
            }

            var outputPath = Path.GetDirectoryName(settings.OutputPath);
            if (outputPath == null || !Directory.Exists(outputPath))
            {
                throw new FileNotFoundException(string.Format("Output directory '{0}' not found.", settings.OutputPath));
            }
        }

        static void ValidateGeneratorSettings(GeneratorSettings genSettings) {
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

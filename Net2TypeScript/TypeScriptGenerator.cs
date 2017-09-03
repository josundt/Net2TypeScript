using jasMIN.Net2TypeScript.Model;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.Script.Serialization;

namespace jasMIN.Net2TypeScript
{
    static class TypeScriptGenerator
    {
        public static string GenerateTypeScript(GlobalSettings globalSettings)
        {
            var ns = new NamespaceModel(globalSettings, globalSettings.clrRootNamespace);

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

            var deserializer = new JavaScriptSerializer();
            var settings = (GlobalSettings)deserializer.Deserialize(jsonSettings, typeof(GlobalSettings));

            return settings;
        }

        public static void NormalizeAndValidateSettings(GlobalSettings settings, string cwd)
        {
            // TODO: Validate that settings object has the expected properties

            settings.assemblyPaths = settings.assemblyPaths.Select(ap => ResolvePath(ap, cwd)).ToList();
            settings.outputPath = ResolvePath(settings.outputPath, cwd);

            ValidateGeneratorSettings(settings);
            settings.namespaceOverrides.ToList().ForEach(kvp => ValidateGeneratorSettings(kvp.Value));
            settings.classOverrides.ToList().ForEach(kvp => ValidateGeneratorSettings(kvp.Value));

            if (settings.assemblyPaths.Any(ap => !File.Exists(ap)))
            {
                throw new FileNotFoundException("Some of the specified assemblies could not be found.");
            }

            var outputPath = Path.GetDirectoryName(settings.outputPath);
            if (outputPath == null || !Directory.Exists(outputPath))
            {
                throw new FileNotFoundException(string.Format("Output directory '{0}' not found.", settings.outputPath));
            }
        }

        static void ValidateGeneratorSettings(GeneratorSettings genSettings) {
            if (!new[] {
                    null,
                    KnockoutMappingOptions.None, 
                    KnockoutMappingOptions.All,
                    KnockoutMappingOptions.ValueTypes
                }.Any(s => s == genSettings.knockoutMapping)
            )
            {
                throw new Exception($"Invalid knockoutMappingOptions value: {genSettings.knockoutMapping}");
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

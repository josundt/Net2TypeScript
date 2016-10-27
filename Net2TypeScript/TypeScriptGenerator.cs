﻿using jasMIN.Net2TypeScript.Model;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.Script.Serialization;

namespace jasMIN.Net2TypeScript
{
    static class TypeScriptGenerator
    {
        public static GeneratorResult GenerateTypeScript(GlobalSettings globalSettings)
        {
            var ns = new NamespaceModel(globalSettings, globalSettings.clrRootNamespace);
            var sbDeclarations = new StringBuilder();
            var sbEnums = new StringBuilder();
            ns.AppendTs(sbDeclarations);
            ns.AppendEnums(sbEnums);
            return new GeneratorResult {
                Declarations = sbDeclarations.ToString(),
                EnumModel = sbEnums.ToString()
            };
        }

        public static GlobalSettings GetGlobalSettingsFromJson(string settingsPath)
        {
            if (!File.Exists(settingsPath)) { throw new FileNotFoundException($@"Settings file ""{settingsPath}"" not found."); }

            string jsonSettings = File.ReadAllText(settingsPath, Encoding.UTF8);

            var deserializer = new JavaScriptSerializer();
            var settings = (GlobalSettings)deserializer.Deserialize(jsonSettings, typeof(GlobalSettings));

            return settings;
        }

        public static void NormalizeAndValidateSettings(Settings settings, string cwd)
        {
            // TODO: Validate that settings object has the expected properties

            settings.assemblyPaths = settings.assemblyPaths.Select(ap => ResolvePath(ap, cwd)).ToList();
            settings.declarationsOutputPath = ResolvePath(settings.declarationsOutputPath, cwd);
            settings.modelModuleOutputPath = ResolvePath(settings.modelModuleOutputPath, cwd);

            if (settings.assemblyPaths.Any(ap => !File.Exists(ap)))
            {
                throw new FileNotFoundException("Some of the specified assemblies could not be found.");
            }

            var declarationsOutputDir = Path.GetDirectoryName(settings.declarationsOutputPath);
            if (declarationsOutputDir == null || !Directory.Exists(declarationsOutputDir))
            {
                throw new FileNotFoundException(string.Format("Output directory '{0}' not found.", settings.declarationsOutputPath));
            }

            var enumModelOutputDir = Path.GetDirectoryName(settings.modelModuleOutputPath);
            if (enumModelOutputDir == null || !Directory.Exists(enumModelOutputDir))
            {
                throw new FileNotFoundException(string.Format("Output directory '{0}' not found.", settings.modelModuleOutputPath));
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

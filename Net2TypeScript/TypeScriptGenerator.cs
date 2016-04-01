using jasMIN.Net2TypeScript.Model;
using System.IO;
using System.Reflection;
using System.Text;
using System.Web.Script.Serialization;

namespace jasMIN.Net2TypeScript
{
    static class TypeScriptGenerator
    {
        public static GeneratorResult GenerateTypeScript(Settings settings = null)
        {
            settings = settings ?? GetSettingsFromJson();
            var ns = new NameSpaceModel(settings, settings.clrRootNamespace);
            var sbDeclarations = new StringBuilder();
            var sbEnums = new StringBuilder();
            ns.AppendTs(sbDeclarations);
            ns.AppendEnums(sbEnums);
            return new GeneratorResult {
                Declarations = sbDeclarations.ToString(),
                EnumModel = sbEnums.ToString()
            };
        }

        public static Settings GetSettingsFromJson(string settingsPath = null)
        {
            settingsPath = settingsPath ?? Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "settings.json");

            if (!File.Exists(settingsPath)) { throw new FileNotFoundException("Settings file ('settings.json') not found."); }

            string jsonSettings = File.ReadAllText(settingsPath, Encoding.UTF8);

            var deserializer = new JavaScriptSerializer();
            var settings = (Settings)deserializer.Deserialize(jsonSettings, typeof(Settings));

            ValidateSettings(settings);

            return settings;
        }

        public static void ValidateSettings(Settings settings)
        {
            // TODO: Validate that settings object has the expected properties

            if (!File.Exists(settings.assemblyPath))
            {
                throw new FileNotFoundException(string.Format("Assembly '{0}' not found.", settings.assemblyPath));
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
    }
}

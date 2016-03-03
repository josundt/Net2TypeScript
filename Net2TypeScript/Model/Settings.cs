using System.IO;

namespace jasMIN.Net2TypeScript.Model
{
    public class Settings
    {
        public Settings()
        {
            // Initializing default values
            this.tab = "    ";
            this.enumType = "stringliteral";
            this.definitelyTypedRelPath = @"..\typings\";
        }
        public string assemblyPath { get; set; }
        public string outputPath { get; set; }
        public string clrRootNamespace { get; set; }
        public string tsRootNamespace { get; set; }
        public string tab { get; set; }
        public string enumType { get; set; }
        public bool camelCase { get; set; }
        public string definitelyTypedRelPath { get; set; }
        public bool useKnockout { get; set; }
        public bool useBreeze { get; set; }
        public dynamic globalExtensions { get; set; }
        public dynamic perTypeExtensions { get; set; }
    }

    static class SettingsExtensions
    {
        public static void Validate(this Settings settings)
        {
            // TODO: Validate that settings object has the expected properties

            if (!File.Exists(settings.assemblyPath))
            {
                throw new FileNotFoundException(string.Format("Assembly '{0}' not found.", settings.assemblyPath));
            }
            var outputDir = Path.GetDirectoryName(settings.outputPath);
            if (outputDir == null || !Directory.Exists(outputDir))
            {
                throw new FileNotFoundException(string.Format("Output directory '{0}' not found.", settings.outputPath));
            }

        }
    }
}

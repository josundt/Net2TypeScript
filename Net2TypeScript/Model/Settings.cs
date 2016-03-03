using System.Collections.Generic;
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
        public string declarationsOutputPath { get; set; }
        public string enumModelOutputPath { get; set; }
        public string clrRootNamespace { get; set; }
        public List<string> classNamespaceFilter { get; set; }
        public List<string> enumNamespaceFilter { get; set; }
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

}

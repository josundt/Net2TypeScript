using System.Collections.Generic;
using System.IO;

namespace jasMIN.Net2TypeScript.Model
{
    public class Settings
    {
        public Settings()
        {
            // Initializing default values
            this.indent = "    ";
            this.enumType = "stringliteral";
            this.definitelyTypedRelPath = @"..\typings\";
            this.tsRootNamespace = this.clrRootNamespace;
        }
        public string assemblyPath { get; set; }
        public string declarationsOutputPath { get; set; }
        public string modelModuleOutputPath { get; set; }
        public string clrRootNamespace { get; set; }
        public string tsRootNamespace { get; set; }
        public string enumType { get; set; }
        public List<string> classNamespaceFilter { get; set; }
        public List<string> enumNamespaceFilter { get; set; }
        public string indent { get; set; }
        public bool camelCase { get; set; }
        public bool strictNullChecks { get; set; }
        public string definitelyTypedRelPath { get; set; }
        public bool useKnockout { get; set; }
        public bool useBreeze { get; set; }
        public dynamic globalExtensions { get; set; }
        public dynamic perTypeExtensions { get; set; }
    }

}

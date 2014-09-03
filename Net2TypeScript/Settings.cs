using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace jasMIN.Net2TypeScript
{
    public class Settings
    {
        public string assemblyPath { get; set; }
        public string rootNamespace { get; set; }
        public string tab { get; set; }
        public string enumType { get; set; }
        public bool camelCase { get; set; }
        public string definitelyTypedRelPath { get; set; }
        public bool useKnockout { get; set; }
        public bool useBreeze { get; set; }
        public dynamic globalExtensions { get; set; }
        public dynamic perTypeExtensions { get; set; }
        public string moduleName { get; set; }
        public string outputPath { get; set; }
    }
}

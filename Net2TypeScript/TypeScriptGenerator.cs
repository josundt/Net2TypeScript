using jasMIN.Net2TypeScript.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace jasMIN.Net2TypeScript
{
    static class TypeScriptGenerator
    {
        public static string GenerateTypeScript(Settings settings)
        {
            var ns = new NameSpaceModel(settings, settings.clrRootNamespace);
            var sb = new StringBuilder();
            ns.AppendTs(sb);
            return sb.ToString();
        }
    }
}

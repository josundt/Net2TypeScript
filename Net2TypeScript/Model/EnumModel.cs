using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace jasMIN.Net2TypeScript.Model
{
    class EnumModel : ClrTypeModelBase
    {
        public EnumModel(GlobalSettings globalSettings, Type type)
            : base(type, globalSettings)
        {
            if (!type.IsEnum || !type.IsPublic)
            {
                throw new InvalidOperationException("Not a public enum type");
            }

        }

        protected override int ExtraIndents => 1;

        IEnumerable<string> Names
        {
            get
            {
                return Enum.GetNames(typeof(Type));
            }
        }

        Dictionary<string, string> Values
        {
            get
            {
                var result = new Dictionary<string, string>();
                foreach(var rawValue in Enum.GetValues(this.Type))
                {
                    var name = Enum.GetName(this.Type, rawValue);

                    var value = this._globalSettings.enumType == "string" 
                        ? $@"""{name}"""
                        : Convert.ChangeType(rawValue, Enum.GetUnderlyingType(this.Type)).ToString();

                    result.Add(name, value);
                }
                return result;
            }
        }
        public override void AppendTs(StringBuilder sb) {

            sb.AppendLine($"{IndentationContext}export enum {TsTypeName} {{");

            var i = 0;
            foreach (var value in this.Values)
            {
                var possiblyComma = i < this.Values.Count - 1 ? "," : string.Empty;
                sb.AppendLine($@"{IndentationContext}{Settings.indent}{value.Key} = {value.Value}{possiblyComma}");
                i++;
            }

            sb.AppendLine($@"{IndentationContext}}}");

        }
    }
}

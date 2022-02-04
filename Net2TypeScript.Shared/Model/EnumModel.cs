using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace jasMIN.Net2TypeScript.Shared.Model
{
    [DebuggerDisplay("enum: {Type.Name,nq}")]
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
                    var enumOutputSetting = this._globalSettings.EnumType;

                    bool useNumeric = enumOutputSetting != "string";

                    if (enumOutputSetting == "stringIfNotFlagEnum") {
                        useNumeric = this.Type.GetCustomAttributes(typeof(FlagsAttribute), false)?.Length > 0;
                    }

                    var value = useNumeric
                        ? Convert.ChangeType(rawValue, Enum.GetUnderlyingType(this.Type)).ToString()
                        : $@"""{name}""";

                    result.Add(name, value);
                }
                return result;
            }
        }

        public override StreamWriter WriteTs(StreamWriter sw) {

            sw.WriteFormat("{2}{0}/** .NET enum: {1} */{2}",
                this.IndentationContext,
                this.ClrFullName,
                Environment.NewLine
            );

            sw.WriteFormat("{0}export enum {1} {{{2}",
                this.IndentationContext,
                this.TsTypeName,
                Environment.NewLine
            );

            var i = 0;
            foreach (var value in this.Values)
            {
                var possiblyComma = i < this.Values.Count - 1 ? "," : string.Empty;
                sw.WriteLine($@"{this.IndentationContext}{this.Settings.Indent}{value.Key} = {value.Value}{possiblyComma}");
                i++;
            }

            sw.WriteLine($@"{this.IndentationContext}}}");

            return sw;

        }
    }
}

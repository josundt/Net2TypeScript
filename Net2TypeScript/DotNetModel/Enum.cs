using jasMIN.Net2TypeScript.SettingsModel;
using jasMIN.Net2TypeScript.Utils;
using System.Globalization;

namespace jasMIN.Net2TypeScript.DotNetModel;

#if DEBUG
[System.Diagnostics.DebuggerDisplay($"{nameof(Enum)}: {{{nameof(Name)}}}, {nameof(Values)}: {{{nameof(Values)}.Count}}")]
#endif
class Enum : DotNetTypeModelBase
{
    public Enum(Type type, GlobalSettings globalSettings)
        : base(type, globalSettings)
    {
        if (!type.IsEnum || !type.IsPublic)
        {
            throw new InvalidOperationException("Not a public enum type");
        }

    }

    Dictionary<string, string> Values
    {
        get
        {
            var result = new Dictionary<string, string>();
            foreach (var rawValue in System.Enum.GetValues(this._type))
            {
                var name = System.Enum.GetName(this._type, rawValue);

                if (name == null)
                {
                    throw new InvalidOperationException($"Could not get enum name: {this._type.FullName ?? string.Empty}");
                }

                var enumOutputSetting = this._globalSettings.EnumType;

                bool useNumeric = enumOutputSetting != "string";

                if (enumOutputSetting == "stringIfNotFlagEnum")
                {
                    useNumeric = this._type.GetCustomAttributes(typeof(FlagsAttribute), false)?.Length > 0;
                }

                var value = useNumeric
                    ? Convert.ChangeType(rawValue, System.Enum.GetUnderlyingType(this._type), CultureInfo.InvariantCulture).ToString()!
                    : $@"""{name}""";

                result.Add(name, value);
            }
            return result;
        }
    }

    public override StreamWriter WriteTs(StreamWriter sw, int indentCount)
    {

        var indent = this.Indent(indentCount);

        sw.WriteFormat("{2}{0}/** .NET enum: {1} */{2}",
            indent,
            this.FullName,
            sw.NewLine
        );

        var i = 0;
        if (this.Settings.EnumFormat == "enum")
        {
            sw.WriteFormat("{0}export enum {1} {{{2}",
                indent,
                this.TsTypeName,
                sw.NewLine
            );

            foreach (var value in this.Values)
            {
                var possiblyComma = i < this.Values.Count - 1 ? "," : string.Empty;
                sw.WriteLine($@"{indent}{this.Settings.Indent}{value.Key} = {value.Value}{possiblyComma}");
                i++;
            }

            sw.WriteLine($@"{indent}}}");
        }
        else if (this.Settings.EnumFormat == "unionType")
        {
            sw.WriteFormat("{0}export type {1} =",
                indent,
                this.TsTypeName
            );

            var maxPerLine = 8;
            foreach (var kvp in this.Values)
            {
                if (i != 0 && i % maxPerLine == 0)
                {
                    sw.WriteFormat("{0}{1}",
                        sw.NewLine,
                        indent
                    );
                }
                else
                {
                    sw.Write(" ");
                }
                var possiblyPipe = (i < this.Values.Count - 1) ?
                    " |" : string.Empty;
                sw.WriteFormat("{0}{1}",
                    kvp.Value,
                    possiblyPipe
                );
                i++;
            }
            sw.WriteLine(";");
        }

        return sw;

    }
}

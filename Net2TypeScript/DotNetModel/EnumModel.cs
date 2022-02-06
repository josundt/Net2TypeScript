using jasMIN.Net2TypeScript.SettingsModel;
using jasMIN.Net2TypeScript.Utils;
using System.Diagnostics;
using System.Globalization;

namespace jasMIN.Net2TypeScript.DotNetModel;

[DebuggerDisplay($"enum: {{{nameof(Name)},nq}}")]
class EnumModel : DotNetTypeModelBase
{
    public EnumModel(Type type, GlobalSettings globalSettings)
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
            foreach (var rawValue in Enum.GetValues(this._type))
            {
                var name = Enum.GetName(this._type, rawValue);

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
                    ? Convert.ChangeType(rawValue, Enum.GetUnderlyingType(this._type), CultureInfo.InvariantCulture).ToString()!
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
            Environment.NewLine
        );

        sw.WriteFormat("{0}export enum {1} {{{2}",
            indent,
            this.TsTypeName,
            Environment.NewLine
        );

        var i = 0;
        foreach (var value in this.Values)
        {
            var possiblyComma = i < this.Values.Count - 1 ? "," : string.Empty;
            sw.WriteLine($@"{indent}{this.Settings.Indent}{value.Key} = {value.Value}{possiblyComma}");
            i++;
        }

        sw.WriteLine($@"{indent}}}");

        return sw;

    }
}

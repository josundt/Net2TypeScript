using System;
using System.Linq;
using System.Reflection;
using System.Text;

namespace jasMIN.Net2TypeScript.Model
{
    class EnumModel : ClrTypeModelBase
    {
        public EnumModel(Settings settings, Type type)
            : base(type, settings)
        {
            if (!type.IsEnum || !type.IsPublic)
            {
                throw new InvalidOperationException("Not a public enum type");
            }

        }

        protected override int ExtraIndents => 1;

        string TsStringLiteralType
        {
            get
            {
                return string.Join("|", Type
                                            .GetMembers(BindingFlags.Public | BindingFlags.Static)
                                            .Select(m => $@"""{m.Name}"""));
            }
        }

        public override void AppendTs(StringBuilder sb) {

            if (Settings.enumType == "stringliteral")
            {
                sb.AppendLine();

                sb.AppendFormat(
                    "{0}/** Enum: {1} ({2}) */\r\n",
                    Indent,
                    TsFullName,
                    ClrTypeName);

                sb.AppendLine($"{Indent}type {TsTypeName} = {TsStringLiteralType};");

            }

            // PS ! Not used or working
            //if (settings.enumType == "enum")
            //{
            //    sb.AppendFormat(
            //        "/** {0} **/\r\n",
            //        enumType.FullName);

            //    sb.AppendFormat(
            //        "declare enum {0} {{\r\n",
            //        enumType.Name);

            //    var valueIterator = 0;
            //    var enumKeys = Enum.GetNames(enumType);
            //    var enumValues = Enum.GetValues(enumType);
            //    foreach (object enumValue in enumValues)
            //    {
            //        sb.AppendFormat(
            //            "{0}{1} = {2}{3}\r\n",
            //            settings.tab,
            //            enumKeys[valueIterator].ToCamelCase(),
            //            Convert.ChangeType(enumValue, enumType.GetEnumUnderlyingType()),
            //            valueIterator == enumKeys.Length - 1 ? null : ",");

            //        valueIterator++;
            //    }

            //    sb.AppendLine("}\r\n");
            //}

        }
    }
}

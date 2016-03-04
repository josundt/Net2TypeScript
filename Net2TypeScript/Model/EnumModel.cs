using System;
using System.Collections.Generic;
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

        List<string> StringValues
            => Type.GetMembers(BindingFlags.Public | BindingFlags.Static).Select(t => t.Name).ToList();

        string TsStringLiteralType
            => string.Join("|", StringValues.Select(s => $@"""{s}"""));

        public override void AppendTs(StringBuilder sb) {

            if (Settings.enumType == "stringliteral")
            {
                sb.AppendLine();

                sb.AppendFormat(
                    "{0}/** Enum: {1} ({2}) */\r\n",
                    IndentationContext,
                    TsFullName,
                    ClrFullName);

                sb.AppendLine($"{IndentationContext}type {TsTypeName} = {TsStringLiteralType};");

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

        public void AppendEnums(StringBuilder sb)
        {
            if (Settings.enumType == "stringliteral")
            {
                sb.AppendLine();

                sb.AppendFormat(
                    "{0}/** Enum: {1} ({2}) */\r\n",
                    IndentationContext,
                    TsFullName,
                    ClrFullName);

                sb.AppendLine($"{IndentationContext}export namespace {TsTypeName} {{");
                sb.AppendLine($@"{IndentationContext}{Settings.indent}export namespace Values {{");
                foreach (var stringValue in StringValues)
                {
                    sb.AppendLine($@"{IndentationContext}{Settings.indent}{Settings.indent}export const {stringValue}: {TsFullName} = ""{stringValue}"";");
                }
                sb.AppendLine($@"{IndentationContext}{Settings.indent}}}");
                sb.AppendLine($"{IndentationContext}}}");

            }
        }
    }
}

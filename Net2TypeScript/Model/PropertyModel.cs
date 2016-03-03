using System;
using System.Linq;
using System.Reflection;
using System.Text;

namespace jasMIN.Net2TypeScript.Model
{
    class PropertyModel : ClrTypeModelBase
    {
        public PropertyModel(Settings settings, PropertyInfo propertyInfo, Type ownerType)
            : base(
                  propertyInfo.PropertyType.IsNullableType() ? propertyInfo.PropertyType.GetGenericArguments()[0] : propertyInfo.PropertyType, 
                  settings)
        {
            this.PropInfo = propertyInfo;
            this.OwnerType = ownerType;
            this.IsNullableType = propertyInfo.PropertyType.IsNullableType();

            if (TsTypeName == null)
            {
                Console.WriteLine("WARNING: Unconvertable type: {0}", Type.FullName);
            }
        }

        protected override string TsTypeName => Type.GetTypeScriptTypeName(this.OwnerType, Settings);
                        
        string TsName => Settings.camelCase ? PropInfo.Name.ToCamelCase() : PropInfo.Name;

        protected override string Indent =>
            string.Concat(Enumerable.Repeat(Settings.tab, OwnerType.Namespace.Split('.').Length - Settings.clrRootNamespace.Split('.').Length + ExtraIndents));

        PropertyInfo PropInfo { get; set; }
        Type OwnerType { get; set; }
        protected override int ExtraIndents => 2;
        bool IsNullableType { get; set; }

        public override void AppendTs(StringBuilder sb)
        {
            if (TsTypeName != null)
            {
                if (Type.IsEnum)
                {
                    sb.AppendFormat(
                        "{0}/** Enum{1}: {2} ({3}) */\r\n",
                        Indent,
                        IsNullableType ? " (NULLABLE)" : null,
                        TsName,
                        ClrTypeName);
                }
                else if (IsNullableType)
                {
                    sb.AppendLine($"{Indent}/** NULLABLE */");
                }

                sb.AppendLine(
                    $"{Indent}{TsName}: {TsTypeName};");
            }
        }
    }
}

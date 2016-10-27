using System;
using System.Linq;
using System.Reflection;
using System.Text;

namespace jasMIN.Net2TypeScript.Model
{
    class PropertyModel : ClrTypeModelBase
    {
        public PropertyModel(GlobalSettings globalSettings, PropertyInfo propertyInfo, Type ownerType)
            : base(
                  propertyInfo.PropertyType.IsNullableType() ? propertyInfo.PropertyType.GetGenericArguments()[0] : propertyInfo.PropertyType, 
                  globalSettings)
        {
            this.PropInfo = propertyInfo;
            this.OwnerType = ownerType;
            this.IsNullableType = propertyInfo.PropertyType.IsNullableType();
        }

        protected override string TsTypeName => 
            Type.GetTypeScriptTypeName(this.IsNullableType, this.OwnerType, Settings);

        protected override string IndentationContext =>
            string.Concat(Enumerable.Repeat(Settings.indent, OwnerType.Namespace.Split('.').Length - Settings.clrRootNamespace.Split('.').Length + ExtraIndents));

        protected override int ExtraIndents => 
            2;

        string TsName =>
            Settings.camelCase ? PropInfo.Name.ToCamelCase() : PropInfo.Name;

        PropertyInfo PropInfo { get; set; }

        public Type OwnerType { get; private set; }

        bool IsNullableType { get; set; }

        public override void AppendTs(StringBuilder sb)
        {
            if (TsTypeName != null)
            {
                if (Type.IsEnum)
                {
                    sb.AppendFormat(
                        "{0}/** Enum{1}: {2} ({3}) */\r\n",
                        IndentationContext,
                        IsNullableType ? " (NULLABLE)" : null,
                        TsFullName,
                        ClrFullName);
                }
                else if (IsNullableType)
                {
                    sb.AppendLine($"{IndentationContext}/** Nullable */");
                }

                sb.AppendLine(
                    $"{IndentationContext}{TsName}: {TsTypeName};");
            }
        }
    }
}

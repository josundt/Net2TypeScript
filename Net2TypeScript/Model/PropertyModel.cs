using System;
using System.Linq;
using System.Reflection;
using System.Text;

namespace jasMIN.Net2TypeScript.Model
{
    class PropertyModel : ClrTypeModelBase
    {
        public PropertyModel(GlobalSettings globalSettings, PropertyInfo propertyInfo)
            : base(
                  propertyInfo.PropertyType.IsNullableType() ? propertyInfo.PropertyType.GetGenericArguments()[0] : propertyInfo.PropertyType, 
                  globalSettings)
        {
            this.PropInfo = propertyInfo;

            if (!(propertyInfo.CanRead || propertyInfo.GetGetMethod().IsPublic))
            {
                throw new Exception($"Property {PropInfo.DeclaringType.FullName}.{PropInfo.Name} is not readable.");
            }

        }

        PropertyInfo PropInfo { get; set; }

        public Type DeclaringType => 
            PropInfo.DeclaringType;

        bool IsNullableType =>
            PropInfo.PropertyType.IsNullableType();

        bool IsReadOnly =>
            !(PropInfo.CanWrite && PropInfo.GetSetMethod() != null && PropInfo.GetSetMethod().IsPublic);

        bool IsObjectType =>
            (PropInfo.PropertyType.IsClass || PropInfo.PropertyType.IsInterface) && PropInfo.PropertyType != typeof(string);

        bool IsKnockoutObservable =>
            Settings.knockoutMapping != null && (Settings.knockoutMapping == KnockoutMappingOptions.All || (Settings.knockoutMapping == KnockoutMappingOptions.ValueTypes && !this.IsObjectType));

        string AccessModifier =>
            IsReadOnly && !IsKnockoutObservable ? "readonly " : "";

        protected override int ExtraIndents =>
            2;

        string TsName =>
            Settings.camelCase ? PropInfo.Name.ToCamelCase() : PropInfo.Name;

        protected override string TsTypeName => 
            Type.GetTypeScriptTypeName(IsNullableType, DeclaringType, Settings);

        protected override string IndentationContext =>
            string.Concat(Enumerable.Repeat(Settings.indent, DeclaringType.Namespace.Split('.').Length - Settings.clrRootNamespace.Split('.').Length + ExtraIndents));

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
                    $"{IndentationContext}{AccessModifier}{TsName}: {TsTypeName};");
            }
        }
    }
}

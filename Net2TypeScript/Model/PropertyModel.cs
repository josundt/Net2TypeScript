using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Text;

namespace jasMIN.Net2TypeScript.Model
{
    class PropertyModel : ClrTypeModelBase
    {
        public PropertyModel(GlobalSettings globalSettings, PropertyInfo propertyInfo, Type ownerType)
            : base(
                  propertyInfo.PropertyType.IsClrNullableType() ? propertyInfo.PropertyType.GetGenericArguments()[0] : propertyInfo.PropertyType, 
                  globalSettings)
        {
            this.PropInfo = propertyInfo;
            this.OwnerType = ownerType;
            this.TsPropInfo = new TypeScriptPropertyInfo(propertyInfo, this.Settings);

            if (!(propertyInfo.CanRead || propertyInfo.GetGetMethod().IsPublic))
            {
                throw new Exception($"Property {PropInfo.DeclaringType.FullName}.{PropInfo.Name} is not readable.");
            }

        }

        PropertyInfo PropInfo { get; set; }

        public Type OwnerType { get; private set; }

        public TypeScriptPropertyInfo TsPropInfo { get; set; }

        protected override int ExtraIndents =>
            2;

        protected override string IndentationContext =>
            string.Concat(Enumerable.Repeat(Settings.indent, OwnerType.Namespace.Split('.').Length - Settings.clrRootNamespace.Split('.').Length + ExtraIndents));

        public override void AppendTs(StringBuilder sb)
        {
            if (TsTypeName != null)
            {
                var nullUnwrappedPropertyType = PropInfo.PropertyType.IsClrNullableType() ? PropInfo.PropertyType.GetGenericArguments()[0] : PropInfo.PropertyType;

                if (Type.IsEnum)
                {
                    sb.AppendFormat(
                        "{0}/** Enum{1}: {2} */\r\n",
                        IndentationContext,
                        PropInfo.PropertyType.IsClrNullableType() ? " (NULLABLE)" : null,
                        ClrFullName);
                }
                else if (PropInfo.PropertyType.IsClrNullableType())
                {
                    sb.AppendLine($"{IndentationContext}/** Nullable */");
                }

                if (Type == typeof(Guid) || Type == typeof(Guid?))
                {
                    sb.AppendFormat(
                        "{0}/** Guid{1} */\r\n",
                        IndentationContext,
                        PropInfo.PropertyType.IsClrNullableType() ? " (NULLABLE)" : null);
                }

                sb.AppendLine(
                    $"{IndentationContext}{TsPropInfo.ToString()};");
            }
        }
    }
}

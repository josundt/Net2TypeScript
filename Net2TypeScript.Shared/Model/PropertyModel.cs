using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace jasMIN.Net2TypeScript.Shared.Model
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
            this.TsPropInfo = new TypeScriptPropertyInfo(propertyInfo, ownerType, this.Settings);

            if (!(propertyInfo.CanRead || propertyInfo.GetGetMethod().IsPublic))
            {
                throw new Exception($"Property {PropInfo.DeclaringType.FullName}.{PropInfo.Name} is not readable.");
            }

        }

        PropertyInfo PropInfo { get; set; }

        public string PropName => this.PropInfo.Name;

        public Type OwnerType { get; private set; }

        public TypeScriptPropertyInfo TsPropInfo { get; set; }

        protected override int ExtraIndents =>
            2;

        protected override string IndentationContext =>
            string.Concat(Enumerable.Repeat(Settings.Indent, OwnerType.Namespace.Split('.').Length - Settings.ClrRootNamespace.Split('.').Length + ExtraIndents));

        public override StreamWriter WriteTs(StreamWriter sw)
        {
            if (TsTypeName != null)
            {
                var nullUnwrappedPropertyType = PropInfo.PropertyType.IsClrNullableType() ? PropInfo.PropertyType.GetGenericArguments()[0] : PropInfo.PropertyType;

                if (Type.IsEnum)
                {
                    sw.WriteFormat(
                        "{0}/** Enum{1}: {2} */\r\n",
                        IndentationContext,
                        PropInfo.PropertyType.IsClrNullableType() ? " (NULLABLE)" : null,
                        ClrFullName);
                }
                else if (PropInfo.PropertyType.IsClrNullableType())
                {
                    sw.WriteLine($"{IndentationContext}/** Nullable */");
                }

                if (Type == typeof(Guid) || Type == typeof(Guid?))
                {
                    sw.WriteFormat(
                        "{0}/** Guid{1} */\r\n",
                        IndentationContext,
                        PropInfo.PropertyType.IsClrNullableType() ? " (NULLABLE)" : null);
                }

                if (Type == typeof(TimeSpan) || Type == typeof(TimeSpan?))
                {
                    sw.WriteFormat(
                        "{0}/** TimeSpan{1} */\r\n",
                        IndentationContext,
                        PropInfo.PropertyType.IsClrNullableType() ? " (NULLABLE)" : null);
                }

                sw.WriteLine(
                    $"{IndentationContext}{TsPropInfo.ToString()};");
            }

            return sw;
        }
    }
}

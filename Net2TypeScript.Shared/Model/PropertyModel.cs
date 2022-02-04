using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace jasMIN.Net2TypeScript.Shared.Model
{
    [DebuggerDisplay("prop: {PropName,nq}")]
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
                throw new Exception($"Property {this.PropInfo.DeclaringType.FullName}.{this.PropInfo.Name} is not readable.");
            }

        }

        PropertyInfo PropInfo { get; set; }

        public string PropName => this.PropInfo.Name;

        public Type OwnerType { get; private set; }

        public TypeScriptPropertyInfo TsPropInfo { get; set; }

        protected override int ExtraIndents =>
            2;

        protected override string IndentationContext =>
            string.Concat(Enumerable.Repeat(this.Settings.Indent, this.OwnerType.Namespace.Split('.').Length - this.Settings.ClrRootNamespace.Split('.').Length + this.ExtraIndents));

        public override StreamWriter WriteTs(StreamWriter sw)
        {
            if (this.TsTypeName != null)
            {
                var nullUnwrappedPropertyType = this.PropInfo.PropertyType.IsClrNullableType() ? this.PropInfo.PropertyType.GetGenericArguments()[0] : this.PropInfo.PropertyType;

                if (this.Type.IsEnum)
                {
                    sw.WriteFormat(
                        "{0}/** Enum{1}: {2} */{3}",
                        this.IndentationContext,
                        this.PropInfo.PropertyType.IsClrNullableType() ? " (NULLABLE)" : null,
                        this.ClrFullName,
                        Environment.NewLine
                    );
                }
                else if (this.PropInfo.PropertyType.IsClrNullableType())
                {
                    sw.WriteLine($"{this.IndentationContext}/** Nullable */");
                }

                if (this.Type == typeof(Guid) || this.Type == typeof(Guid?))
                {
                    sw.WriteFormat(
                        "{0}/** Guid{1} */{2}",
                        this.IndentationContext,
                        this.PropInfo.PropertyType.IsClrNullableType() ? " (NULLABLE)" : null,
                        Environment.NewLine
                    );
                }

                if (this.Type == typeof(TimeSpan) || this.Type == typeof(TimeSpan?))
                {
                    sw.WriteFormat(
                        "{0}/** TimeSpan{1} */{2}",
                        this.IndentationContext,
                        this.PropInfo.PropertyType.IsClrNullableType() ? " (NULLABLE)" : null,
                        Environment.NewLine
                    );
                }

                sw.WriteLine(
                    $"{this.IndentationContext}{this.TsPropInfo.ToString()};");
            }

            return sw;
        }
    }
}

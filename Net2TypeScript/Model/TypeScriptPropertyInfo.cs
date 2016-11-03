using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace jasMIN.Net2TypeScript.Model
{
    public interface ITypeScriptType
    {
        string TypeName { get; }
        bool IsNullable { get; }
        bool IsGeneric { get; }
        bool IsKnockoutObservable { get; }
        List<ITypeScriptType> GenericTypeArguments { get; }
        string ToString();
    }

    public class TypeScriptType : ITypeScriptType
    {
        public TypeScriptType()
        {
            this.GenericTypeArguments = new List<ITypeScriptType>();
        }

        public string TypeName { get; private set; }
        public bool IsNullable { get; private set; }
        public bool IsGeneric {
            get
            {
                return this.GenericTypeArguments.Count > 0;
            }
        }
        public bool IsKnockoutObservable { get; private set; }
        public List<ITypeScriptType> GenericTypeArguments { get; private set; }

        public static ITypeScriptType FromClrType(Type clrType, Settings settings, bool isKnockoutObservable = false)
        {

            var tsType = new TypeScriptType()
            {
                IsKnockoutObservable = isKnockoutObservable
            };

            Type propertyType = clrType;

            if (clrType.IsClrNullableType())
            {
                tsType.IsClrNullableType = true;
                propertyType = clrType.GetGenericArguments()[0];
            }

            if (propertyType.IsTypeScriptNumberType())
            {
                tsType.TypeName = "number";
            }
            else if (propertyType.IsTypeScriptDateType())
            {
                tsType.TypeName = "Date";
            }
            else if (propertyType.IsTypeScriptBoolType())
            {
                tsType.TypeName = "boolean";
            }
            else if (propertyType.IsTypeScriptStringType())
            {
                tsType.TypeName = "string";
            }
            else if (propertyType.IsEnum)
            {
                if (settings.enumType == "enum" || settings.enumType == "stringliteral")
                {
                    string propertyTypeTsNs;
                    if (propertyType.TryGetTypeScriptNamespaceName(settings, out propertyTypeTsNs))
                    {
                        tsType.TypeName = $"{propertyTypeTsNs}.{propertyType.Name}";
                    }
                    else
                    {
                        tsType.TypeName = "Object";
                    }
                }
                else
                {
                    tsType.TypeName = settings.enumType;
                }
            }
            else if (propertyType.IsTypeScriptArrayType())
            {
                var itemType = propertyType.HasElementType ? propertyType.GetElementType() : propertyType.GenericTypeArguments[0];
                tsType.TypeName = "Array";
                tsType.GenericTypeArguments.Add(FromClrType(itemType, settings));
            }
            else if (propertyType.IsTypeScriptInterfaceType())
            {
                string propertyTypeTsNs;
                if (propertyType.TryGetTypeScriptNamespaceName(settings, out propertyTypeTsNs))
                {
                    tsType.TypeName = $"{propertyTypeTsNs}.{propertyType.Name}";
                }
                else
                {
                    tsType.TypeName = "Object";
                }
            }

            tsType.IsNullable = clrType.IsClrNullableType() || propertyType.IsTypeScriptNullableType();

            if (tsType.IsKnockoutObservable)
            {
                if (propertyType.IsTypeScriptArrayType())
                {
                    tsType.TypeName = "KnockoutObservableArray";
                    tsType.IsNullable = false;
                }
                else
                {
                    tsType = tsType.ToKnockoutObservable();
                }
            }

            return tsType;

        }

        public override string ToString()
        {
            return string.Format(
                "{0}{1}{2}{3}{4}",
                this.TypeName,
                this.GenericTypeArguments.Count > 0 ? "<" : string.Empty,
                string.Join(", ", this.GenericTypeArguments.Select(tsti => tsti.ToString())),
                this.GenericTypeArguments.Count > 0 ? ">" : string.Empty,
                this.IsNullable ? " | null" : "");
        }

        bool IsClrNullableType { get; set; }

        TypeScriptType ToKnockoutObservable()
        {
            this.IsKnockoutObservable = false;
            return new TypeScriptType
            {
                IsNullable = false,
                IsKnockoutObservable = true,
                TypeName = "KnockoutObservable",
                GenericTypeArguments = new List<ITypeScriptType> { this }
            };
        }
    }

    public class TypeScriptPropertyInfo
    {
        public TypeScriptPropertyInfo(PropertyInfo propertyInfo, Settings settings)        {

            var isKnockoutObservable = false;
            if (settings.knockoutMapping != null && settings.knockoutMapping != KnockoutMappingOptions.None)
            {
                var propertyType = propertyInfo.PropertyType.IsClrNullableType() ? propertyInfo.PropertyType.GetGenericArguments()[0] : propertyInfo.PropertyType;
                if (propertyType.IsTypeScriptArrayType())
                {
                    isKnockoutObservable = true;
                }
                else
                {
                    isKnockoutObservable = 
                        settings.knockoutMapping == KnockoutMappingOptions.All
                        ||
                        (settings.knockoutMapping == KnockoutMappingOptions.ValueTypes && !propertyInfo.PropertyType.IsTypeScriptInterfaceType());
                }
            }



            this.PropertyType = TypeScriptType.FromClrType(propertyInfo.PropertyType, settings, isKnockoutObservable);
            this.PropertyName = settings.camelCase ? propertyInfo.Name.ToCamelCase() : propertyInfo.Name;
            this.ReadOnly = !(propertyInfo.CanWrite && propertyInfo.GetSetMethod() != null && propertyInfo.GetSetMethod().IsPublic);
        }
        public ITypeScriptType PropertyType { get; private set; }
        public string PropertyName { get; private set; }
        public bool ReadOnly { get; private set; }

        public override string ToString()
        {
            var typeInfo = this.PropertyType.ToString();

            return string.Format(
                "{0}{1}: {2}",
                this.ReadOnly ? "readonly " : string.Empty,
                PropertyName,
                this.PropertyType);
        }
    }
}

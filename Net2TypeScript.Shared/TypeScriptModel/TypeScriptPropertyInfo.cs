using jasMIN.Net2TypeScript.Shared.SettingsModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;

namespace jasMIN.Net2TypeScript.Shared.TypeScriptModel;


public class TypeScriptPropertyInfo
{
    public TypeScriptPropertyInfo(PropertyInfo propertyInfo, NullabilityInfoContext nullabilityContext, Settings settings)
    {
        if (propertyInfo is null)
        {
            throw new ArgumentNullException(nameof(propertyInfo));
        }

        if (nullabilityContext is null)
        {
            throw new ArgumentNullException(nameof(nullabilityContext));
        }

        if (settings is null)
        {
            throw new ArgumentNullException(nameof(settings));
        }

        var isKnockoutObservable = false;
        if (settings.KnockoutMapping != null && settings.KnockoutMapping != KnockoutMappingOptions.None)
        {
            var propertyType = propertyInfo.PropertyType.IsDotNetNullableValueType() ? propertyInfo.PropertyType.GetGenericArguments()[0] : propertyInfo.PropertyType;
            if (propertyType.IsTypeScriptArrayType())
            {
                isKnockoutObservable = true;
            }
            else
            {
                isKnockoutObservable =
                    settings.KnockoutMapping == KnockoutMappingOptions.All
                    ||
                    settings.KnockoutMapping == KnockoutMappingOptions.ValueTypes && !propertyInfo.PropertyType.IsTypeScriptInterfaceType();
            }
        }

        var nullability = nullabilityContext.Create(propertyInfo);

        string? declarerTsNamespace = null;
        propertyInfo.DeclaringType?.TryGetTypeScriptNamespaceName(settings, out declarerTsNamespace);

        this.PropertyType = TypeScriptType.FromDotNetType(
            propertyInfo.PropertyType,
            declarerTsNamespace,
            nullability,
            settings,
            isKnockoutObservable,
            propertyInfo.DeclaringType?.IsTypeScriptArrayType() ?? false,
            propertyInfo.DeclaringType?.IsTypeScriptRecordType() ?? false,
            Attribute.IsDefined(propertyInfo, typeof(RequiredAttribute))
        );
        this.PropertyName = settings.CamelCase ? propertyInfo.Name.ToCamelCase() : propertyInfo.Name;
        this.ReadOnly = !(propertyInfo.CanWrite && propertyInfo.GetSetMethod() != null && (propertyInfo.GetSetMethod()?.IsPublic ?? false));
    }

    public ITypeScriptType PropertyType { get; private set; }

    public string PropertyName { get; private set; }

    public bool ReadOnly { get; private set; }

    public override string ToString()
    {
        return string.Format(
            CultureInfo.InvariantCulture,
            "{0}{1}: {2}",
            this.ReadOnly ? "readonly " : string.Empty,
            this.PropertyName,
            this.PropertyType
        );
    }
}

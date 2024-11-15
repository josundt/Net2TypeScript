using jasMIN.Net2TypeScript.SettingsModel;
using jasMIN.Net2TypeScript.Utils;
using System.Globalization;
using System.Reflection;

namespace jasMIN.Net2TypeScript.TypeScriptModel;

#if DEBUG
[System.Diagnostics.DebuggerDisplay($"{nameof(TypeScriptProperty)}: {{ToString()}}")]
#endif
public class TypeScriptProperty
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0045:Convert to conditional expression", Justification = "<Pending>")]
    public TypeScriptProperty(PropertyInfo propertyInfo, NullabilityInfoContext nullabilityContext, Settings settings)
    {
        ArgumentNullException.ThrowIfNull(propertyInfo);
        ArgumentNullException.ThrowIfNull(nullabilityContext);
        ArgumentNullException.ThrowIfNull(settings);

        var isKnockoutObservable = false;
        if (settings.KnockoutMapping is not null and not KnockoutMappingOptions.None)
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
                    (settings.KnockoutMapping == KnockoutMappingOptions.ValueTypes && !propertyInfo.PropertyType.IsTypeScriptInterfaceType());
            }
        }

        var nullability = nullabilityContext.Create(propertyInfo);

        string? declarerTsNamespace = null;
        propertyInfo.DeclaringType?.TryGetTypeScriptNamespaceName(settings, out declarerTsNamespace);

        //var hasRequiredAnnotation =
        //    Array.Exists(Attribute.GetCustomAttributes(propertyInfo), a => a.GetType().DerivesFromClass(typeof(RequiredAttribute)))

        this.PropertyType = TypeScriptType.FromDotNetType(
            propertyInfo.PropertyType,
            declarerTsNamespace,
            nullability,
            settings,
            isKnockoutObservable,
            propertyInfo.DeclaringType?.IsTypeScriptArrayType() ?? false,
            propertyInfo.DeclaringType?.IsTypeScriptRecordType() ?? false
        //, hasRequiredAnnotation
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

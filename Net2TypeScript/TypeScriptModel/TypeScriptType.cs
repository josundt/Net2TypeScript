using jasMIN.Net2TypeScript.SettingsModel;
using jasMIN.Net2TypeScript.Utils;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;

namespace jasMIN.Net2TypeScript.TypeScriptModel;

public class TypeScriptType : ITypeScriptType
{
    public string TypeName { get; private set; }
    public bool IsNullable { get; private set; }
    public bool IsGeneric => this.GenericTypeArguments.Count > 0;
    public bool IsKnockoutObservable { get; private set; }
    public IList<ITypeScriptType> GenericTypeArguments { get; set; } = new List<ITypeScriptType>();

    public static ITypeScriptType FromDotNetType(
        Type dotnetType,
        string? declarerNamespace,
        NullabilityInfo? nullability,
        Settings settings,
        bool isKnockoutObservable = false,
        bool isArrayItem = false,
        bool isDictionaryValue = false,
        bool isRequiredProperty = false
    )
    {
        if (dotnetType is null)
        {
            throw new ArgumentNullException(nameof(dotnetType));
        }

        if (settings is null)
        {
            throw new ArgumentNullException(nameof(settings));
        }

        var tsType = new TypeScriptType()
        {
            IsKnockoutObservable = isKnockoutObservable
        };

        Type propertyType = dotnetType;

        if (dotnetType.IsDotNetNullableValueType())
        {
            tsType.IsNullable = true;
            propertyType = dotnetType.GetGenericArguments()[0];
        }
        else if (
            settings.NullableReferenceTypes == true &&
            nullability != null &&
            !(
                nullability.WriteState is NullabilityState.Unknown &&
                nullability.ReadState is NullabilityState.Unknown
            )
        )
        {
            tsType.IsNullable =
                nullability.WriteState is NullabilityState.Nullable ||
                nullability.ReadState is NullabilityState.Nullable;
        }
        else
        {
            tsType.IsNullable =
                !isRequiredProperty
                && propertyType.IsTypeScriptNullableType(settings, isArrayItem, isDictionaryValue);
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
        else if (propertyType.IsTypeScriptArrayType())
        {
            var elementType = propertyType.HasElementType ? propertyType.GetElementType()! : propertyType.GenericTypeArguments[0];
            var elementNullability = propertyType.HasElementType ? nullability?.ElementType : nullability?.GenericTypeArguments?[0];
            tsType.TypeName = "Array";
            tsType.GenericTypeArguments.Add(FromDotNetType(
                elementType,
                declarerNamespace,
                elementNullability,
                settings,
                isKnockoutObservable: false,
                isArrayItem: true,
                isDictionaryValue: false
            ));
        }
        else if (propertyType.IsTypeScriptRecordType())
        {
            var keyType = propertyType.GenericTypeArguments[0];
            var valueType = propertyType.GenericTypeArguments[1];
            tsType.TypeName = "Record";
            tsType.GenericTypeArguments.Add(FromDotNetType(
                keyType,
                declarerNamespace,
                nullability?.GenericTypeArguments[0],
                settings,
                isKnockoutObservable: false,
                isArrayItem: false,
                isDictionaryValue: false,
                true
            ));
            tsType.GenericTypeArguments.Add(FromDotNetType(
                valueType,
                declarerNamespace,
                nullability?.GenericTypeArguments[1],
                settings,
                isKnockoutObservable: false,
                isArrayItem: false,
                isDictionaryValue: true
            ));
        }
        else if (propertyType.IsGenericParameter)
        {
            tsType.TypeName = propertyType.Name;
        }
        else if (
            propertyType.IsTypeScriptInterfaceType() ||
            propertyType.IsEnum
        )
        {
            if (propertyType.TryGetTypeScriptNamespaceName(settings, out string propertyTypeTsNs))
            {
                string scopedNs;
                if (declarerNamespace != null)
                {
                    if (settings.TsFlattenNamespaces)
                    {
                        scopedNs = string.Empty;
                    }
                    else
                    {
                        var trimDeclarerNsRegex = new Regex($"^{declarerNamespace.Replace(".", "\\.", StringComparison.Ordinal)}\\.?");
                        scopedNs = trimDeclarerNsRegex.Replace(propertyTypeTsNs, string.Empty);
                    }
                }
                else
                {
                    scopedNs = propertyTypeTsNs;
                }
                tsType.TypeName = $"{scopedNs}{(string.IsNullOrEmpty(scopedNs) ? "" : ".")}{propertyType.Name}";
            }
            else
            {
                tsType.TypeName = "object";
            }
        }


        if (tsType.IsKnockoutObservable)
        {
            if (propertyType.IsTypeScriptArrayType())
            {
                tsType.TypeName = "KnockoutObservableArray";
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
            CultureInfo.InvariantCulture,
            "{0}{1}{2}{3}{4}",
            this.TypeName,
            this.GenericTypeArguments.Count > 0 ? "<" : string.Empty,
            string.Join(", ", this.GenericTypeArguments.Select(tsti => tsti.ToString())),
            this.GenericTypeArguments.Count > 0 ? ">" : string.Empty,
            this.IsNullable ? " | null" : ""
        );
    }

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

using jasMIN.Net2TypeScript.SettingsModel;
using jasMIN.Net2TypeScript.TypeScriptModel;
using jasMIN.Net2TypeScript.Utils;
using System.Reflection;

namespace jasMIN.Net2TypeScript.DotNetModel;

#if DEBUG
[System.Diagnostics.DebuggerDisplay($"{nameof(Property)}: {{{nameof(PropName)}}}, {{{nameof(TypeScriptProperty)}}}")]
#endif
internal class Property : DotNetTypeModelBase
{
    private readonly PropertyInfo _propInfo;

    public Property(PropertyInfo propertyInfo, NullabilityInfoContext nullabilityContext, GlobalSettings globalSettings)
        : base(
              propertyInfo.PropertyType.IsDotNetNullableValueType() ? propertyInfo.PropertyType.GetGenericArguments()[0] : propertyInfo.PropertyType,
              globalSettings)
    {
        this._propInfo = propertyInfo;
        this.TypeScriptProperty = new TypeScriptProperty(propertyInfo, nullabilityContext, this.Settings);

        if (!(propertyInfo.CanRead || propertyInfo.GetGetMethod()!.IsPublic))
        {
            throw new InvalidOperationException($"Property {this._propInfo.DeclaringType?.FullName}.{this._propInfo.Name} is not readable.");
        }
    }

    public string PropName => this._propInfo.Name;

    public Type DeclaringType => this._propInfo?.DeclaringType!;

    public TypeScriptProperty TypeScriptProperty { get; }

    public override StreamWriter WriteTs(StreamWriter sw, int indentCount)
    {
        var indent = this.Indent(indentCount);

        if (this.TsTypeName != null)
        {
            if (this._type.IsEnum)
            {
                sw.WriteFormat(
                    "{0}/** Enum{1} */{2}",
                    indent,
                    this.TypeScriptProperty.PropertyType.IsNullable ? " (NULLABLE)" : string.Empty,
                    sw.NewLine
                );
            }
            else if (this.TypeScriptProperty.PropertyType.IsNullable)
            {
                sw.WriteLine($"{indent}/** Nullable */");
            }

            if (this._type == typeof(Guid) || this._type == typeof(Guid?))
            {
                sw.WriteFormat(
                    "{0}/** Guid{1} */{2}",
                    indent,
                    this._propInfo.PropertyType.IsDotNetNullableValueType() ? " (NULLABLE)" : string.Empty,
                    sw.NewLine
                );
            }

            if (this._type == typeof(TimeSpan) || this._type == typeof(TimeSpan?))
            {
                sw.WriteFormat(
                    "{0}/** TimeSpan{1} */{2}",
                    indent,
                    this._propInfo.PropertyType.IsDotNetNullableValueType() ? " (NULLABLE)" : string.Empty,
                    sw.NewLine
                );
            }

            sw.WriteLine(
                $"{indent}{this.TypeScriptProperty};");
        }

        return sw;
    }
}

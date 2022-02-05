using jasMIN.Net2TypeScript.SettingsModel;
using jasMIN.Net2TypeScript.TypeScriptModel;
using jasMIN.Net2TypeScript.Utils;
using System.Diagnostics;
using System.Reflection;

namespace jasMIN.Net2TypeScript.DotNetModel;

[DebuggerDisplay($"prop: {{{nameof(PropName)},nq}}")]
class PropertyModel : DotNetTypeModelBase
{
    public PropertyModel(PropertyInfo propertyInfo, NullabilityInfoContext nullabilityContext, GlobalSettings globalSettings)
        : base(
              propertyInfo.PropertyType.IsDotNetNullableValueType() ? propertyInfo.PropertyType.GetGenericArguments()[0] : propertyInfo.PropertyType,
              globalSettings)
    {
        this.PropInfo = propertyInfo;
        this.DeclaringType = propertyInfo.DeclaringType!;
        this.TsPropInfo = new TypeScriptPropertyInfo(propertyInfo, nullabilityContext, this.Settings);

        if (!(propertyInfo.CanRead || propertyInfo.GetGetMethod()!.IsPublic))
        {
            throw new InvalidOperationException($"Property {this.DeclaringType.FullName}.{this.PropInfo.Name} is not readable.");
        }

    }

    PropertyInfo PropInfo { get; }

    public string PropName => this.PropInfo.Name;

    public Type DeclaringType { get; private set; }

    public TypeScriptPropertyInfo TsPropInfo { get; set; }

    //protected string IndentationContext =>
    //    string.Concat(
    //        Enumerable.Repeat(
    //            this.Settings.Indent,

    //                (this.DeclaringType.Namespace ?? string.Empty).Split('.').Length -
    //                this.Settings.DotNetRootNamespace.Split('.').Length +
    //                this.ExtraIndents

    //        )
    //    );

    public override StreamWriter WriteTs(StreamWriter sw, int indentCount)
    {
        var indent = this.Indent(indentCount);

        if (this.TsTypeName != null)
        {
            if (this.Type.IsEnum)
            {
                sw.WriteFormat(
                    "{0}/** Enum{1}: {2} */{3}",
                    indent,
                    this.PropInfo.PropertyType.IsDotNetNullableValueType() ? " (NULLABLE)" : string.Empty,
                    this.FullName,
                    Environment.NewLine
                );
            }
            else if (this.PropInfo.PropertyType.IsDotNetNullableValueType())
            {
                sw.WriteLine($"{indent}/** Nullable */");
            }

            if (this.Type == typeof(Guid) || this.Type == typeof(Guid?))
            {
                sw.WriteFormat(
                    "{0}/** Guid{1} */{2}",
                    indent,
                    this.PropInfo.PropertyType.IsDotNetNullableValueType() ? " (NULLABLE)" : string.Empty,
                    Environment.NewLine
                );
            }

            if (this.Type == typeof(TimeSpan) || this.Type == typeof(TimeSpan?))
            {
                sw.WriteFormat(
                    "{0}/** TimeSpan{1} */{2}",
                    indent,
                    this.PropInfo.PropertyType.IsDotNetNullableValueType() ? " (NULLABLE)" : string.Empty,
                    Environment.NewLine
                );
            }

            sw.WriteLine(
                $"{indent}{this.TsPropInfo};");
        }

        return sw;
    }
}

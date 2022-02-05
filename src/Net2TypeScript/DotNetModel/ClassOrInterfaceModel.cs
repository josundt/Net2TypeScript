using jasMIN.Net2TypeScript.SettingsModel;
using jasMIN.Net2TypeScript.Utils;
using System.Diagnostics;
using System.Reflection;

namespace jasMIN.Net2TypeScript.DotNetModel;

[DebuggerDisplay($"{{{nameof(DebuggerDisplay)},nq}}")]
class ClassOrInterfaceModel : DotNetTypeModelBase
{
    private string DebuggerDisplay => @$"{(this.Type.IsClass ? "class" : "interface")}: {this.Type.Name}";

    public ClassOrInterfaceModel(Type type, NullabilityInfoContext nullabilityContext, GlobalSettings globalSettings)
        : base(type, globalSettings)
    {
        var isValidType = (type.IsClass || type.IsInterface) && type.IsPublic;

        if (!isValidType)
        {
            throw new InvalidOperationException("Not a public class type");
        }
        this.Properties = new List<PropertyModel>();
        this.Initialize(nullabilityContext);
    }

    List<PropertyModel> Properties { get; set; }

    private void Initialize(NullabilityInfoContext nullabilityContext)
    {
        var type = this.Type;

        while (type != null)
        {
            foreach (PropertyInfo propertyInfo in type.GetProperties().Where(prop => prop.CanRead && (prop.GetGetMethod()?.IsPublic ?? false)))
            {
                var isDefinedAsExtraProp =
                    this.Settings.ExtraProperties.Any(ep => ep.Key == (this.Settings.CamelCase ? propertyInfo.Name.ToCamelCase() : propertyInfo.Name));

                if (!isDefinedAsExtraProp)
                {
                    this.Properties.Add(new PropertyModel(propertyInfo, nullabilityContext, this._globalSettings));
                }
            }

            type = this.Type.IsInterface ? this.GetBaseType(type) : null;
        }
    }

    private Type? GetBaseType(Type classOrInterface)
    {
        Type? result = null;
        if (classOrInterface.IsInterface)
        {
            var interfaces = classOrInterface.GetInterfaces();
            if (interfaces != null && interfaces.Length > 0)
            {
                result = interfaces[0];
            }
        }
        else if (classOrInterface.IsClass)
        {
            result = this.Type.BaseType;
        }

        // If base type not in the included assemblies, skip it
        if (result != null)
        {
            var baseTypeAssemblyName = result.Assembly.FullName!.Split(',')[0];
            if (!this._globalSettings.AssemblyPaths.Any(ap => ap.Contains(baseTypeAssemblyName, StringComparison.OrdinalIgnoreCase)))
            {
                result = null;
            }
            else if (result == classOrInterface)
            {
                result = null;
            }
        }

        return result;
    }

    public override StreamWriter WriteTs(StreamWriter sw, int indentCount)
    {
        var indent = this.Indent(indentCount);

        var skip =
            this.Type.IsClass && this.Settings.ExcludeClass == true ||
            this.Type.IsInterface && this.Settings.ExcludeInterface == true ||
            this.Properties.Count == 0;


        if (!skip)
        {

            sw.WriteFormat("{3}{0}/** .NET {1}: {2} */{3}",
                indent,
                this.Type.IsClass ? "class" : "interface",
                this.FullName,
                Environment.NewLine
            );

            sw.WriteFormat("{0}export interface {1}{2} {{{3}",
                indent,
                this.TsTypeName,
                this.Settings.UseBreeze == true ? " extends breeze.Entity" : string.Empty,
                Environment.NewLine
            );

            // TODO: Filter non-public props
            // RENDER PROPERTYINFOS
            foreach (var prop in this.Properties)
            {
                prop.WriteTs(sw, indentCount + 1);
            }


            if (this.Settings.ExtraProperties != null)
            {
                foreach (KeyValuePair<string, string> prop in this.Settings.ExtraProperties)
                {
                    if (!string.IsNullOrEmpty(prop.Value))
                    {
                        WriteExtensionProperty(sw, prop, indent, this.Settings);
                    }
                }
            }

            sw.WriteLine($"{indent}}}");

        }

        return sw;
    }

    private static void WriteExtensionProperty(StreamWriter sw, KeyValuePair<string, string> prop, string indentationContext, Settings settings)
    {
        var tsPropName = prop.Key;
        var tsTypeName = prop.Value.ToString();
        sw.WriteFormat(
            "{0}/** Extra/overridden property */{1}",
            indentationContext + settings.Indent,
            Environment.NewLine
        );
        sw.WriteFormat(
            "{0}{1}: {2};{3}",
            indentationContext + settings.Indent,
            tsPropName,
            tsTypeName,
            Environment.NewLine
        );
    }

}

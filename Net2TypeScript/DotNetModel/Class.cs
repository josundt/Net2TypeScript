using jasMIN.Net2TypeScript.SettingsModel;
using jasMIN.Net2TypeScript.Utils;
using System.Reflection;

namespace jasMIN.Net2TypeScript.DotNetModel;

#if DEBUG
[System.Diagnostics.DebuggerDisplay(@$"{nameof(Class)}: {{{nameof(_type)}.Name}} ({{_classOrInterfaceName,nq}}), {nameof(Properties)}: {{{nameof(Properties)}.Count}}")]
#endif
class Class : DotNetTypeModelBase
{
    private readonly IEnumerable<Property> _properties;

    public Class(Type type, NullabilityInfoContext nullabilityContext, GlobalSettings globalSettings)
        : base(type, globalSettings)
    {
        var isValidType = (type.IsClass || type.IsInterface) && type.IsPublic;

        if (!isValidType)
        {
            throw new InvalidOperationException("Not a public class type");
        }

        this._properties = GetProperties(type, nullabilityContext, this.Settings, globalSettings);
    }

    public override StreamWriter WriteTs(StreamWriter sw, int indentCount)
    {
        var indent = this.Indent(indentCount);

        var skip =
            this._type.IsClass && this.Settings.ExcludeClass == true ||
            this._type.IsInterface && this.Settings.ExcludeInterface == true ||
            !this._properties.Any();


        if (!skip)
        {

            sw.WriteFormat("{3}{0}/** .NET {1}: {2} */{3}",
                indent,
                this._type.IsClass ? "class" : "interface",
                this.FullName,
                sw.NewLine
            );

            sw.WriteFormat("{0}export interface {1}{2} {{{3}",
                indent,
                this.TsTypeName,
                this.Settings.UseBreeze == true ? " extends breeze.Entity" : string.Empty,
                sw.NewLine
            );

            // TODO: Filter non-public props
            // RENDER PROPERTYINFOS
            foreach (var prop in this._properties)
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
            sw.NewLine
        );
        sw.WriteFormat(
            "{0}{1}: {2};{3}",
            indentationContext + settings.Indent,
            tsPropName,
            tsTypeName,
            sw.NewLine
        );
    }

    private static IEnumerable<Property> GetProperties(
        Type type,
        NullabilityInfoContext nullabilityContext,
        Settings classSettings,
        GlobalSettings globalSettings
    )
    {
        var properties = new List<Property>();

        Type? baseType = type;

        // Get properties for this type and all base types
        while (baseType != null)
        {
            foreach (PropertyInfo propertyInfo in type.GetProperties().Where(prop => prop.CanRead && (prop.GetGetMethod()?.IsPublic ?? false)))
            {
                var isDefinedAsExtraProp =
                    classSettings.ExtraProperties.Any(ep => ep.Key == (classSettings.CamelCase ? propertyInfo.Name.ToCamelCase() : propertyInfo.Name));

                if (!isDefinedAsExtraProp)
                {
                    properties.Add(new Property(propertyInfo, nullabilityContext, globalSettings));
                }
            }

            baseType = type.IsInterface ? GetBaseType(baseType, globalSettings.AssemblyPaths) : null;
        }

        return properties;
    }

    private static Type? GetBaseType(Type classOrInterface, IEnumerable<string> assemblyPaths)
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
            result = classOrInterface.BaseType;
        }

        // If base type not in the included assemblies, skip it
        if (result != null)
        {
            var baseTypeAssemblyName = result.Assembly.FullName!.Split(',')[0];
            if (!assemblyPaths.Any(ap => ap.Contains(baseTypeAssemblyName, StringComparison.OrdinalIgnoreCase)))
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

    private string _classOrInterfaceName => this._type.IsClass ? "class" : "interface";

#region Debug-Only Helper Properties
#if DEBUG

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Critical Code Smell", "S2365:Properties should not make collection or array copies", Justification = "<Pending>")]
    private IReadOnlyCollection<Property> Properties => this._properties.ToList();

#endif
#endregion

}

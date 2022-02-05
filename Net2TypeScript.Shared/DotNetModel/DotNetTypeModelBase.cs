using jasMIN.Net2TypeScript.Shared.SettingsModel;
using System.Text.RegularExpressions;

namespace jasMIN.Net2TypeScript.Shared.DotNetModel;

abstract class DotNetTypeModelBase : DotNetModelBase
{
    private static readonly Regex _genericTypeNameFixRegEx = new(@"`.*$", RegexOptions.Compiled);

    protected DotNetTypeModelBase(Type type, GlobalSettings globalSettings)
        : base(globalSettings)
    {
        this.Type = type;
        if (type.IsGenericTypeDefinition)
        {
            var genericArgNames = type.GetGenericArguments().Select(a => a.Name);

            var baseFullName = _genericTypeNameFixRegEx.Replace(type.FullName!, string.Empty);
            this.FullName = $"{baseFullName}<{string.Join(", ", genericArgNames)}>";

            var baseName = _genericTypeNameFixRegEx.Replace(type.Name, string.Empty);
            this.TsTypeName = $"{baseName}<{string.Join(", ", genericArgNames)}>";
        }
        else
        {
            this.FullName = type.FullName!;
            this.TsTypeName = type.Name;
        }
    }

    protected Type Type { get; private set; }

    protected string Namespace => this.Type.Namespace ?? string.Empty;

    protected virtual string TsTypeName { get; }

    protected virtual string TsFullName => this.Settings.ToTsFullName(this.FullName);

    public override abstract StreamWriter WriteTs(StreamWriter sw, int indentCount);

}

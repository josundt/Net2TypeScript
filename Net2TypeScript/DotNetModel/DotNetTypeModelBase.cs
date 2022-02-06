using jasMIN.Net2TypeScript.SettingsModel;
using System.Text.RegularExpressions;

namespace jasMIN.Net2TypeScript.DotNetModel;

abstract class DotNetTypeModelBase : DotNetModelBase
{
    private static readonly Regex _genericTypeNameFixRegEx = new(@"`.*$", RegexOptions.Compiled);

    protected DotNetTypeModelBase(Type type, GlobalSettings globalSettings)
        : base(globalSettings)
    {
        this._type = type;
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

    protected readonly Type _type;

    public string Namespace() => this._type.Namespace ?? string.Empty;

    public string Name => this._type.Name;

    protected virtual string TsTypeName { get; }

    public override abstract StreamWriter WriteTs(StreamWriter sw, int indentCount);

}

using jasMIN.Net2TypeScript.SettingsModel;
using System.Text.RegularExpressions;

namespace jasMIN.Net2TypeScript.DotNetModel;

internal abstract partial class DotNetTypeModelBase : DotNetModelBase
{
    private readonly Lazy<string> _tsTypeNameLazy;

    protected DotNetTypeModelBase(Type type, GlobalSettings globalSettings)
        : base(globalSettings, GetFullName(type))
    {
        this._type = type;
        this.FullName = GetFullName(this._type);
        this._tsTypeNameLazy = new Lazy<string>(() => GetTsTypeName(this._type));
    }

    protected readonly Type _type;

    public string Namespace() => this._type.Namespace ?? string.Empty;

    public string Name => this._type.Name;

    protected string TsTypeName => this._tsTypeNameLazy.Value;

    public abstract override StreamWriter WriteTs(StreamWriter sw, int indentCount);

    private static string GetFullName(Type type)
    {
        if (type.IsGenericTypeDefinition)
        {
            var genericArgNames = type.GetGenericArguments().Select(a => a.Name);

            var baseFullName = GenericTypeNameFixRegEx().Replace(type.FullName!, string.Empty);
            return $"{baseFullName}<{string.Join(", ", genericArgNames)}>";

        }
        else
        {
            return type.FullName!;
        }
    }

    private static string GetTsTypeName(Type type)
    {
        if (type.IsGenericTypeDefinition)
        {
            var genericArgNames = type.GetGenericArguments().Select(a => a.Name);

            var baseName = GenericTypeNameFixRegEx().Replace(type.Name, string.Empty);
            return $"{baseName}<{string.Join(", ", genericArgNames)}>";
        }
        else
        {
            return type.Name;
        }
    }

    [GeneratedRegex("`.*$", RegexOptions.Compiled)]
    private static partial Regex GenericTypeNameFixRegEx();
}
using jasMIN.Net2TypeScript.SettingsModel;
using System.Globalization;
using System.Reflection;

namespace jasMIN.Net2TypeScript.DotNetModel;

#if DEBUG
[System.Diagnostics.DebuggerDisplay($"{nameof(Namespace)}: {{{nameof(FullName)}}}, {nameof(ChildNamespaces)}: {{{nameof(ChildNamespaces)}.Count}}, {nameof(Entities)}: {{{nameof(Entities)}.Count}}")]
#endif
internal sealed class Namespace : DotNetModelBase
{
    private readonly bool _isRoot;
    private readonly IEnumerable<Namespace> _childNamespaces;
    private readonly IEnumerable<DotNetTypeModelBase> _entities;

    public Namespace(string name, NullabilityInfoContext nullabilityContext, GlobalSettings settings, IEnumerable<Type>? rootAndDescendantNsTypes = null)
        : base(settings, name)
    {
        this._isRoot = rootAndDescendantNsTypes is null;

        rootAndDescendantNsTypes ??= LoadRootAndDescendantNsTypes(this.Settings.AssemblyPaths, name);

        var entities = new List<DotNetTypeModelBase>();

        if (this.IncludeClasses())
        {
            entities.AddRange(this.GetNamespaceClassesAndInterfaces(rootAndDescendantNsTypes, nullabilityContext));
        }

        if (this.IncludeEnums())
        {
            entities.AddRange(this.GetNamespaceEnums(rootAndDescendantNsTypes));
        }

        this._entities = entities;

        this._childNamespaces = rootAndDescendantNsTypes
            .Select(c => c.Namespace)
            .Distinct()
            .Where(ns => ns?.StartsWith($"{this.FullName}.", StringComparison.Ordinal) ?? false)
            .Select(ns =>
                string.Format(
                    CultureInfo.InvariantCulture,
                    "{0}.{1}",
                    this.FullName,
                    ns![this.FullName.Length..].Split('.')[1]
                )
            )
            .Distinct()
            .OrderBy(ns => ns)
            .Select(ns => new Namespace(ns, nullabilityContext, this._globalSettings, rootAndDescendantNsTypes))
            .Where(ns => !ns.IsEmpty());
    }

    public override StreamWriter WriteTs(StreamWriter sw, int indentCount)
    {
        var indent = this.Indent(indentCount);

        var childIndentCount = this._isRoot || this.Settings.TsFlattenNamespaces ? indentCount : indentCount + 1;

        if (!this._isRoot && !this.Settings.TsFlattenNamespaces)
        {
            sw.WriteLine($"{Environment.NewLine}{indent}export namespace {this.FullName.Split('.')[^1]} {{");
        }

        foreach (var ns in this._childNamespaces)
        {
            ns.WriteTs(sw, childIndentCount);
        }

        foreach (var type in this._entities)
        {
            type.WriteTs(sw, childIndentCount);
        }

        if (!this._isRoot && !this.Settings.TsFlattenNamespaces)
        {
            sw.WriteLine($"{indent}}}");
        }

        return sw;
    }

    private bool IncludeClasses() =>
        this.Settings.ClassNamespaceFilter?.Any(
            f => f.EndsWith('*')
                ? this.FullName.StartsWith(f[..f.LastIndexOf('*')], StringComparison.Ordinal)
                : this.FullName == f
        ) ?? true;

    private bool IncludeEnums() =>
        this.Settings.EnumNamespaceFilter?.Any(
            f => f.EndsWith('*')
                ? this.FullName.StartsWith(f[..f.LastIndexOf('*')], StringComparison.Ordinal)
                : this.FullName == f
        ) ?? true;

    private bool IsEmpty() =>
        !this._entities.Any() && this._childNamespaces.All(ns => ns.IsEmpty());

    private static List<Type> GetTypes(IEnumerable<Assembly> assemblies)
    {
        List<Type> types = [];

        foreach (var a in assemblies)
        {
            Type[] assemblyTypes;

            try
            {
                assemblyTypes = a.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                assemblyTypes = e.Types.Where(t => t != null).ToArray()!;
            }

            types.AddRange(assemblyTypes.Where(t => t != null));
        }

        return types;

    }

    private static IEnumerable<Type> LoadRootAndDescendantNsTypes(IEnumerable<string> assemblyPaths, string namespaceName)
    {
        var assemblies = assemblyPaths
            .Select(Assembly.LoadFrom);

        foreach (var a in assemblies)
        {
            a.GetReferencedAssemblies();
        }

        return GetTypes(assemblies)
            .Where(t =>
                t.IsPublic &&
                (t.IsClass || t.IsInterface || t.IsEnum) &&
                (t.FullName ?? string.Empty).StartsWith(namespaceName, StringComparison.Ordinal)
            );
    }

    private IEnumerable<Class> GetNamespaceClassesAndInterfaces(IEnumerable<Type> namespaceDescendantTypes, NullabilityInfoContext nullabilityContext)
    {
        return namespaceDescendantTypes
            .Where(t => t.Namespace == this.FullName && (t.IsClass || t.IsInterface))
            .OrderBy(t => t.Name)
            .Select(t => new Class(t, nullabilityContext, this._globalSettings));
    }

    private IEnumerable<Enum> GetNamespaceEnums(IEnumerable<Type> namespaceDescendantTypes)
    {
        return namespaceDescendantTypes
            .Where(t => t.Namespace == this.FullName && t.IsEnum)
            .OrderBy(t => t.Name)
            .Select(t => new Enum(t, this._globalSettings));
    }

    #region Debug-Only Helper Properties
#if DEBUG

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Critical Code Smell", "S2365:Properties should not make collection or array copies", Justification = "<Pending>")]
    public IReadOnlyCollection<Namespace> ChildNamespaces => this._childNamespaces.ToList();

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Critical Code Smell", "S2365:Properties should not make collection or array copies", Justification = "<Pending>")]
    public IReadOnlyCollection<DotNetTypeModelBase> Entities => this._entities.ToList();

#endif
    #endregion
}
using jasMIN.Net2TypeScript.SettingsModel;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;

namespace jasMIN.Net2TypeScript.DotNetModel;

[DebuggerDisplay($"{nameof(NamespaceModel)}: {{{nameof(FullName)}}}, {nameof(ChildNamespaces)}: {{{nameof(ChildNamespaces)}.Count}}, {nameof(Entities)}: {{{nameof(Entities)}.Count}}")]
class NamespaceModel : DotNetModelBase
{
    public NamespaceModel(string name, NullabilityInfoContext nullabilityContext, GlobalSettings settings, IEnumerable<Type>? rootAndDescendantNsTypes = null)
        : base(settings)
    {
        this.FullName = name;
        this.IsRoot = rootAndDescendantNsTypes is null;

        if (rootAndDescendantNsTypes is null)
        {
            rootAndDescendantNsTypes = LoadRootAndDescendantNsTypes(this.Settings.AssemblyPaths, name);
        }

        var entities = new List<DotNetTypeModelBase>();

        if (this.IncludeClasses())
        {
            entities.AddRange(this.GetNamespaceClassesAndInterfaces(rootAndDescendantNsTypes, nullabilityContext));
        }

        if (this.IncludeEnums())
        {
            entities.AddRange(this.GetNamespaceEnums(rootAndDescendantNsTypes));
        }

        this.Entities = entities;

        this.ChildNamespaces = rootAndDescendantNsTypes
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
            .Select(ns => new NamespaceModel(ns, nullabilityContext, this._globalSettings, rootAndDescendantNsTypes))
            .ToList();

        if (this.IsRoot)
        {
            RemoveEmptyNamespaces(this);
        }
    }

    public IList<NamespaceModel> ChildNamespaces { get; set; }

    public IReadOnlyCollection<DotNetTypeModelBase> Entities { get; set; }

    public override StreamWriter WriteTs(StreamWriter sw, int indentCount)
    {

        var indent = this.Indent(indentCount);

        var childIndentCount = this.IsRoot ? indentCount : indentCount + 1;


        if (!this.IsEmpty())
        {
            if (!this.IsRoot)
            {
                sw.WriteLine($"{Environment.NewLine}{indent}export namespace {this.FullName.Split('.').Last()} {{");
            }

            foreach (var ns in this.ChildNamespaces)
            {
                ns.WriteTs(sw, childIndentCount);
            }

            foreach (var type in this.Entities)
            {
                type.WriteTs(sw, childIndentCount);
            }

            if (!this.IsRoot)
            {
                sw.WriteLine($"{indent}}}");
            }

        }

        return sw;
    }

    private bool IsRoot { get; }

    private bool IncludeClasses() =>
        this.Settings.ClassNamespaceFilter?.Any(
            f => f.EndsWith("*", StringComparison.Ordinal)
                ? this.FullName.StartsWith(f[..f.LastIndexOf("*", StringComparison.Ordinal)], StringComparison.Ordinal)
                : this.FullName == f
        ) ?? false;

    private bool IncludeEnums() =>
        this.Settings.EnumNamespaceFilter?.Any(
            f => f.EndsWith("*", StringComparison.Ordinal)
                ? this.FullName.StartsWith(f[..f.LastIndexOf("*", StringComparison.Ordinal)], StringComparison.Ordinal)
                : this.FullName == f
        ) ?? false;

    private bool IsEmpty() =>
        this.Entities.Count == 0 && this.ChildNamespaces.All(ns => ns.IsEmpty());

    private static IEnumerable<Type> GetTypes(IEnumerable<Assembly> assemblies)
    {
        List<Type> types = new();

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

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S3885:\"Assembly.Load\" should be used", Justification = "<Pending>")]
    private static IEnumerable<Type> LoadRootAndDescendantNsTypes(IEnumerable<string> assemblyPaths, string namespaceName)
    {
        var assemblies = assemblyPaths
            .Select(ap => Assembly.LoadFrom(ap));

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

    private IEnumerable<ClassOrInterfaceModel> GetNamespaceClassesAndInterfaces(IEnumerable<Type> namespaceDescendantTypes, NullabilityInfoContext nullabilityContext)
    {
        return namespaceDescendantTypes
            .Where(t => t.Namespace == this.FullName && (t.IsClass || t.IsInterface))
            .OrderBy(t => t.Name)
            .Select(t => new ClassOrInterfaceModel(t, nullabilityContext, this._globalSettings));
    }

    private IEnumerable<EnumModel> GetNamespaceEnums(IEnumerable<Type> namespaceDescendantTypes)
    {
        return namespaceDescendantTypes
            .Where(t => t.Namespace == this.FullName && t.IsEnum)
            .OrderBy(t => t.Name)
            .Select(t => new EnumModel(t, this._globalSettings));
    }

    private static bool RemoveEmptyNamespaces(NamespaceModel namespaceModel)
    {
        var allChildrenAreEmpty = true;
        for (var i = namespaceModel.ChildNamespaces.Count - 1; i >= 0; i--)
        {
            var childNs = namespaceModel.ChildNamespaces[i];

            var childIsEmpty = RemoveEmptyNamespaces(childNs);
            if (childIsEmpty)
            {
                namespaceModel.ChildNamespaces.RemoveAt(i);
            }

            allChildrenAreEmpty = allChildrenAreEmpty && childIsEmpty;
        }

        return allChildrenAreEmpty && !namespaceModel.Entities.Any();
    }

    private static bool HasAnyEntities(NamespaceModel namespaceModel)
    {
        if (namespaceModel.Entities.Count > 0)
        {
            return true;
        }
        else
        {
            var result = false;
            foreach (var ns in namespaceModel.ChildNamespaces)
            {
                result = HasAnyEntities(ns);
                if (result)
                {
                    break;
                }
            }
            return result;
        }
    }
}
using jasMIN.Net2TypeScript.Shared.SettingsModel;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;

namespace jasMIN.Net2TypeScript.Shared.DotNetModel;

[DebuggerDisplay($"namespace: {{{nameof(FullName)},nq}}")]
class NamespaceModel : DotNetModelBase
{
    public NamespaceModel(string name, NullabilityInfoContext nullabilityContext, GlobalSettings settings)
        : base(settings)
    {
        this.FullName = name;
        this.Entities = new List<ClassOrInterfaceModel>();
        this.Enums = new List<EnumModel>();
        this.ChildNamespaces = new List<NamespaceModel>();

        this.Initialize(nullabilityContext);
    }

    bool IsRoot =>
        this.Settings.DotNetRootNamespace == this.FullName;

    bool IsRootDirectChild =>
        this.FullName.Split('.').Length - this.Settings.DotNetRootNamespace.Split('.').Length == 1;

    bool IncludeClasses =>
        this.Settings.ClassNamespaceFilter?.Any(
            f => f.EndsWith("*", StringComparison.Ordinal)
                ? this.FullName.StartsWith(f[..f.LastIndexOf("*", StringComparison.Ordinal)], StringComparison.Ordinal)
                : this.FullName == f
        ) ?? false;

    bool IncludeEnums =>
        this.Settings.EnumNamespaceFilter?.Any(
            f => f.EndsWith("*", StringComparison.Ordinal)
                ? this.FullName.StartsWith(f[..f.LastIndexOf("*", StringComparison.Ordinal)], StringComparison.Ordinal)
                : this.FullName == f
        ) ?? false;

    bool IsEmpty =>
        this.Entities.Count + this.Enums.Count == 0 && this.ChildNamespaces.All(ns => ns.IsEmpty);

    bool ContainsEnums =>
        this.Enums.Count > 0 || this.ChildNamespaces.Any(ns => ns.ContainsEnums);

    List<ClassOrInterfaceModel> Entities { get; set; }

    List<EnumModel> Enums { get; set; }

    List<NamespaceModel> ChildNamespaces { get; set; }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S3885:\"Assembly.Load\" should be used", Justification = "<Pending>")]
    void Initialize(NullabilityInfoContext nullabilityContext)
    {

        List<Assembly> assemblies = this.Settings.AssemblyPaths.Select(ap => Assembly.LoadFrom(ap)).ToList();
        foreach (var a in assemblies)
        {
            a.GetReferencedAssemblies();
        }


        // Processing entities (class types)
        var allClassTypes = GetTypes(assemblies)
            .Where(t => (t.IsClass || t.IsInterface) && t.IsPublic && (t.Namespace ?? string.Empty).StartsWith(this.FullName, StringComparison.Ordinal))
            .ToList();

        // Processing enums
        var allEnumTypes = GetTypes(assemblies)
            .Where(t => t.IsEnum && t.IsPublic && (t.Namespace ?? string.Empty).StartsWith(this.FullName, StringComparison.Ordinal))
            .ToList();

        if (this.IncludeClasses)
        {
            var nsClassTypes = allClassTypes.Where(t => t.Namespace == this.FullName).ToList();
            nsClassTypes.Sort(delegate (Type type1, Type type2) { return string.Compare(type1.Name, type2.Name, StringComparison.Ordinal); });

            foreach (var classType in nsClassTypes)
            {
                this.Entities.Add(new ClassOrInterfaceModel(classType, nullabilityContext, this._globalSettings));
            }
        }

        if (this.IncludeEnums)
        {
            var nsEnumTypes = allEnumTypes.Where(t => t.Namespace == this.FullName).ToList();
            nsEnumTypes.Sort(delegate (Type type1, Type type2) { return string.Compare(type1.Name, type2.Name, StringComparison.Ordinal); });

            foreach (var enumType in nsEnumTypes)
            {
                this.Enums.Add(new EnumModel(this._globalSettings, enumType));
            }
        }

        // Processing direct child namespaces
        var allNamespaces = allClassTypes
            .Select(t => t.Namespace)
            .Concat(allEnumTypes.Select(t => t.Namespace))
            .Distinct();

        var childNamespaces = allNamespaces
            .Select(ns => ns ?? string.Empty)
            .Where(ns => ns.StartsWith($"{this.FullName}.", StringComparison.Ordinal) && ns.Length > this.FullName.Length)
            .Select(ns =>
                string.Format(
                    CultureInfo.InvariantCulture,
                    "{0}.{1}",
                    this.FullName, ns[this.FullName.Length..].Split('.')[1]
                )
            )
            .Distinct()
            .ToList();

        childNamespaces.Sort();

        foreach (var childNamespace in childNamespaces)
        {
            this.ChildNamespaces.Add(new NamespaceModel(childNamespace, nullabilityContext, this._globalSettings));
        }
    }

    static List<Type> GetTypes(List<Assembly> assemblies)
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

    public override StreamWriter WriteTs(StreamWriter sw, int indentCount)
    {

        var indent = base.Indent(indentCount);

        var childIndentCount = this.IsRoot ? indentCount : indentCount + 1;


        if (!this.IsEmpty)
        {
            if (!this.IsRoot)
            {
                sw.WriteLine($"{Environment.NewLine}{indent}export namespace {this.FullName.Split('.').Last()} {{");
            }

            foreach (var ns in this.ChildNamespaces)
            {
                ns.WriteTs(sw, childIndentCount);
            }

            foreach (var entity in this.Entities)
            {
                entity.WriteTs(sw, childIndentCount);
            }

            foreach (var enumModel in this.Enums)
            {
                enumModel.WriteTs(sw, childIndentCount);
            }

            if (!this.IsRoot)
            {
                sw.WriteLine($"{indent}}}");
            }

        }

        return sw;
    }
}

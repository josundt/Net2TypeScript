using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace jasMIN.Net2TypeScript.Shared.Model
{
    [DebuggerDisplay("namespace: {ClrFullName,nq}")]
    class NamespaceModel : ClrModelBase
    {
        public NamespaceModel(GlobalSettings settings, string name)
            : base(settings)
        {
            this.ClrFullName = name;
            this.Entities = new List<ClassOrInterfaceModel>();
            this.Enums = new List<EnumModel>();
            this.ChildNamespaces = new List<NamespaceModel>();

            this.Initialize();
        }

        protected override string IndentationContext =>
                string.Concat(Enumerable.Repeat(this.Settings.Indent, this.ClrFullName.Split('.').Length - this.Settings.ClrRootNamespace.Split('.').Length));

        bool IsRoot =>
            this.Settings.ClrRootNamespace == this.ClrFullName;

        bool IsRootDirectChild =>
            this.ClrFullName.Split('.').Length - this.Settings.ClrRootNamespace.Split('.').Length == 1;

        bool IsTsRoot =>
            (this.IsRoot && !string.IsNullOrWhiteSpace(this.Settings.TsRootNamespace)) || (this.IsRootDirectChild && string.IsNullOrWhiteSpace(this.Settings.TsRootNamespace));

        string TsName =>
            this.ClrFullName.Split('.').Last();

        string TsFullName =>
            this.Settings.ToTsFullName(this.ClrFullName);

        bool IncludeClasses =>
            this.Settings.ClassNamespaceFilter?.Any(f =>
                f.EndsWith("*")
                    ? this.ClrFullName.StartsWith(f.Substring(0, f.IndexOf("*")))
                    : this.ClrFullName == f
            ) ?? false;

        bool IncludeEnums =>
            this.Settings.EnumNamespaceFilter?.Any(f =>
                f.EndsWith("*")
                    ? this.ClrFullName.StartsWith(f.Substring(0, f.IndexOf("*")))
                    : this.ClrFullName == f
            ) ?? false;

        bool IsEmpty =>
            this.Entities.Count + this.Enums.Count == 0 && this.ChildNamespaces.All(ns => ns.IsEmpty);

        bool ContainsEnums =>
            this.Enums.Count > 0 || this.ChildNamespaces.Any(ns => ns.ContainsEnums);

        List<ClassOrInterfaceModel> Entities { get; set; }

        List<EnumModel> Enums { get; set; }

        List<NamespaceModel> ChildNamespaces { get; set; }

        void Initialize()
        {

            List<Assembly> assemblies = this.Settings.AssemblyPaths.Select(ap => Assembly.LoadFrom(ap)).ToList();
            foreach (var a in assemblies)
            {
                a.GetReferencedAssemblies();
            }


            // Processing entities (class types)
            var allClassTypes = this.GetTypes(assemblies)
                .Where(t => (t.IsClass || t.IsInterface) && t.IsPublic && t.Namespace.StartsWith(this.ClrFullName, StringComparison.Ordinal))
                .ToList();

            // Processing enums
            var allEnumTypes = this.GetTypes(assemblies)
                .Where(t => t.IsEnum && t.IsPublic && t.Namespace.StartsWith(this.ClrFullName, StringComparison.Ordinal))
                .ToList();

            if(this.IncludeClasses)
            {
                var nsClassTypes = allClassTypes.Where(t => t.Namespace == this.ClrFullName).ToList();
                nsClassTypes.Sort(delegate (Type type1, Type type2) { return string.Compare(type1.Name, type2.Name, StringComparison.Ordinal); });

                foreach (var classType in nsClassTypes)
                {
                    this.Entities.Add(new ClassOrInterfaceModel(this._globalSettings, classType));
                }
            }

            if (this.IncludeEnums)
            {
                var nsEnumTypes = allEnumTypes.Where(t => t.Namespace == this.ClrFullName).ToList();
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
                .Where(ns => ns.StartsWith($"{this.ClrFullName}.", StringComparison.Ordinal) && ns.Length > this.ClrFullName.Length)
                .Select(ns => string.Format("{0}.{1}", this.ClrFullName, ns.Substring(this.ClrFullName.Length).Split('.')[1]))
                .Distinct()
                .ToList();

            childNamespaces.Sort();

            foreach (var childNamespace in childNamespaces)
            {
                this.ChildNamespaces.Add(new NamespaceModel(this._globalSettings, childNamespace));
            }
        }

        List<Type> GetTypes(List<Assembly> assemblies)
        {
            List<Type> types = new List<Type>();
            foreach (var a in assemblies)
            {
                Type[] assemblyTypes;
                try
                {
                    assemblyTypes = a.GetTypes();
                }
                catch (ReflectionTypeLoadException e)
                {
                    assemblyTypes = e.Types;
                }
                types.AddRange(assemblyTypes.Where(t => t != null));
            }
            return types;

        }

        public override StreamWriter WriteTs(StreamWriter sw)
        {
            if (this.IsRoot)
            {
                List<GeneratorSettings> allGeneratorSettings = new List<GeneratorSettings>
                {
                    this._globalSettings
                };
                allGeneratorSettings.AddRange(this._globalSettings.NamespaceOverrides.Values);
                allGeneratorSettings.AddRange(this._globalSettings.ClassOverrides.Values);

                if (allGeneratorSettings.Any(gs => gs.KnockoutMapping != KnockoutMappingOptions.None && gs.KnockoutMapping != null))
                {
                    if (this.Settings.TypingsPaths != null && this.Settings.TypingsPaths.Knockout != null)
                    {
                        sw.WriteFormat(
                            "/// <reference path=\"{0}\"/>{1}",
                            this.Settings.TypingsPaths.Knockout,
                            Environment.NewLine
                        );
                    }
                }

                if (allGeneratorSettings.Any(gs => gs.UseBreeze == true))
                {
                    if (this.Settings.TypingsPaths != null && this.Settings.TypingsPaths.Breeze == null)
                    {
                        sw.WriteFormat(
                            "/// <reference path=\"{0}\"/>{1}",
                            this.Settings.TypingsPaths.Breeze,
                            Environment.NewLine
                        );
                    }
                }
            }

            if(!this.IsEmpty)
            {

                sw.WriteLine();

                if (!(this.IsRoot && !this.IsTsRoot))
                {
                    sw.WriteLine($"{this.IndentationContext}export namespace {this.TsName} {{");
                }

                foreach (var ns in this.ChildNamespaces)
                {
                    ns.WriteTs(sw);
                }

                if (!(this.IsRoot && !this.IsTsRoot))
                {

                    foreach (var entity in this.Entities)
                    {
                        entity.WriteTs(sw);
                    }

                    foreach (var enumModel in this.Enums)
                    {
                        enumModel.WriteTs(sw);
                    }

                    sw.WriteLine();
                    sw.WriteLine($"{this.IndentationContext}}}");
                }
            }

            return sw;
        }

    }
}

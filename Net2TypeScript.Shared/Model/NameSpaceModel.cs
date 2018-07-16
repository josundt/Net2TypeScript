using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace jasMIN.Net2TypeScript.Shared.Model
{
    class NamespaceModel : ClrModelBase
    {
        public NamespaceModel(GlobalSettings settings, string name)
            : base(settings)
        {
            this.ClrFullName = name;
            this.Entities = new List<ClassOrInterfaceModel>();
            this.Enums = new List<EnumModel>();
            this.ChildNamespaces = new List<NamespaceModel>();

            Initialize();
        }

        protected override string IndentationContext =>
                string.Concat(Enumerable.Repeat(Settings.Indent, ClrFullName.Split('.').Length - Settings.ClrRootNamespace.Split('.').Length));

        bool IsRoot => 
            Settings.ClrRootNamespace == this.ClrFullName;

        bool IsDirectChildOfRoot =>
            this.ClrFullName.Split('.').Length - Settings.ClrRootNamespace.Split('.').Length == 1;

        bool IsTsRoot =>
            (this.IsRoot && !string.IsNullOrWhiteSpace(Settings.TsRootNamespace)) || (this.IsDirectChildOfRoot && string.IsNullOrWhiteSpace(Settings.TsRootNamespace));

        string TsName => 
            ClrFullName.Split('.').Last();

        string TsFullName =>
            Settings.ToTsFullName(ClrFullName);

        bool IncludeClasses => 
            Settings.ClassNamespaceFilter == null || Settings.ClassNamespaceFilter.Contains(ClrFullName);

        bool IncludeEnums => 
            Settings.EnumNamespaceFilter == null || Settings.EnumNamespaceFilter.Contains(ClrFullName);

        bool IsEmpty =>
            Entities.Count + Enums.Count == 0 && ChildNamespaces.All(ns => ns.IsEmpty);

        bool ContainsEnums =>
            Enums.Count > 0 || ChildNamespaces.Any(ns => ns.ContainsEnums);

        List<ClassOrInterfaceModel> Entities { get; set; }

        List<EnumModel> Enums { get; set; }

        List<NamespaceModel> ChildNamespaces { get; set; }

        void Initialize()
        {

            List<Assembly> assemblies = Settings.AssemblyPaths.Select(ap => Assembly.LoadFrom(ap)).ToList();
            foreach (var a in assemblies)
            {
                a.GetReferencedAssemblies();
            }


            // Processing entities (class types)
            var allClassTypes = GetTypes(assemblies)
                .Where(t => (t.IsClass || t.IsInterface) && t.IsPublic && t.Namespace.StartsWith(ClrFullName, StringComparison.Ordinal))
                .ToList();

            // Processing enums
            var allEnumTypes = GetTypes(assemblies)
                .Where(t => t.IsEnum && t.IsPublic && t.Namespace.StartsWith(ClrFullName, StringComparison.Ordinal))
                .ToList();

            if(IncludeClasses)
            {
                var nsClassTypes = allClassTypes.Where(t => t.Namespace == ClrFullName).ToList();
                nsClassTypes.Sort(delegate (Type type1, Type type2) { return string.Compare(type1.Name, type2.Name, StringComparison.Ordinal); });

                foreach (var classType in nsClassTypes)
                {
                    this.Entities.Add(new ClassOrInterfaceModel(this._globalSettings, classType));
                }
            }


            if (IncludeEnums)
            {
                var nsEnumTypes = allEnumTypes.Where(t => t.Namespace == ClrFullName).ToList();
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
                .Where(ns => ns.StartsWith(this.ClrFullName, StringComparison.Ordinal) && ns.Length > this.ClrFullName.Length)
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

        public override void AppendTs(StringBuilder sb)
        {
            if (IsRoot)
            {
                List<GeneratorSettings> allGeneratorSettings = new List<GeneratorSettings> ();
                allGeneratorSettings.Add(this._globalSettings);
                allGeneratorSettings.AddRange(this._globalSettings.NamespaceOverrides.Values);
                allGeneratorSettings.AddRange(this._globalSettings.ClassOverrides.Values);

                if (allGeneratorSettings.Any(gs => gs.KnockoutMapping != KnockoutMappingOptions.None && gs.KnockoutMapping != null))
                {
                    if (Settings.TypingsPaths != null && Settings.TypingsPaths.Knockout != null)
                    {
                        sb.AppendFormat("/// <reference path=\"{0}\"/>\r\n", Settings.TypingsPaths.Knockout);
                    }
                }

                if (allGeneratorSettings.Any(gs => gs.UseBreeze == true))
                {
                    if (Settings.TypingsPaths != null && Settings.TypingsPaths.Breeze == null)
                    {
                        sb.AppendFormat("/// <reference path=\"{0}\"/>\r\n", Settings.TypingsPaths.Breeze);
                    }
                }
            }

            if(!IsEmpty)
            {

                sb.AppendLine();

                if (!(this.IsRoot && !this.IsTsRoot))
                {
                    sb.AppendLine($"{IndentationContext}export namespace {TsName} {{");
                }

                foreach (var ns in this.ChildNamespaces)
                {
                    ns.AppendTs(sb);
                }

                if (!(this.IsRoot && !this.IsTsRoot))
                {

                    foreach (var entity in this.Entities)
                    {
                        entity.AppendTs(sb);
                    }

                    foreach (var enumModel in this.Enums)
                    {
                        enumModel.AppendTs(sb);
                    }

                    sb.AppendLine();
                    sb.AppendLine($"{IndentationContext}}}");
                }
            }

        }

    }
}

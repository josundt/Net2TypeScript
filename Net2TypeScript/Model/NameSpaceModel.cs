using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace jasMIN.Net2TypeScript.Model
{
    class NameSpaceModel : ClrModelBase
    {
        public NameSpaceModel(Settings settings, string name)
            : base(settings)
        {
            this.ClrTypeName = name;
            this.Entities = new List<ClassModel>();
            this.Enums = new List<EnumModel>();
            this.ChildNamespaces = new List<NameSpaceModel>();

            Initialize();
        }

        protected override string Indent =>
                string.Concat(Enumerable.Repeat(Settings.tab, ClrTypeName.Split('.').Length - Settings.clrRootNamespace.Split('.').Length));

        bool IsRoot => Settings.clrRootNamespace == this.ClrTypeName;
        string TsName => (Settings.tsRootNamespace + ClrTypeName.Remove(0, Settings.clrRootNamespace.Length)).Split('.').Last();
        string TsFullName => (Settings.tsRootNamespace + ClrTypeName.Remove(0, Settings.clrRootNamespace.Length));

        List<ClassModel> Entities { get; set; }
        List<EnumModel> Enums { get; set; }
        List<NameSpaceModel> ChildNamespaces { get; set; }

        void Initialize()
        {
            Assembly assembly = Assembly.LoadFrom(Settings.assemblyPath);
            assembly.GetReferencedAssemblies();

            // Processing entities (class types)
            var allClassTypes = assembly.GetTypes()
                .Where(t => t.IsClass && t.IsPublic && t.Namespace.StartsWith(ClrTypeName, StringComparison.Ordinal))
                .ToList();

            var nsClassTypes = allClassTypes.Where(t => t.Namespace == ClrTypeName).ToList();
            nsClassTypes.Sort(delegate (Type type1, Type type2) { return string.Compare(type1.Name, type2.Name, StringComparison.Ordinal); });

            foreach (var classType in nsClassTypes)
            {
                this.Entities.Add(new ClassModel(Settings, classType));
            }

            // Processing enums
            var allEnumTypes = assembly.GetTypes()
                .Where(t => t.IsEnum && t.IsPublic && t.Namespace.StartsWith(ClrTypeName, StringComparison.Ordinal))
                .ToList();

            var nsEnumTypes = allEnumTypes.Where(t => t.Namespace == ClrTypeName).ToList();
            nsEnumTypes.Sort(delegate (Type type1, Type type2) { return string.Compare(type1.Name, type2.Name, StringComparison.Ordinal); });

            foreach (var enumType in nsEnumTypes)
            {
                this.Enums.Add(new EnumModel(Settings, enumType));
            }


            // Processing direct child namespaces
            var allNamespaces = allClassTypes.Select(t => t.Namespace).Distinct();
            var childNamespaces = allNamespaces.Where(ns => ns.Remove(0, ClrTypeName.Length).Split('.').Length == 2).ToList();

            foreach (var childNamespace in childNamespaces)
            {
                this.ChildNamespaces.Add(new NameSpaceModel(Settings, childNamespace));
            }
        }

        public override void AppendTs(StringBuilder sb)
        {
            if (IsRoot)
            {
                if (Settings.useKnockout)
                {
                    sb.AppendFormat("/// <reference path=\"{0}\"/>\r\n", Path.Combine(Settings.definitelyTypedRelPath, "knockout/knockout.d.ts"));
                }
                if (Settings.useBreeze)
                {
                    sb.AppendFormat("/// <reference path=\"{0}\"/>\r\n", Path.Combine(Settings.definitelyTypedRelPath, "breeze/breeze.d.ts"));
                }
            }


            sb.AppendLine();

            var possiblyDeclare = IsRoot ? "declare " : "";
            sb.AppendLine($"{Indent}{possiblyDeclare}namespace {TsName} {{");

            foreach(var entity in this.Entities)
            {
                entity.AppendTs(sb);
            }

            foreach (var enumModel in this.Enums)
            {
                enumModel.AppendTs(sb);
            }

            foreach (var ns in this.ChildNamespaces)
            {
                ns.AppendTs(sb);
            }

            sb.AppendLine();
            sb.AppendLine($"{Indent}}}");
        }
    }
}

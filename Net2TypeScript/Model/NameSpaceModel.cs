using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Configuration;
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
            this.ClrFullName = name;
            this.Entities = new List<ClassModel>();
            this.Enums = new List<EnumModel>();
            this.ChildNamespaces = new List<NameSpaceModel>();

            Initialize();
        }

        protected override string IndentationContext =>
                string.Concat(Enumerable.Repeat(Settings.indent, ClrFullName.Split('.').Length - Settings.clrRootNamespace.Split('.').Length));

        bool IsRoot => 
            Settings.clrRootNamespace == this.ClrFullName;

        string TsName => 
            (Settings.tsRootNamespace + ClrFullName.Remove(0, Settings.clrRootNamespace.Length)).Split('.').Last();

        string TsFullName => 
            (Settings.tsRootNamespace + ClrFullName.Remove(0, Settings.clrRootNamespace.Length));

        bool IncludeClasses => 
            Settings.classNamespaceFilter == null || Settings.classNamespaceFilter.Contains(ClrFullName);

        bool IncludeEnums => 
            Settings.enumNamespaceFilter == null || Settings.enumNamespaceFilter.Contains(ClrFullName);

        bool IsEmpty =>
            Entities.Count + Enums.Count == 0 && ChildNamespaces.All(ns => ns.IsEmpty);

        bool ContainsEnums =>
            Enums.Count > 0 || ChildNamespaces.Any(ns => ns.ContainsEnums);

        List<ClassModel> Entities { get; set; }

        List<EnumModel> Enums { get; set; }

        List<NameSpaceModel> ChildNamespaces { get; set; }

        void Initialize()
        {

            Assembly assembly = Assembly.LoadFrom(Settings.assemblyPath);
            var refs = assembly.GetReferencedAssemblies();


            // Processing entities (class types)
            var allClassTypes = GetTypes(assembly)
                .Where(t => t.IsClass && t.IsPublic && t.Namespace.StartsWith(ClrFullName, StringComparison.Ordinal))
                .ToList();

            // Processing enums
            var allEnumTypes = GetTypes(assembly)
                .Where(t => t.IsEnum && t.IsPublic && t.Namespace.StartsWith(ClrFullName, StringComparison.Ordinal))
                .ToList();

            if(IncludeClasses)
            {
                var nsClassTypes = allClassTypes.Where(t => t.Namespace == ClrFullName).ToList();
                nsClassTypes.Sort(delegate (Type type1, Type type2) { return string.Compare(type1.Name, type2.Name, StringComparison.Ordinal); });

                foreach (var classType in nsClassTypes)
                {
                    this.Entities.Add(new ClassModel(Settings, classType));
                }
            }


            if (IncludeEnums)
            {
                var nsEnumTypes = allEnumTypes.Where(t => t.Namespace == ClrFullName).ToList();
                nsEnumTypes.Sort(delegate (Type type1, Type type2) { return string.Compare(type1.Name, type2.Name, StringComparison.Ordinal); });

                foreach (var enumType in nsEnumTypes)
                {
                    this.Enums.Add(new EnumModel(Settings, enumType));
                }
            }

            // Processing direct child namespaces
            var allNamespaces = allClassTypes.Select(t => t.Namespace).Concat(allEnumTypes.Select(t => t.Namespace)).Distinct();
            var childNamespaces = allNamespaces.Where(ns => ns.Remove(0, ClrFullName.Length).Split('.').Length == 2).ToList();

            foreach (var childNamespace in childNamespaces)
            {
                this.ChildNamespaces.Add(new NameSpaceModel(Settings, childNamespace));
            }
        }

        Type[] GetTypes(Assembly assembly)
        {
            Type[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch(ReflectionTypeLoadException e)
            {
                types = e.Types;
            }
            return types.Where(t => t != null).ToArray();

        }

        public override void AppendTs(StringBuilder sb)
        {
            if (IsRoot)
            {
                if (Settings.useKnockout)
                {
                    if (Settings.typingsPaths == null || Settings.typingsPaths.knockout == null)
                    {
                        throw new ConfigurationErrorsException("When useKnockout is set to true, typingsPaths.knockout must be specified.");
                    }
                    sb.AppendFormat("/// <reference path=\"{0}\"/>\r\n", Settings.typingsPaths.knockout);
                }
                if (Settings.useBreeze)
                {
                    if (Settings.typingsPaths == null || Settings.typingsPaths.breeze == null)
                    {
                        throw new ConfigurationErrorsException("When useBreeze is set to true, typingsPaths.breeze must be specified.");
                    }
                    sb.AppendFormat("/// <reference path=\"{0}\"/>\r\n", Settings.typingsPaths.breeze);
                }
            }

            if(!IsEmpty)
            {

                sb.AppendLine();

                var possiblyDeclare = IsRoot ? "declare " : "";
                sb.AppendLine($"{IndentationContext}{possiblyDeclare}namespace {TsName} {{");

                foreach (var entity in this.Entities)
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
                sb.AppendLine($"{IndentationContext}}}");

            }

        }

        public void AppendEnums(StringBuilder sb)
        {

            if (ContainsEnums)
            {


                sb.AppendLine();

                if (IsRoot)
                {
                    sb.AppendFormat("/// <reference path=\"{0}\"/>\r\n", Settings.modelModuleOutputPath.GetRelativePathTo(Settings.declarationsOutputPath));
                    sb.AppendLine();
                    sb.AppendLine("/* tslint:disable:variable-name */");
                    sb.AppendLine();
                }

                var possiblyExport = IsRoot ? string.Empty : "export";
                var namespaceName = IsRoot ? $"{TsName}Enums" : TsName;

                sb.AppendLine($"{IndentationContext}{possiblyExport} namespace {namespaceName} {{");

                foreach (var enumModel in this.Enums)
                {
                    enumModel.AppendEnums(sb);
                }

                foreach (var ns in this.ChildNamespaces)
                {
                    ns.AppendEnums(sb);
                }

                sb.AppendLine();
                sb.AppendLine($"{IndentationContext}}}");

                if (IsRoot)
                {
                    sb.AppendLine();
                    sb.AppendLine($"{IndentationContext}export let {TsName} = {namespaceName};");

                    sb.AppendLine();
                    sb.AppendLine("/* tslint:enable:variable-name */");
                }

            }
        }
    }
}

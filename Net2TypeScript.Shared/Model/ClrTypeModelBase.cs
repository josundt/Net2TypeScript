using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace jasMIN.Net2TypeScript.Shared.Model
{
    abstract class ClrTypeModelBase : ClrModelBase
    {
        private static readonly Regex GenericTypeNameFixRegEx = new Regex(@"`.*$", RegexOptions.Compiled);

        protected ClrTypeModelBase(Type type, GlobalSettings globalSettings)
            : base(globalSettings)
        {
            this.Type = type;
            if (type.IsGenericTypeDefinition)
            {
                var genericArgNames = type.GetGenericArguments().Select(a => a.Name);

                var baseFullName = GenericTypeNameFixRegEx.Replace(type.FullName, string.Empty);
                this.ClrFullName = $"{baseFullName}<{string.Join(", ", genericArgNames)}>";

                var baseName = GenericTypeNameFixRegEx.Replace(type.Name, string.Empty);
                this.TsTypeName = $"{baseName}<{string.Join(", ", genericArgNames)}>";
            }
            else
            {
                this.ClrFullName = type.FullName;
                this.TsTypeName = type.Name;
            }
        }

        public override abstract StreamWriter WriteTs(StreamWriter sw);

        protected override string IndentationContext =>
            string.Concat(Enumerable.Repeat(this.Settings.Indent, this.Type.Namespace.Split('.').Length - this.Settings.ClrRootNamespace.Split('.').Length + this.ExtraIndents));

        protected Type Type { get; private set; }

        protected virtual string TsTypeName { get; }

        protected virtual string TsFullName => this.Settings.ToTsFullName(this.ClrFullName);

        protected abstract int ExtraIndents { get; }

    }
}

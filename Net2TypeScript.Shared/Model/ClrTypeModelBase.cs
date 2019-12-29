using System;
using System.IO;
using System.Linq;

namespace jasMIN.Net2TypeScript.Shared.Model
{
    abstract class ClrTypeModelBase : ClrModelBase
    {
        protected ClrTypeModelBase(Type type, GlobalSettings globalSettings)
            : base(globalSettings)
        {
            this.Type = type;
            this.ClrFullName = Type.FullName;
        }

        public override abstract StreamWriter WriteTs(StreamWriter sw);

        protected override string IndentationContext =>
            string.Concat(Enumerable.Repeat(Settings.Indent, Type.Namespace.Split('.').Length - Settings.ClrRootNamespace.Split('.').Length + ExtraIndents));

        protected Type Type { get; private set; }

        protected virtual string TsTypeName => Type.Name;
        
        protected virtual string TsFullName => Settings.ToTsFullName(ClrFullName);

        protected abstract int ExtraIndents { get; }

    }
}

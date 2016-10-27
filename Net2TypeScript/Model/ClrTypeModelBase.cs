﻿using System;
using System.Linq;
using System.Text;

namespace jasMIN.Net2TypeScript.Model
{
    abstract class ClrTypeModelBase : ClrModelBase
    {
        protected ClrTypeModelBase(Type type, GlobalSettings globalSettings)
            : base(globalSettings)
        {
            this.Type = type;
            this.ClrFullName = Type.FullName;
        }
        public override abstract void AppendTs(StringBuilder sb);

        protected override string IndentationContext =>
            string.Concat(Enumerable.Repeat(Settings.indent, Type.Namespace.Split('.').Length - Settings.clrRootNamespace.Split('.').Length + ExtraIndents));

        protected Type Type { get; private set; }
        protected virtual string TsTypeName => Type.Name;
        protected virtual string TsFullName => Settings.ToTsFullName(ClrFullName);

        protected abstract int ExtraIndents { get; }

    }
}

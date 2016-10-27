using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace jasMIN.Net2TypeScript.Model
{
    class ClassModel : ClrTypeModelBase
    {
        public ClassModel(GlobalSettings globalSettings, Type type)
            : base(type, globalSettings)
        {
            if (!type.IsClass || !type.IsPublic)
            {
                throw new InvalidOperationException("Not a public class type");
            }
            this.Properties = new List<PropertyModel>();
            this.Initialize();
        }

        protected override int ExtraIndents => 1;

        List<PropertyModel> Properties { get; set; }

        void Initialize()
        {
            foreach (PropertyInfo propertyInfo in Type.GetProperties())
            {
                this.Properties.Add(new PropertyModel(this._globalSettings, propertyInfo, Type));
            }
        }

        public override void AppendTs(StringBuilder sb)
        {

            sb.AppendFormat("\r\n{0}/** Class: {1} ({2}) */\r\n",
                IndentationContext,
                TsFullName,
                ClrFullName);

            sb.AppendFormat("{0}interface {1}{2} {{\r\n",
                IndentationContext,
                TsTypeName,
                Settings.useBreeze == true ? " extends breeze.Entity" : string.Empty);

            // TODO: Filter non-public props
            // RENDER PROPERTYINFOS
            foreach (var prop in this.Properties)
            {
                prop.AppendTs(sb);
            }


            if (Settings.extraProperties != null)
            {
                foreach (KeyValuePair<string, string> prop in Settings.extraProperties)
                {
                    AppendExtensionProperty(sb, prop, Settings);
                }
            }

            //if (Settings.perTypeExtensions != null)
            //{
            //    foreach (KeyValuePair<string, Dictionary<string, string>> typeExtensionProp in Settings.perTypeExtensions)
            //    {
            //        var targetClassType = typeExtensionProp.ToString().Substring(1, typeExtensionProp.ToString().Length - 2).Split(',')[0].Trim();
            //        if (Type.FullName.Equals(targetClassType))
            //        {
            //            foreach (var prop in typeExtensionProp.Value)
            //            {
            //                AppendExtensionProperty(sb, prop, Settings);
            //            }
            //        }
            //    }
            //}

            sb.AppendLine($"{IndentationContext}}}");
        }

        void AppendExtensionProperty(StringBuilder sb, KeyValuePair<string, string> prop, Settings settings)
        {
            var tsPropName = prop.Key;
            var tsTypeName = prop.Value.ToString();

            sb.AppendFormat("{0}{1}: {2};\r\n",
                            IndentationContext + settings.indent,
                            tsPropName,
                            tsTypeName);
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace jasMIN.Net2TypeScript.Model
{
    class ClassOrInterfaceModel : ClrTypeModelBase
    {
        public ClassOrInterfaceModel(GlobalSettings globalSettings, Type type)
            : base(type, globalSettings)
        {
            var fullName = type.FullName;

            var isValidType = (type.IsClass || type.IsInterface) && type.IsPublic;

            if (!isValidType)
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
            foreach (PropertyInfo propertyInfo in Type.GetProperties().Where(prop => prop.CanRead && prop.GetGetMethod().IsPublic))
            {
                var isDefinedAsExtraProp =
                    this.Settings.extraProperties.Any(ep => ep.Key == (Settings.camelCase ? propertyInfo.Name.ToCamelCase() : propertyInfo.Name));

                if (!isDefinedAsExtraProp) {
                    this.Properties.Add(new PropertyModel(this._globalSettings, propertyInfo, Type));
                }
            }
        }

        public override void AppendTs(StringBuilder sb)
        {

            var skip =
                (this.Type.IsClass && this.Settings.excludeClass == true) ||
                (this.Type.IsInterface && this.Settings.excludeInterface == true) ||
                this.Properties.Count == 0;


            if (!skip) {

                sb.AppendFormat("\r\n{0}/** {1}: {2} ({3}) */\r\n",
                    IndentationContext,
                    this.Type.IsClass ? "Class" : "Interface",
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
                        if (!string.IsNullOrEmpty(prop.Value))
                        {
                            AppendExtensionProperty(sb, prop, Settings);
                        }
                    }
                }

                sb.AppendLine($"{IndentationContext}}}");

            }

        }

        void AppendExtensionProperty(StringBuilder sb, KeyValuePair<string, string> prop, Settings settings)
        {
            var tsPropName = prop.Key;
            var tsTypeName = prop.Value.ToString();
            sb.AppendFormat("{0}/** Extra/overridden property */\r\n",
                            IndentationContext + settings.indent);
            sb.AppendFormat("{0}{1}: {2};\r\n",
                            IndentationContext + settings.indent,
                            tsPropName,
                            tsTypeName);
        }

    }
}

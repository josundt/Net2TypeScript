using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace jasMIN.Net2TypeScript.Shared.Model
{

    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    class ClassOrInterfaceModel : ClrTypeModelBase
    {
        public ClassOrInterfaceModel(GlobalSettings globalSettings, Type type)
            : base(type, globalSettings)
        {
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
            var type = this.Type;

            while (type != null)
            {
                foreach (PropertyInfo propertyInfo in type.GetProperties().Where(prop => prop.CanRead && prop.GetGetMethod().IsPublic))
                {
                    var isDefinedAsExtraProp =
                        this.Settings.ExtraProperties.Any(ep => ep.Key == (this.Settings.CamelCase ? propertyInfo.Name.ToCamelCase() : propertyInfo.Name));

                    if (!isDefinedAsExtraProp)
                    {
                        this.Properties.Add(new PropertyModel(this._globalSettings, propertyInfo, this.Type));
                    }
                }

                type = this.Type.IsInterface ? this.GetBaseType(type) : null;
            }
        }

        private string DebuggerDisplay => @$"{(this.Type.IsClass ? "class" : "interface")}: {this.Type.Name}";

        private Type GetBaseType(Type classOrInterface)
        {
            Type result = null;
            if (classOrInterface.IsInterface)
            {
                var interfaces = classOrInterface.GetInterfaces();
                if (interfaces != null && interfaces.Length > 0)
                {
                    result = interfaces[0];
                }
            }
            else if (classOrInterface.IsClass)
            {
                result = this.Type.BaseType;
            }

            // If base type not in the included assemblies, skip it
            if (result != null)
            {
                var baseTypeAssemblyName = result.Assembly.FullName.Split(',')[0];
                if (!this._globalSettings.AssemblyPaths.Any(ap => ap.Contains(baseTypeAssemblyName))) {
                    result = null;
                }
                else if (result == classOrInterface)
                {
                    result = null;
                }
            }
            return result;
        }

        public override StreamWriter WriteTs(StreamWriter sw)
        {

            var skip =
                (this.Type.IsClass && this.Settings.ExcludeClass == true) ||
                (this.Type.IsInterface && this.Settings.ExcludeInterface == true) ||
                this.Properties.Count == 0;


            if (!skip)
            {

                sw.WriteFormat("{3}{0}/** .NET {1}: {2} */{3}",
                    this.IndentationContext,
                    this.Type.IsClass ? "class" : "interface",
                    this.ClrFullName,
                    Environment.NewLine
                );

                sw.WriteFormat("{0}export interface {1}{2} {{{3}",
                    this.IndentationContext,
                    this.TsTypeName,
                    this.Settings.UseBreeze == true ? " extends breeze.Entity" : string.Empty,
                    Environment.NewLine
                );

                // TODO: Filter non-public props
                // RENDER PROPERTYINFOS
                foreach (var prop in this.Properties)
                {
                    prop.WriteTs(sw);
                }


                if (this.Settings.ExtraProperties != null)
                {
                    foreach (KeyValuePair<string, string> prop in this.Settings.ExtraProperties)
                    {
                        if (!string.IsNullOrEmpty(prop.Value))
                        {
                            this.WriteExtensionProperty(sw, prop, this.Settings);
                        }
                    }
                }

                sw.WriteLine($"{this.IndentationContext}}}");

            }

            return sw;
        }

        void WriteExtensionProperty(StreamWriter sw, KeyValuePair<string, string> prop, Settings settings)
        {
            var tsPropName = prop.Key;
            var tsTypeName = prop.Value.ToString();
            sw.WriteFormat(
                "{0}/** Extra/overridden property */{1}",
                this.IndentationContext + settings.Indent,
                Environment.NewLine
            );
            sw.WriteFormat(
                "{0}{1}: {2};{3}",
                this.IndentationContext + settings.Indent,
                tsPropName,
                tsTypeName,
                Environment.NewLine
            );
        }

    }
}

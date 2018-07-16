using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace jasMIN.Net2TypeScript.Shared.Model
{
    public class TypingsPaths
    {
        public string Knockout { get; set; }
        public string Breeze { get; set; } 
    }

    public static class KnockoutMappingOptions
    {
        public static string None = "none";
        public static string All = "all";
        public static string ValueTypes = "valuetypes";
    }

    public class GeneratorSettings
    {
        public GeneratorSettings()
        {
            this.ExtraProperties = new Dictionary<string, string>();
        }

        public string KnockoutMapping { get; set; }
        public bool? UseBreeze { get; set; }
        public bool? ExcludeClass { get; set; }
        public bool? ExcludeInterface { get; set; }
        public Dictionary<string, string> ExtraProperties { get; set; }
        public bool? NonNullableEntities { get; set; }
        public bool? NonNullableArrays { get; set; }
        public bool? NonNullableArrayEntityItems { get; set; }


        public GeneratorSettings Clone()
        {
            var clone = (GeneratorSettings)this.MemberwiseClone();
            clone.ExtraProperties = new Dictionary<string, string>();
            foreach (var kvp in this.ExtraProperties)
            {
                clone.ExtraProperties.Add(kvp.Key, kvp.Value);
            }
            return clone;
        }

        protected T Merge<T>(T initialSetting, params GeneratorSettings[] generatorSettingsColletion) where T : GeneratorSettings, new()
        {
            var result = initialSetting;

            foreach (var genSetting in generatorSettingsColletion.Where(gs => gs != null))
            {
                result = result ?? new T();

                result.ExcludeClass = genSetting.ExcludeClass ?? result.ExcludeClass;
                result.ExcludeInterface = genSetting.ExcludeInterface ?? result.ExcludeInterface;
                result.KnockoutMapping = genSetting.KnockoutMapping ?? result.KnockoutMapping;
                result.UseBreeze = genSetting.UseBreeze ?? result.UseBreeze;
                result.NonNullableArrayEntityItems = genSetting.NonNullableArrayEntityItems ?? result.NonNullableArrayEntityItems;
                result.NonNullableEntities = genSetting.NonNullableEntities ?? result.NonNullableEntities;
                result.NonNullableArrays = genSetting.NonNullableArrays ?? result.NonNullableArrays;

                foreach (var kvp in genSetting.ExtraProperties)
                {
                    if (result.ExtraProperties.ContainsKey(kvp.Key))
                    {
                        result.ExtraProperties.Remove(kvp.Key);
                    }
                    result.ExtraProperties.Add(kvp.Key, kvp.Value);
                }
            }

            return result;
        }

    }

    public class Settings : GeneratorSettings
    {
        public Settings()
        {
            // Initializing default values
            this.Indent = "    ";
            this.EnumType = "string";
            this.TsRootNamespace = this.ClrRootNamespace;
            this.ExtraProperties = new Dictionary<string, string>();
        }
        public List<string> AssemblyPaths { get; set; }
        public string OutputPath { get; set; }
        public string ClrRootNamespace { get; set; }
        public string TsRootNamespace { get; set; }
        public string EnumType { get; set; }
        public List<string> ClassNamespaceFilter { get; set; }
        public List<string> EnumNamespaceFilter { get; set; }
        public string Indent { get; set; }
        public bool CamelCase { get; set; }
        public TypingsPaths TypingsPaths { get; set; }

        public new Settings Clone()
        {
            var clone = (Settings)this.MemberwiseClone();

            clone.ExtraProperties = new Dictionary<string, string>();
            foreach (var kvp in this.ExtraProperties)
            {
                clone.ExtraProperties.Add(kvp.Key, kvp.Value);
            }

            clone.AssemblyPaths = new List<string>();
            foreach (var s in this.AssemblyPaths)
            {
                clone.AssemblyPaths.Add(s);
            }

            if (this.ClassNamespaceFilter != null)
            {
                clone.ClassNamespaceFilter = new List<string>();
                foreach (var s in this.ClassNamespaceFilter)
                {
                    clone.ClassNamespaceFilter.Add(s);
                }
            }

            if (this.EnumNamespaceFilter != null)
            {
                clone.EnumNamespaceFilter = new List<string>();
                foreach (var s in this.EnumNamespaceFilter)
                {
                    clone.EnumNamespaceFilter.Add(s);
                }
            }

            clone.TypingsPaths = this.TypingsPaths == null ? null : new TypingsPaths
            {
                Breeze = this.TypingsPaths.Breeze,
                Knockout = this.TypingsPaths.Knockout
            };
            return clone;
        }
    }

    public class GlobalSettings : Settings
    {
        public Dictionary<string, GeneratorSettings> NamespaceOverrides { get; set; }
        public Dictionary<string, GeneratorSettings> ClassOverrides { get; set; }

        public GlobalSettings()
        {
            this.NamespaceOverrides = new Dictionary<string, GeneratorSettings>();
            this.ClassOverrides = new Dictionary<string, GeneratorSettings>();
        }

        Settings GetEffectiveSettings(GeneratorSettings genSettings)
        {
            var result = Merge(this.Clone(), genSettings) as Settings;

            return result;
        }

        public Settings GetNamespaceSettings(string namespaceName)
        {
            var applicableNsSettings = this.NamespaceOverrides
                .Where(kvp => namespaceName.StartsWith(kvp.Key, StringComparison.Ordinal))
                .OrderBy(kvp => kvp.Key)
                .Select(kvp => kvp.Value);

            var effectiveGenSettings = Merge(this.Clone(), applicableNsSettings.ToArray());

            return this.GetEffectiveSettings(effectiveGenSettings);
        }

        public Settings GetClassSettings(string className)
        {
            var nsSettings = this.GetNamespaceSettings(className);

            var classSettings = this.ClassOverrides.ContainsKey(className) ? this.ClassOverrides[className] : null;

            var effectiveGenSettings = Merge(nsSettings.Clone(), classSettings);

            return this.GetEffectiveSettings(effectiveGenSettings);
        }
    }





}

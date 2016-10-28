using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace jasMIN.Net2TypeScript.Model
{
    public class TypingsPaths
    {
        public string knockout { get; set; }
        public string breeze { get; set; } 
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
            this.extraProperties = new Dictionary<string, string>();
        }

        public string knockoutMapping { get; set; }
        public bool? useBreeze { get; set; }
        public Dictionary<string, string> extraProperties { get; set; }


        public GeneratorSettings Clone()
        {
            var clone = (GeneratorSettings)this.MemberwiseClone();
            clone.extraProperties = new Dictionary<string, string>();
            foreach (var kvp in this.extraProperties)
            {
                clone.extraProperties.Add(kvp.Key, kvp.Value);
            }
            return clone;
        }

        protected GeneratorSettings Merge(GeneratorSettings initialSetting, params GeneratorSettings[] generatorSettingsColletion)
        {
            var result = initialSetting != null ? initialSetting.Clone() : null;

            foreach (var genSetting in generatorSettingsColletion.Where(gs => gs != null))
            {
                result = result ?? new GeneratorSettings();
                result.useBreeze = genSetting.useBreeze.HasValue ? genSetting.useBreeze : result.useBreeze;
                result.knockoutMapping = !string.IsNullOrEmpty(genSetting.knockoutMapping) ? genSetting.knockoutMapping : result.knockoutMapping;
                foreach (var kvp in genSetting.extraProperties)
                {
                    if (!result.extraProperties.ContainsKey(kvp.Key))
                    {
                        result.extraProperties.Remove(kvp.Key);
                    }
                    result.extraProperties.Add(kvp.Key, kvp.Value);
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
            this.indent = "    ";
            this.enumType = "stringliteral";
            this.tsRootNamespace = this.clrRootNamespace;
            this.extraProperties = new Dictionary<string, string>();
        }
        public List<string> assemblyPaths { get; set; }
        public string declarationsOutputPath { get; set; }
        public string modelModuleOutputPath { get; set; }
        public string clrRootNamespace { get; set; }
        public string tsRootNamespace { get; set; }
        public string enumType { get; set; }
        public List<string> classNamespaceFilter { get; set; }
        public List<string> enumNamespaceFilter { get; set; }
        public string indent { get; set; }
        public bool camelCase { get; set; }
        public bool strictNullChecks { get; set; }
        public TypingsPaths typingsPaths { get; set; }

        public new Settings Clone()
        {
            var clone = (Settings)this.MemberwiseClone();

            clone.extraProperties = new Dictionary<string, string>();
            foreach (var kvp in this.extraProperties)
            {
                clone.extraProperties.Add(kvp.Key, kvp.Value);
            }

            clone.assemblyPaths = new List<string>();
            foreach (var s in this.assemblyPaths)
            {
                clone.assemblyPaths.Add(s);
            }

            if (this.classNamespaceFilter != null)
            {
                clone.classNamespaceFilter = new List<string>();
                foreach (var s in this.classNamespaceFilter)
                {
                    clone.classNamespaceFilter.Add(s);
                }
            }

            if (this.enumNamespaceFilter != null)
            {
                clone.enumNamespaceFilter = new List<string>();
                foreach (var s in this.enumNamespaceFilter)
                {
                    clone.enumNamespaceFilter.Add(s);
                }
            }

            clone.typingsPaths = this.typingsPaths == null ? null : new TypingsPaths
            {
                breeze = this.typingsPaths.breeze,
                knockout = this.typingsPaths.knockout
            };
            return clone;
        }

        public Settings GetEffectiveSettings(GeneratorSettings genSettings)
        {
            var result = this.Clone();
            result.useBreeze = genSettings.useBreeze.HasValue ? genSettings.useBreeze : result.useBreeze;
            result.knockoutMapping = !string.IsNullOrEmpty(genSettings.knockoutMapping) ? genSettings.knockoutMapping : result.knockoutMapping;
            result.extraProperties = genSettings.extraProperties;
            return result;
        }


    }

    public class GlobalSettings : Settings
    {
        public Dictionary<string, GeneratorSettings> namespaceOverrides { get; set; }
        public Dictionary<string, GeneratorSettings> classOverrides { get; set; }

        public GlobalSettings()
        {
            this.namespaceOverrides = new Dictionary<string, GeneratorSettings>();
            this.classOverrides = new Dictionary<string, GeneratorSettings>();
        }

        public Settings GetNamespaceSettings(string namespaceName)
        {
            var applicableNsSettings = this.namespaceOverrides
                .Where(kvp => namespaceName.StartsWith(kvp.Key, StringComparison.Ordinal))
                .OrderBy(kvp => kvp.Key)
                .Select(kvp => kvp.Value);

            var effectiveGenSettings = Merge(this, applicableNsSettings.ToArray());

            return this.GetEffectiveSettings(effectiveGenSettings);
        }

        public Settings GetClassSettings(string className)
        {
            var nsSettings = this.GetNamespaceSettings(className);

            var classSettings = this.classOverrides.ContainsKey(className) ? this.classOverrides[className] : null;

            var effectiveGenSettings = Merge(nsSettings, classSettings);

            return this.GetEffectiveSettings(effectiveGenSettings);

        }

    }





}

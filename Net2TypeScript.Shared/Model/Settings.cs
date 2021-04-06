using System;
using System.Collections.Generic;
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
            ExtraProperties = new Dictionary<string, string>();
        }

        public string KnockoutMapping { get; set; }
        public bool? UseBreeze { get; set; }
        public bool? ExcludeClass { get; set; }
        public bool? ExcludeInterface { get; set; }
        public Dictionary<string, string> ExtraProperties { get; set; }
        public bool? NonNullableEntities { get; set; }
        public bool? NonNullableArrays { get; set; }
        public bool? NonNullableArrayEntityItems { get; set; }
        public bool? NonNullableDictionaries { get; set; }
        public bool? NonNullableDictionaryEntityValues { get; set; }

        public GeneratorSettings Clone()
        {
            var clone = (GeneratorSettings)MemberwiseClone();
            clone.ExtraProperties = new Dictionary<string, string>();
            foreach (var kvp in ExtraProperties)
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
                result ??= new T();

                result.ExcludeClass = genSetting.ExcludeClass ?? result.ExcludeClass;
                result.ExcludeInterface = genSetting.ExcludeInterface ?? result.ExcludeInterface;
                result.KnockoutMapping = genSetting.KnockoutMapping ?? result.KnockoutMapping;
                result.UseBreeze = genSetting.UseBreeze ?? result.UseBreeze;
                result.NonNullableArrayEntityItems = genSetting.NonNullableArrayEntityItems ?? result.NonNullableArrayEntityItems;
                result.NonNullableEntities = genSetting.NonNullableEntities ?? result.NonNullableEntities;
                result.NonNullableArrays = genSetting.NonNullableArrays ?? result.NonNullableArrays;
                result.NonNullableDictionaries = genSetting.NonNullableDictionaries ?? result.NonNullableDictionaries;
                result.NonNullableDictionaryEntityValues = genSetting.NonNullableDictionaryEntityValues ?? result.NonNullableDictionaryEntityValues;

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
            Indent = "    ";
            EnumType = "string";
            TsRootNamespace = ClrRootNamespace;
            ExtraProperties = new Dictionary<string, string>();
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
            var clone = (Settings)MemberwiseClone();

            clone.ExtraProperties = new Dictionary<string, string>();
            foreach (var kvp in ExtraProperties)
            {
                clone.ExtraProperties.Add(kvp.Key, kvp.Value);
            }

            clone.AssemblyPaths = new List<string>();
            foreach (var s in AssemblyPaths)
            {
                clone.AssemblyPaths.Add(s);
            }

            if (ClassNamespaceFilter != null)
            {
                clone.ClassNamespaceFilter = new List<string>();
                foreach (var s in ClassNamespaceFilter)
                {
                    clone.ClassNamespaceFilter.Add(s);
                }
            }

            if (EnumNamespaceFilter != null)
            {
                clone.EnumNamespaceFilter = new List<string>();
                foreach (var s in EnumNamespaceFilter)
                {
                    clone.EnumNamespaceFilter.Add(s);
                }
            }

            clone.TypingsPaths = TypingsPaths == null ? null : new TypingsPaths
            {
                Breeze = TypingsPaths.Breeze,
                Knockout = TypingsPaths.Knockout
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
            NamespaceOverrides = new Dictionary<string, GeneratorSettings>();
            ClassOverrides = new Dictionary<string, GeneratorSettings>();
        }

        Settings GetEffectiveSettings(GeneratorSettings genSettings)
        {
            var result = Merge(Clone(), genSettings) as Settings;

            return result;
        }

        public Settings GetNamespaceSettings(string namespaceName)
        {
            var applicableNsSettings = NamespaceOverrides
                .Where(kvp => namespaceName.StartsWith(kvp.Key, StringComparison.Ordinal))
                .OrderBy(kvp => kvp.Key)
                .Select(kvp => kvp.Value);

            var effectiveGenSettings = Merge(Clone(), applicableNsSettings.ToArray());

            return GetEffectiveSettings(effectiveGenSettings);
        }

        public Settings GetClassSettings(string className)
        {
            var nsSettings = GetNamespaceSettings(className);

            var classSettings = ClassOverrides.ContainsKey(className) ? ClassOverrides[className] : null;

            var effectiveGenSettings = Merge(nsSettings.Clone(), classSettings);

            return GetEffectiveSettings(effectiveGenSettings);
        }
    }





}

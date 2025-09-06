namespace jasMIN.Net2TypeScript.SettingsModel;

public class GlobalSettings : Settings
{
    public Dictionary<string, GeneratorSettings> NamespaceOverrides { get; set; }
    public Dictionary<string, GeneratorSettings> ClassOverrides { get; set; }
    public GlobalSettings()
    {
        this.NamespaceOverrides = [];
        this.ClassOverrides = [];
    }

    public Settings GetEffectiveSettings(GeneratorSettings genSettings)
    {
        var result = Merge(this.Clone(), genSettings);

        return result;
    }

    public Settings GetNamespaceSettings(string namespaceName)
    {
        var applicableNsSettings = this.NamespaceOverrides
            .Where(kvp => kvp.Key.EndsWith('*')
                ? namespaceName.StartsWith(kvp.Key[..kvp.Key.LastIndexOf('*')], StringComparison.Ordinal)
                : namespaceName == kvp.Key
            )
            .OrderBy(kvp => kvp.Key)
            .Select(kvp => kvp.Value);

        var effectiveGenSettings = Merge(this.Clone(), [.. applicableNsSettings]);

        return this.GetEffectiveSettings(effectiveGenSettings);
    }

    public Settings GetClassSettings(string className)
    {
        var nsSettings = this.GetNamespaceSettings(className);

        this.ClassOverrides.TryGetValue(className, out GeneratorSettings? classSettings);

        var effectiveGenSettings = classSettings == null ? nsSettings.Clone() : Merge(nsSettings.Clone(), classSettings);

        return this.GetEffectiveSettings(effectiveGenSettings);
    }
}

namespace jasMIN.Net2TypeScript.SettingsModel;

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
    public bool? NonNullableDictionaries { get; set; }
    public bool? NonNullableDictionaryEntityValues { get; set; }
    public bool? NullableReferenceTypes { get; set; }

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

    protected static T Merge<T>(T initialSetting, params GeneratorSettings[] generatorSettingsColletion) where T : GeneratorSettings, new()
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
            result.NullableReferenceTypes = genSetting.NullableReferenceTypes ?? result.NullableReferenceTypes;

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
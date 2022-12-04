namespace jasMIN.Net2TypeScript.SettingsModel;

public class Settings : GeneratorSettings
{
    public Settings()
    {
        // Initializing default values
        this.Indent = "    ";
        this.EnumType = "string";
        this.EnumFormat = "enum";
        this.TsRootNamespace = this.DotNetRootNamespace ?? string.Empty;
        this.ExtraProperties = new Dictionary<string, string>();
        this.AssemblyPaths = new List<string>();
    }
    public List<string> AssemblyPaths { get; set; }
    public string OutputPath { get; set; } = default!;
    public string DotNetRootNamespace { get; set; } = default!;
    public string TsRootNamespace { get; set; }
    public bool TsFlattenNamespaces { get; set; }
    public string EnumType { get; set; }
    public string EnumFormat { get; set; }
    public List<string>? ClassNamespaceFilter { get; set; }
    public List<string>? EnumNamespaceFilter { get; set; }
    public string Indent { get; set; }
    public bool CamelCase { get; set; }
    public TypingsPaths? TypingsPaths { get; set; }

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


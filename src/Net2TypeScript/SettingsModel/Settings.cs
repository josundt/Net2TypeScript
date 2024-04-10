using System.Collections.ObjectModel;

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
        this.ExtraProperties = [];
        this.AssemblyPaths = [];
    }
    public Collection<string> AssemblyPaths { get; set; }
    public string OutputPath { get; set; } = default!;
    public string DotNetRootNamespace { get; set; } = default!;
    public string TsRootNamespace { get; set; }
    public bool TsFlattenNamespaces { get; set; }
    public string EnumType { get; set; }
    public string EnumFormat { get; set; }
    public Collection<string>? ClassNamespaceFilter { get; set; }
    public Collection<string>? EnumNamespaceFilter { get; set; }
    public string Indent { get; set; }
    public bool CamelCase { get; set; }
    public TypingsPaths? TypingsPaths { get; set; }

    public new Settings Clone()
    {
        var clone = (Settings)this.MemberwiseClone();

        clone.ExtraProperties = [];
        foreach (var kvp in this.ExtraProperties)
        {
            clone.ExtraProperties.Add(kvp.Key, kvp.Value);
        }

        clone.AssemblyPaths = [.. this.AssemblyPaths];

        if (this.ClassNamespaceFilter != null)
        {
            clone.ClassNamespaceFilter = [.. this.ClassNamespaceFilter];
        }

        if (this.EnumNamespaceFilter != null)
        {
            clone.EnumNamespaceFilter = [.. this.EnumNamespaceFilter];
        }

        clone.TypingsPaths = this.TypingsPaths == null ? null : new TypingsPaths
        {
            Breeze = this.TypingsPaths.Breeze,
            Knockout = this.TypingsPaths.Knockout
        };
        return clone;
    }
}


using jasMIN.Net2TypeScript.Shared.SettingsModel;

namespace jasMIN.Net2TypeScript.Shared.DotNetModel;

abstract class DotNetModelBase
{
    private Settings? _settings;
    protected GlobalSettings _globalSettings;

    protected DotNetModelBase(GlobalSettings globalSettings)
    {
        this._globalSettings = globalSettings;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Blocker Code Smell", "S3060:\"is\" should not be used with \"this\"", Justification = "<Pending>")]
    protected virtual Settings Settings
    {
        get
        {
            if (this._settings == null)
            {
                if (this is NamespaceModel)
                {
                    this._settings = this._globalSettings.GetNamespaceSettings(this.FullName);
                }
                else if (this is ClassOrInterfaceModel)
                {
                    this._settings = this._globalSettings.GetClassSettings(this.FullName);
                }
                else if (this is PropertyModel propModel)
                {
                    this._settings = this._globalSettings.GetClassSettings(propModel.DeclaringType.FullName!);
                }
                else
                {
                    this._settings = this._globalSettings;
                }
            }
            return this._settings;
        }
    }

    protected string FullName { get; set; }

    protected string Indent(int count)
    {
        return string.Concat(Enumerable.Repeat(this.Settings.Indent, count));
    }

    public abstract StreamWriter WriteTs(StreamWriter sw, int indentCount);
}

using System.Text;

namespace jasMIN.Net2TypeScript.Model
{
    abstract class ClrModelBase
    {
        private Settings _settings = null;
        protected GlobalSettings _globalSettings;
        protected ClrModelBase(GlobalSettings globalSettings)
        {
            this._globalSettings = globalSettings;
        }
        protected Settings Settings
        {
            get
            {
                if (this._settings == null)
                {
                    if (this is NamespaceModel)
                    {
                        this._settings = this._globalSettings.GetNamespaceSettings(this.ClrFullName);
                    }
                    else if (this is ClassOrInterfaceModel)
                    {
                        this._settings = this._globalSettings.GetClassSettings(this.ClrFullName);
                    }
                    else if (this is PropertyModel)
                    {
                        var propModel = this as PropertyModel;
                        this._settings = this._globalSettings.GetClassSettings(propModel.OwnerType.FullName);
                    }
                    else
                    {
                        this._settings = this._globalSettings;
                    }
                }
                return this._settings;
            }
        }
        protected abstract string IndentationContext { get; }
        protected string ClrFullName { get; set; }
        public abstract void AppendTs(StringBuilder sb);
    }
}

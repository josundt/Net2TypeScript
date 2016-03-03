using System.Text;

namespace jasMIN.Net2TypeScript.Model
{
    abstract class ClrModelBase
    {
        protected ClrModelBase(Settings settings)
        {
            this.Settings = settings;
        }
        protected Settings Settings { get; set; }
        protected abstract string Indent { get; }
        protected string ClrTypeName { get; set; }
        public abstract void AppendTs(StringBuilder sb);
    }
}

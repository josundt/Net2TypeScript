namespace jasMIN.Net2TypeScript.Shared.TypeScriptModel;

public interface ITypeScriptType
{
    string TypeName { get; }
    bool IsNullable { get; }
    bool IsGeneric { get; }
    bool IsKnockoutObservable { get; }
    IList<ITypeScriptType> GenericTypeArguments { get; }
    string ToString();
}

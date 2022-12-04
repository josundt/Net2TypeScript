namespace jasMIN.Net2TypeScript.TypeScriptModel;

public interface ITypeScriptType
{
    string? TypeName { get; }
    bool IsNullable { get; }
    bool IsGeneric { get; }
    bool IsKnockoutObservable { get; }
    IList<ITypeScriptType> GenericTypeArguments { get; }
    string ToString();
}

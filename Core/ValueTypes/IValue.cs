public interface IValue
{
    string TypeName { get; }
    object Raw { get; }

    int AsInt(); 
    float AsFloat();
    string AsString();
    bool AsBool();

    string ToString();
}
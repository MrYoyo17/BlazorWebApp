namespace TestBlazor.Models.Symbols;

public class ValveProperties : SymbolProperties
{
    public bool IsOpen { get; set; }

    public ValveProperties(string name) : base(name)
    {
    }
}

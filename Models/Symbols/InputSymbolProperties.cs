namespace TestBlazor.Models.Symbols;

public class InputSymbolProperties : SymbolProperties
{
    public double Value { get; set; }
    public string Unit { get; set; } = string.Empty;

    public InputSymbolProperties(string name) : base(name)
    {
    }
}

namespace TestBlazor.Models.Symbols;

public class CompassProperties : SymbolProperties
{
    public int Heading { get; set; }

    public CompassProperties(string name) : base(name)
    {
    }
}

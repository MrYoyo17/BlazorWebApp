namespace TestBlazor.Models.Symbols;

public class AnalogDisplayProperties : SymbolProperties
{
    public double Value { get; set; }
    public double MinValue { get; set; } = 0;
    public double MaxValue { get; set; } = 100;
    public string Unit { get; set; } = "%";

    public AnalogDisplayProperties(string name) : base(name)
    {
    }
}

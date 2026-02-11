namespace TestBlazor.Models.Symbols;

public abstract class RotatingMachineProperties : SymbolProperties
{
    public double Speed { get; set; }
    public bool IsRunning { get; set; }

    protected RotatingMachineProperties(string name) : base(name)
    {
    }
}

using System;

namespace TestBlazor.Models.Symbols;

public abstract class SymbolProperties
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    
    // UI State
    public bool IsSelected { get; set; }
    public bool IsEnabled { get; set; } = true;
    
    // Simulation State
    public bool HasCommLoss { get; set; }
    public bool HasCrossFilter { get; set; }
    public int AlarmState { get; set; } // 0=None, 1=Warning, 2=Error
    public bool AlarmBlink { get; set; }
    
    // Layout
    public int Rotation { get; set; }
    public double? X { get; set; }
    public double? Y { get; set; }
    public bool MirrorX { get; set; }
    public bool MirrorY { get; set; }

    protected SymbolProperties(string name)
    {
        Name = name;
    }
}

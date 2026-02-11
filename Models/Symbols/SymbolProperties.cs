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
    public int AlarmState { get; set; } // 0=None, 1=Warning, 2=Error
    public bool AlarmBlink { get; set; }

    protected SymbolProperties(string name)
    {
        Name = name;
    }
}

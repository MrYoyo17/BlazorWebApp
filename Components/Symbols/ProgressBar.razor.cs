using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using TestBlazor.Models.Symbols;

namespace TestBlazor.Components.Symbols;

public partial class ProgressBar : ComponentBase
{
    [Parameter] public AnalogDisplayProperties? Symbol { get; set; }

    [Parameter] public double Value { get; set; } = 0; // 0 to 100
    [Parameter] public string Color { get; set; } = "#28a745"; // Green
    
    [Parameter] public bool HasCrossFilter { get; set; }
    
    [Parameter] public int Width { get; set; } = 50;
    [Parameter] public int Height { get; set; } = 150;
    [Parameter] public int Rotation { get; set; } = 0;
    [Parameter] public bool MirrorX { get; set; }
    [Parameter] public bool MirrorY { get; set; }

    [Parameter] public EventCallback<MouseEventArgs> OnClick { get; set; }
    [Parameter] public EventCallback OnSettingsClick { get; set; }
    [Parameter] public RenderFragment? OverlayContent { get; set; }

    private double CurrentValue => Symbol != null ? Symbol.Value : Value;
}

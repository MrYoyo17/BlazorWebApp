using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using TestBlazor.Models.Symbols;

namespace TestBlazor.Components.Symbols;

public partial class InputSymbol : ComponentBase
{
    [Parameter] public InputSymbolProperties? Symbol { get; set; }
    
    [Parameter] public double Value { get; set; }
    [Parameter] public string Label { get; set; } = "Value";
    [Parameter] public string Unit { get; set; } = "";
    [Parameter] public EventCallback<double> ValueChanged { get; set; }

    [Parameter] public bool HasCrossFilter { get; set; }
    
    [Parameter] public int Width { get; set; } = 100;
    [Parameter] public int Height { get; set; } = 80;
    [Parameter] public int Rotation { get; set; } = 0;
    [Parameter] public bool MirrorX { get; set; }
    [Parameter] public bool MirrorY { get; set; }

    [Parameter] public EventCallback<MouseEventArgs> OnClick { get; set; }
    [Parameter] public EventCallback OnSettingsClick { get; set; }
    [Parameter] public RenderFragment? OverlayContent { get; set; }

    private double CurrentValue => Symbol != null ? Symbol.Value : Value;
    private string CurrentUnit => Symbol != null ? Symbol.Unit : Unit;

    private async Task OnValueChanged(ChangeEventArgs e)
    {
        if (double.TryParse(e.Value?.ToString(), out double val))
        {
            if (Symbol != null) 
            {
                Symbol.Value = val;
            }
            // Always invoke ValueChanged for compatibility
            await ValueChanged.InvokeAsync(val);
        }
    }
}

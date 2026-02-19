using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using TestBlazor.Models.Symbols;

namespace TestBlazor.Components.Symbols;

public partial class Moteur : ComponentBase
{
    [Parameter] public RotatingMachineProperties? Symbol { get; set; }

    [Parameter] public bool HasCrossFilter { get; set; }
    
    [Parameter] public int Size { get; set; } = 100;
    [Parameter] public int? Width { get; set; }
    [Parameter] public int? Height { get; set; }
    [Parameter] public int Rotation { get; set; } = 0;
    [Parameter] public bool MirrorX { get; set; }
    [Parameter] public bool MirrorY { get; set; }

    [Parameter] public EventCallback<MouseEventArgs> OnClick { get; set; }
    [Parameter] public EventCallback OnSettingsClick { get; set; }
    [Parameter] public RenderFragment? OverlayContent { get; set; }
}

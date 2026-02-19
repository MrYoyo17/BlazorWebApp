using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using TestBlazor.Models.Symbols;

namespace TestBlazor.Components.Symbols;

public partial class SymbolWrapper : ComponentBase
{
    [Parameter] public SymbolProperties? Symbol { get; set; }

    // Paramètres d'état visuel du symbole (SUPPRIMÉS - Utiliser Symbol)
    // [Parameter] public bool IsSelected { get; set; }
    // [Parameter] public bool IsEnabled { get; set; } = true;
    // [Parameter] public bool HasCommLoss { get; set; }
    // ...
    // NOTE: HasCrossFilter is NOT in SymbolProperties yet, so keeping it or removing if user wants ALL common params removed.
    // User request: "supprime les paramètres des composants qui sont déjà présent dans les entités"
    // HasCrossFilter is NOT in entity. So keep it.
    [Parameter] public bool HasCrossFilter { get; set; }
    
    // Computed Properties
    private bool EffectiveIsSelected => Symbol?.IsSelected ?? false;
    private bool EffectiveIsEnabled => Symbol?.IsEnabled ?? true; // Default to true if no symbol
    private bool EffectiveHasCommLoss => Symbol?.HasCommLoss ?? false;
    private bool EffectiveHasCrossFilter => Symbol?.HasCrossFilter ?? HasCrossFilter;
    private int EffectiveAlarmState => Symbol?.AlarmState ?? 0;
    private bool EffectiveAlarmBlink => Symbol?.AlarmBlink ?? false;
    
    // Paramètres de dimensionnement et rotation
    [Parameter] public int? Size { get; set; } // Gardé pour rétro-compatibilité
    [Parameter] public int? Width { get; set; }
    [Parameter] public int? Height { get; set; }
    [Parameter] public int Rotation { get; set; } = 0;
    
    // Positionnement
    [Parameter] public double? X { get; set; }
    [Parameter] public double? Y { get; set; }
    
    // Paramètres de symétrie
    [Parameter] public bool MirrorX { get; set; }
    [Parameter] public bool MirrorY { get; set; }

    // RenderFragments
    [Parameter] public RenderFragment? OverlayContent { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }
    
    // EventCallbacks
    [Parameter] public EventCallback<MouseEventArgs> OnClick { get; set; }
    [Parameter] public EventCallback OnSettingsClick { get; set; }

    private int ActualWidth => Width ?? Size ?? 100;
    private int ActualHeight => Height ?? Size ?? 100;
    
    private double? EffectiveX => Symbol?.X ?? X;
    private double? EffectiveY => Symbol?.Y ?? Y;
    private int EffectiveRotation => Symbol?.Rotation ?? Rotation;
    
    private bool EffectiveMirrorX => Symbol?.MirrorX ?? MirrorX;
    private bool EffectiveMirrorY => Symbol?.MirrorY ?? MirrorY;

    // Style Racine : Dimensions fixes et Position Absolue (si X/Y définis ou via Symbol)
    private string GetRootStyle()
    {
        var style = $"width: {ActualWidth}px; height: {ActualHeight}px;";
        
        // On force la position absolue UNIQUEMENT si X ou Y sont définis
        if (EffectiveX.HasValue || EffectiveY.HasValue)
        {
            style += $" position: absolute; left: {EffectiveX ?? 0}px; top: {EffectiveY ?? 0}px;";
        }
        else
        {
             // Position relative par défaut pour le flux standard
             style += " position: relative;";
        }
        
        return style;
    }

    // Style Rotation : sur le conteneur interne
    private string GetRotationStyle()
    {
        var transform = "";
        if (EffectiveRotation != 0)
        {
            transform += $"rotate({EffectiveRotation}deg) ";
        }
        
        if (EffectiveMirrorX)
        {
            transform += "scaleX(-1) ";
        }
        
        if (EffectiveMirrorY)
        {
            transform += "scaleY(-1) ";
        }
        
        if (!string.IsNullOrEmpty(transform))
        {
            return $"transform: {transform}; transform-origin: center;";
        }
        
        return "";
    }

    // Style Miroir : sur le contenu
    private string GetMirrorStyle()
    {
        var transform = "";
        
        if (MirrorX) transform += " scaleX(-1)";
        if (MirrorY) transform += " scaleY(-1)";
        
        return $"width: 100%; height: 100%;{(string.IsNullOrEmpty(transform) ? "" : $" transform:{transform}; transform-origin: center;")}";
    }
}

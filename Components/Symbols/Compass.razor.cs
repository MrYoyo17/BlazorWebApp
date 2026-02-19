using Microsoft.AspNetCore.Components;
using TestBlazor.Models.Symbols;
using TestBlazor.Services;

namespace TestBlazor.Components.Symbols;

public partial class Compass : ComponentBase, IDisposable
{
    [Inject]
    public required CompassService CompassService { get; set; }

    [Parameter] public CompassProperties? Symbol { get; set; }
    [Parameter] public int? Heading { get; set; }

    private int CurrentHeading => Symbol?.Heading ?? Heading ?? CompassService.Heading;

    protected override void OnInitialized()
    {
        // S'abonne à l'événement de changement du service.
        // Cela permet au composant de réagir aux mises à jour globales si le paramètre n'est pas fourni.
        CompassService.OnChange += OnCompassChange;
    }

    public void Dispose()
    {
        CompassService.OnChange -= OnCompassChange;
    }

    private async void OnCompassChange()
    {
        // Si le Heading est fourni via paramètre, on ne force pas le refresh ici car c'est le parent qui gère
        if (Heading.HasValue) return;

        await InvokeAsync(StateHasChanged);
    }
}

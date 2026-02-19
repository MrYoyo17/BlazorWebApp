using Microsoft.AspNetCore.Components;
using TestBlazor.Components.Symbols;
using TestBlazor.Models;
using TestBlazor.Models.Symbols;
using TestBlazor.Services;

namespace TestBlazor.Components.Pages;

public partial class Simulation : ComponentBase, IDisposable
{
    [Inject]
    public required SimulationService SimulationService { get; set; }

    [Inject]
    public required WcfService WcfService { get; set; }
    
    [Inject]
    public required IUdpSenderService UdpSenderService { get; set; }

    private FanProperties? MotorFan;
    private ValveProperties? InletValve;
    private AnalogDisplayProperties? TankLevel;
    private CompassProperties? SimCompass;
    private PumpProperties? FeedPump;

    protected override void OnInitialized()
    {
        // Récupération des symboles par nom ou type
        MotorFan = SimulationService.Symbols.OfType<FanProperties>().FirstOrDefault(f => f.Name == "Main Fan");
        InletValve = SimulationService.Symbols.OfType<ValveProperties>().FirstOrDefault(v => v.Name == "Inlet Valve");
        TankLevel = SimulationService.Symbols.OfType<AnalogDisplayProperties>().FirstOrDefault(t => t.Name == "Tank Level");
        SimCompass = SimulationService.Symbols.OfType<CompassProperties>().FirstOrDefault(c => c.Name == "Navigation");
        FeedPump = SimulationService.Symbols.OfType<PumpProperties>().FirstOrDefault(p => p.Name == "Feed Pump");

        SimulationService.OnChange += OnSimulationChange;
    }

    public void Dispose()
    {
        SimulationService.OnChange -= OnSimulationChange;
    }

    private async void OnSimulationChange()
    {
        // Re-render
        await InvokeAsync(StateHasChanged);
    }

    private string GetColor(double value)
    {
        if (value > 90) return "#dc3545"; // Rouge
        if (value > 75) return "#ffc107"; // Jaune
        return "#28a745"; // Vert
    }

    // WCF Test
    private string WcfResult = "-";
    private string WcfError = "";

    private async Task TestWcf()
    {
        WcfResult = "Appel en cours...";
        WcfError = "";
        try
        {
            var res = await WcfService.AddAsync(10, 20);
            WcfResult = res.ToString();
        }
        catch (Exception ex)
        {
            WcfError = $"Erreur: {ex.Message} (Normal si pas de serveur)";
            WcfResult = "Erreur";
        }
    }

    // UDP Sender
    private string UdpIp = "239.0.0.1";
    private int UdpPort = 11000;
    private double UdpPump1 = 50;
    private double UdpPump2 = 50;
    private string UdpStatus = "";

    private async Task SendUdpPacket()
    {
        UdpStatus = "Envoi...";
        try
        {
            var data = new TestBlazor.Models.UdpPacketData
            {
                PacketId = new Random().Next(1000),
                Pump1Value = UdpPump1,
                Pump2Value = UdpPump2,
                AlarmState = 0,
                Input1Value = 0
            };
            
            await UdpSenderService.SendPacketAsync(UdpIp, UdpPort, data);
            UdpStatus = $"Paquet envoyé à {UdpIp}:{UdpPort}";
        }
        catch (Exception ex)
        {
             UdpStatus = $"Erreur: {ex.Message}";
        }
    }
}

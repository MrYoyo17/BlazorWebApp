using Microsoft.AspNetCore.Components;
using TestBlazor.Components.Symbols;
using TestBlazor.Components.Symbols.Generics;
using TestBlazor.Models;
using TestBlazor.Models.Symbols;
using TestBlazor.Services;

namespace TestBlazor.Components.Pages;

public partial class Supervision : ComponentBase, IDisposable
{
    [Inject]
    public required IUdpListenerService UdpService { get; set; }

    // Entities for the demo page
    private PumpProperties Pump1 { get; set; } = new("Pump 1");
    private PumpProperties Pump2 { get; set; } = new("Pump 2") { MirrorX = true };
    private ValveProperties Valve1 { get; set; } = new("Valve 1");
    private ValveProperties Valve2 { get; set; } = new("Valve 2");
    private FanProperties Fan1 { get; set; } = new("Fan 1");
    private FanProperties Fan2 { get; set; } = new("Fan 2");
    private RotatingMachineProperties Motor1 { get; set; } = new FanProperties("Motor 1"); 
    private RotatingMachineProperties Motor2 { get; set; } = new FanProperties("Motor 2");

    private CompassProperties Compass1 { get; set; } = new("Compass 1");
    
    private InputSymbolProperties Input1 { get; set; } = new("Input 1") { Value = 50, Unit = "%" };
    private AnalogDisplayProperties Progress1 { get; set; } = new("Progress 1") { Value = 50, Unit = "%" };

    private SymbolProperties? SelectedSymbol { get; set; }

    private bool IsModalVisible { get; set; }
    private string? TargetSettingsId { get; set; } 

    protected override void OnInitialized()
    {
        UdpService.OnDataReceived += HandleUdpData;
        UdpService.StartListening(11000, "239.0.0.1"); // Listen on port 11000 and join multicast group 239.0.0.1
        StartWatchdog();
    }

    private void HandleUdpData(UdpPacketData data)
    {
        _lastPacketTime = DateTime.Now;
        
        // If we were in Comm Loss, clear it immediately
        if (_isCommLoss)
        {
            _isCommLoss = false;
            UpdateCommLossState(false);
            // StateHasChanged will be called below
        }

        // Map UDP data to Symbol Properties
        Pump1.IsEnabled = data.Pump1Value > 0;
        Input1.Value = data.Input1Value;
        
        // Optional: Update alarm based on data
        if (data.AlarmState > 0)
        {
             Pump1.AlarmState = data.AlarmState;
        }

        InvokeAsync(StateHasChanged);
    }
    
    public void Dispose()
    {
        _watchdogTimer?.Dispose();
        
        if (UdpService != null)
        {
            UdpService.OnDataReceived -= HandleUdpData;
        }
    }

    private void Select(SymbolProperties symbol)
    {
        SelectedSymbol = symbol;
        SelectedSymbol.IsSelected = true;
        ClearSelection();
        symbol.IsSelected = true;
    }
    
    // Watchdog Logic
    private System.Threading.Timer? _watchdogTimer;
    private DateTime _lastPacketTime = DateTime.Now; 
    private bool _isCommLoss = false;
    
    private void StartWatchdog()
    {
        // Check every 1 second
        _watchdogTimer = new System.Threading.Timer(WatchdogCallback, null, 1000, 1000);
    }
    
    private void WatchdogCallback(object? state)
    {
        var timeSinceLastPacket = DateTime.Now - _lastPacketTime;
        bool shouldBeCommLoss = timeSinceLastPacket.TotalSeconds > 3;
        
        if (_isCommLoss != shouldBeCommLoss)
        {
            _isCommLoss = shouldBeCommLoss;
            UpdateCommLossState(_isCommLoss);
            InvokeAsync(StateHasChanged);
        }
    }
    
    private void UpdateCommLossState(bool isLoss)
    {
        // Update all symbols
        var symbols = new SymbolProperties[] 
        { 
            Pump1, Pump2, Valve1, Valve2, Fan1, Fan2, Motor1, Motor2, 
            Input1, Progress1, Compass1 
        };
        
        foreach(var s in symbols)
        {
            s.HasCommLoss = isLoss;
        }
    }

    private void ClearSelection()
    {
        Pump1.IsSelected = false; Pump2.IsSelected = false;
        Valve1.IsSelected = false; Valve2.IsSelected = false;
        Fan1.IsSelected = false; Fan2.IsSelected = false;
        Motor1.IsSelected = false; Motor2.IsSelected = false;
        Input1.IsSelected = false; Progress1.IsSelected = false;
    }

    // Control Panel Actions
    private void SetAlarmState(int state)
    {
        if (SelectedSymbol != null) SelectedSymbol.AlarmState = state;
    }

    private int GetAlarmState() => SelectedSymbol?.AlarmState ?? 0;

    private void SetAlarmBlink(bool blink)
    {
        if (SelectedSymbol != null) SelectedSymbol.AlarmBlink = blink;
    }

    private bool GetAlarmBlink() => SelectedSymbol?.AlarmBlink ?? false;

    private void SetRotation(int rotation)
    {
        // Placeholder for future implementation
    }
    
    private void ToggleOverlay() { }
    private void ToggleEnable() { if (SelectedSymbol != null) SelectedSymbol.IsEnabled = !SelectedSymbol.IsEnabled; }
    private void ToggleCommLoss() { if (SelectedSymbol != null) SelectedSymbol.HasCommLoss = !SelectedSymbol.HasCommLoss; }
    private void ToggleCrossFilter() 
    { 
        // Placeholder
    }

    // Settings
    private void OpenSettings(SymbolProperties symbol)
    {
        TargetSettingsId = symbol.Name;
        IsModalVisible = true;
    }

    private void OnConfirmSettings() { IsModalVisible = false; }
    private void OnCancelSettings() { IsModalVisible = false; }
}

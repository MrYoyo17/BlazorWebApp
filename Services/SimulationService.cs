using System;
using System.Timers;
using TestBlazor.Models.Symbols;
using System.Collections.Generic;

namespace TestBlazor.Services;

public class SimulationService : IDisposable
{
    private readonly System.Timers.Timer _timer;
    private readonly Random _random = new();

    // Collection of all symbols
    public List<SymbolProperties> Symbols { get; private set; } = new();

    // Événement pour notifier les composants
    public event Action? OnChange;

    public SimulationService()
    {
        InitializeSymbols();

        // Initialisation du timer (toutes les 100ms pour plus de fluidité)
        _timer = new System.Timers.Timer(100);
        _timer.Elapsed += OnTimerElapsed;
        _timer.AutoReset = true;
        _timer.Enabled = true;
    }

    private void InitializeSymbols()
    {
        // Fan
        var fan = new FanProperties("Main Fan")
        {
            Speed = 0,
            IsRunning = false
        };
        Symbols.Add(fan);

        // Pump
        var pump = new PumpProperties("Feed Pump")
        {
            Speed = 0,
            IsRunning = false
        };
        Symbols.Add(pump);

        // Valve
        var valve = new ValveProperties("Inlet Valve")
        {
            IsOpen = false
        };
        Symbols.Add(valve);

        // Compass
        var compass = new CompassProperties("Navigation")
        {
            Heading = 0
        };
        Symbols.Add(compass);

        // Progress Bar (Tank Level)
        var tankLevel = new AnalogDisplayProperties("Tank Level")
        {
            Value = 50,
            MinValue = 0,
            MaxValue = 100,
            Unit = "%"
        };
        Symbols.Add(tankLevel);
    }

    private void OnTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        foreach (var symbol in Symbols)
        {
            // Simulate random changes based on symbol type
            switch (symbol)
            {
                case FanProperties fan:
                    if (fan.IsRunning)
                        fan.Speed = Math.Clamp(fan.Speed + _random.Next(-2, 3), 0, 100);
                    else
                        fan.Speed = Math.Max(0, fan.Speed - 5);
                    
                    // Simple logic to toggle running state randomly
                    if (_random.Next(0, 100) == 0) fan.IsRunning = !fan.IsRunning;
                    break;

                case PumpProperties pump:
                    if (pump.IsRunning)
                        pump.Speed = Math.Clamp(pump.Speed + _random.Next(-1, 2), 0, 100);
                    else
                        pump.Speed = Math.Max(0, pump.Speed - 5);
                        
                    if (_random.Next(0, 150) == 0) pump.IsRunning = !pump.IsRunning;
                    break;

                case CompassProperties compass:
                    compass.Heading = (compass.Heading + 1) % 360;
                    break;

                case ValveProperties valve:
                    if (_random.Next(0, 200) == 0) valve.IsOpen = !valve.IsOpen;
                    break;
                    
                case AnalogDisplayProperties analog:
                    analog.Value = Math.Clamp(analog.Value + _random.Next(-1, 2), analog.MinValue, analog.MaxValue);
                    
                    // Alarm logic
                    if (analog.Value > 90) analog.AlarmState = 2;
                    else if (analog.Value > 75) analog.AlarmState = 1;
                    else analog.AlarmState = 0;
                    break;
            }
        }

        // Notification des abonnés
        OnChange?.Invoke();
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}

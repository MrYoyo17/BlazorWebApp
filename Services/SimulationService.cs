using System;
using System.Timers;

namespace TestBlazor.Services;

public class SimulationService : IDisposable
{
    private readonly System.Timers.Timer _timer;
    private readonly Random _random = new();

    // Propriétés simulées
    public double MotorSpeed { get; private set; }
    public double TankLevel { get; private set; }
    public bool IsValveOpen { get; private set; }
    public int AlarmState { get; private set; } // 0=None, 1=Warning, 2=Error
    
    // Événement pour notifier les composants
    public event Action? OnChange;

    public SimulationService()
    {
        // Initialisation du timer (toutes les 100ms pour plus de fluidité)
        _timer = new System.Timers.Timer(100);
        _timer.Elapsed += OnTimerElapsed;
        _timer.AutoReset = true;
        _timer.Enabled = true;

        // Valeurs initiales
        MotorSpeed = 0;
        TankLevel = 50;
        IsValveOpen = false;
        AlarmState = 0;
    }

    private void OnTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        // Simulation de variations aléatoires (petits incréments pour fluidité)
        MotorSpeed = Math.Clamp(MotorSpeed + _random.Next(-2, 3), 0, 100);
        TankLevel = Math.Clamp(TankLevel + _random.Next(-1, 2), 0, 100);
        
        // Changement d'état aléatoire (1 chance sur 50)
        if (_random.Next(0, 50) == 0)
        {
            IsValveOpen = !IsValveOpen;
        }

        // Gestion d'alarmes simpliste
        if (MotorSpeed > 90) AlarmState = 2; // Error
        else if (MotorSpeed > 75) AlarmState = 1; // Warning
        else AlarmState = 0;

        // Notification des abonnés
        OnChange?.Invoke();
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}

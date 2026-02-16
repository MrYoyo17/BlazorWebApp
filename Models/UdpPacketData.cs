using System.Runtime.InteropServices;

namespace TestBlazor.Models;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct UdpPacketData
{
    // Placeholder fields. The user will provide the actual structure later.
    // For now, we can add some fields that might be useful for testing or basic data.
    
    // Example: A simple header or identifier
    public int PacketId; 
    
    // Example: Some data related to the symbols
    public double Pump1Value;
    public double Pump2Value;
    public int AlarmState;
    public double Input1Value;
    
    // Additional bytes to match a potential larger structure size?
    // For now, keep it simple.
}

using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using TestBlazor.Models;

namespace TestBlazor.Services;

/// <summary>
/// Service responsible for sending UDP packets, used mainly for verification and simulation.
/// </summary>
public class UdpSenderService : IUdpSenderService
{
    /// <summary>
    /// Sends a <see cref="UdpPacketData"/> struct as a valid UDP packet to the specified IP and Port.
    /// Handles finding the IP (supports Multicast or Unicast).
    /// </summary>
    /// <param name="ipAddress">Target IP Address (can be a multicast group).</param>
    /// <param name="port">Target Port.</param>
    /// <param name="data">The data struct to send.</param>
    public async Task SendPacketAsync(string ipAddress, int port, UdpPacketData data)
    {
        using var client = new UdpClient();
        try
        {
            // Convert struct to raw bytes
            var bytes = Serialize(data);
            IPEndPoint endPoint;
            
            if (IPAddress.TryParse(ipAddress, out var address))
            {
                 endPoint = new IPEndPoint(address, port);
            }
            else
            {
                // Fallback to host resolution if not a valid IP string
                var addresses = await Dns.GetHostAddressesAsync(ipAddress);
                if (addresses.Length == 0)
                {
                    throw new ArgumentException($"Could not resolve IP address: {ipAddress}");
                }
                 endPoint = new IPEndPoint(addresses[0], port);
            }

            // Send payload
            await client.SendAsync(bytes, bytes.Length, endPoint);
            Console.WriteLine($"[UDP Sender] Sent {bytes.Length} bytes to {endPoint}");
        }
        catch (Exception ex)
        {
             Console.WriteLine($"[UDP Sender] Error sending packet: {ex.Message}");
             throw;
        }
    }

    /// <summary>
    /// Marshals a struct into a byte array.
    /// </summary>
    /// <typeparam name="T">Struct type.</typeparam>
    /// <param name="data">Struct instance.</param>
    /// <returns>Byte array matching the memory layout of the struct.</returns>
    private byte[] Serialize<T>(T data) where T : struct
    {
        int size = Marshal.SizeOf(data);
        byte[] arr = new byte[size];
        
        // Allocate unmanaged memory block
        IntPtr ptr = Marshal.AllocHGlobal(size);

        try
        {
            // Copy struct to unmanaged memory
            Marshal.StructureToPtr(data, ptr, true);
            // Copy from unmanaged memory to byte array
            Marshal.Copy(ptr, arr, 0, size);
        }
        finally
        {
            // Always free unmanaged memory
            Marshal.FreeHGlobal(ptr);
        }
        return arr;
    }
}

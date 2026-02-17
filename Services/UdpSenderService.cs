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
        // Enforce IPv4 to avoid issues with IPv6 defaults on some systems when using older multicast groups
        using var client = new UdpClient(AddressFamily.InterNetwork);
        
        // Ensure TTL is sufficient for local network (default is 1, which is usually fine for local segment)
        client.Ttl = 1;

        // Serialize data before try block so it's available in catch block
        var bytes = Serialize(data);

        try
        {
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
                // Prefer IPv4 if available
                var ipv4Address = addresses.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork) ?? addresses[0];
                endPoint = new IPEndPoint(ipv4Address, port);
            }

            // Send payload
            await client.SendAsync(bytes, bytes.Length, endPoint);
            Console.WriteLine($"[UDP Sender] Sent {bytes.Length} bytes to {endPoint}");
        }
        catch (SocketException ex) when (ex.SocketErrorCode == SocketError.HostUnreachable || ex.SocketErrorCode == SocketError.NetworkUnreachable)
        {
             // Specific handling for "No route to host" (HostUnreachable)
             Console.WriteLine($"[UDP Sender] Network Unreachable: {ex.Message}. Attempting fallback bind.");
             
             // Retry with explicit bind to LocalHost if the previous attempt failed due to routing 
             // (Common on macOS for Multicast without explicit route)
             try 
             {
                 using var fallbackClient = new UdpClient(new IPEndPoint(IPAddress.Loopback, 0));
                 if (IPAddress.TryParse(ipAddress, out var address))
                 {
                    var endPoint = new IPEndPoint(address, port);
                    await fallbackClient.SendAsync(bytes, bytes.Length, endPoint);
                    Console.WriteLine($"[UDP Sender] Fallback sent {bytes.Length} bytes to {endPoint} via Loopback");
                    return;
                 }
             }
             catch
             {
                 // invalid, throw original
             }
             throw;
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

using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using TestBlazor.Models;

namespace TestBlazor.Services;

public interface IUdpListenerService
{
    event Action<UdpPacketData> OnDataReceived;
    void StartListening(int port, string? multicastGroup = null);
    void StopListening();
}

/// <summary>
/// Service responsible for listening to UDP traffic, including Multicast groups.
/// It deserializes received binary data into the <see cref="UdpPacketData"/> structure.
/// </summary>
public class UdpListenerService : IUdpListenerService, IDisposable
{
    private UdpClient? _udpClient;
    private CancellationTokenSource? _cts;
    private Task? _listeningTask;

    /// <summary>
    /// Event triggered when a valid UDP packet is received and deserialized.
    /// </summary>
    public event Action<UdpPacketData>? OnDataReceived;

    /// <summary>
    /// Starts listening on the specified port.
    /// Optionally joins a multicast group if an address is provided.
    /// </summary>
    /// <param name="port">The local port to listen on.</param>
    /// <param name="multicastGroup">Optional multicast IP address to join (e.g., "239.0.0.1").</param>
    public void StartListening(int port, string? multicastGroup = null)
    {
        if (_udpClient != null)
        {
            // Already listening or not properly stopped
            return;
        }

        try 
        {
            _udpClient = new UdpClient();
            
            // Allow multiple applications to bind to the same port (essential for multicast testing on same machine)
            _udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            
            // Bind to all available network interfaces on the specified port
            _udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, port));

            if (!string.IsNullOrEmpty(multicastGroup))
            {
                if (IPAddress.TryParse(multicastGroup, out IPAddress? mcastAddress))
                {
                    // Join the multicast group to receive packets sent to this address
                    _udpClient.JoinMulticastGroup(mcastAddress);
                    Console.WriteLine($"Joined multicast group {multicastGroup}");
                }
                else
                {
                     Console.WriteLine($"Invalid multicast group address: {multicastGroup}");
                }
            }

            _cts = new CancellationTokenSource();
            _listeningTask = Task.Run(() => ListenLoop(_cts.Token), _cts.Token);
            Console.WriteLine($"UDP Listener started on port {port}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to start UDP listener: {ex.Message}");
        }
    }

    /// <summary>
    /// Stops the background listener and releases resources.
    /// </summary>
    public void StopListening()
    {
        _cts?.Cancel();
        _udpClient?.Close();
        _udpClient = null;
    }

    /// <summary>
    /// Background loop that continuously receives UDP packets until cancellation.
    /// </summary>
    private async Task ListenLoop(CancellationToken token)
    {
        while (!token.IsCancellationRequested && _udpClient != null)
        {
            try
            {
                // Verify if data is available for reading to avoid blocking indefinitely if possible, 
                // but ReceiveAsync handles cancellation via the client closure usually.
                var result = await _udpClient.ReceiveAsync(token);
                var data = result.Buffer;
                
                // Log the received data for debugging
                Console.WriteLine($"[UDP] Received packet of size: {data.Length} bytes from {result.RemoteEndPoint}");
               
                // Check if the received data size matches our expected structure size
                if (data.Length >= Marshal.SizeOf<UdpPacketData>())
                {
                    // Convert raw bytes to struct
                    var packet = Deserialize<UdpPacketData>(data);
                    
                    // Notify subscribers (UI, other services)
                    OnDataReceived?.Invoke(packet);
                }
                else 
                {
                     // Only log warning if strictly enforcing size, otherwise could verify header etc.
                     Console.WriteLine($"[UDP] Packet too small or invalid format.");
                }
            }
            catch (OperationCanceledException)
            {
                break; // Normal shutdown
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UDP Receive Error: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Marshals a byte array into a struct.
    /// </summary>
    /// <typeparam name="T">The struct type to deserialize into.</typeparam>
    /// <param name="data">Raw byte data.</param>
    /// <returns>The populated struct.</returns>
    private T Deserialize<T>(byte[] data) where T : struct
    {
        // GCHandle pins the byte array in memory so the garbage collector doesn't move it
        GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
        try
        {
            // Marshal data from the memory pointer to the structure
            return Marshal.PtrToStructure<T>(handle.AddrOfPinnedObject());
        }
        finally
        {
            handle.Free(); // Always free the handle
        }
    }

    public void Dispose()
    {
        StopListening();
        _cts?.Dispose();
    }
}

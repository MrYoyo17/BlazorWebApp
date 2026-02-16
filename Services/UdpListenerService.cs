using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using TestBlazor.Models;

namespace TestBlazor.Services;

public interface IUdpListenerService
{
    event Action<UdpPacketData> OnDataReceived;
    void StartListening(int port);
    void StopListening();
}

public class UdpListenerService : IUdpListenerService, IDisposable
{
    private UdpClient? _udpClient;
    private CancellationTokenSource? _cts;
    private Task? _listeningTask;

    public event Action<UdpPacketData>? OnDataReceived;

    public void StartListening(int port)
    {
        if (_udpClient != null)
        {
            // Already listening or not properly stopped
            return;
        }

        try 
        {
            _udpClient = new UdpClient(port);
            _cts = new CancellationTokenSource();
            _listeningTask = Task.Run(() => ListenLoop(_cts.Token), _cts.Token);
            Console.WriteLine($"UDP Listener started on port {port}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to start UDP listener: {ex.Message}");
        }
    }

    public void StopListening()
    {
        _cts?.Cancel();
        _udpClient?.Close();
        _udpClient = null;
    }

    private async Task ListenLoop(CancellationToken token)
    {
        while (!token.IsCancellationRequested && _udpClient != null)
        {
            try
            {
                var result = await _udpClient.ReceiveAsync(token);
                var data = result.Buffer;
                
                // Log the received data
                Console.WriteLine($"[UDP] Received packet of size: {data.Length} bytes from {result.RemoteEndPoint}");
                Console.WriteLine($"[UDP] Data (Hex): {BitConverter.ToString(data)}");

                if (data.Length >= Marshal.SizeOf<UdpPacketData>())
                {
                    var packet = Deserialize<UdpPacketData>(data);
                    OnDataReceived?.Invoke(packet);
                }
                else 
                {
                     // Handle packet size mismatch or partial data if necessary
                     // For now, ignore invalid size
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UDP Receive Error: {ex.Message}");
            }
        }
    }

    private T Deserialize<T>(byte[] data) where T : struct
    {
        GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
        try
        {
            return Marshal.PtrToStructure<T>(handle.AddrOfPinnedObject());
        }
        finally
        {
            handle.Free();
        }
    }

    public void Dispose()
    {
        StopListening();
        _cts?.Dispose();
    }
}

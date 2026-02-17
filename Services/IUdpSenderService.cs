using TestBlazor.Models;

namespace TestBlazor.Services;

public interface IUdpSenderService
{
    Task SendPacketAsync(string ipAddress, int port, UdpPacketData data);
}

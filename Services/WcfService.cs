using System.ServiceModel;
using TestBlazor.Services.WcfContext;

namespace TestBlazor.Services;

public class WcfService
{
    // Example endpoint URL - in a real app this would come from configuration
    private const string ServiceUrl = "http://localhost:8000/CalculatorService";

    public async Task<double> AddAsync(double n1, double n2)
    {
        return await ExecuteAsync(client => client.Add(n1, n2));
    }

    private async Task<T> ExecuteAsync<T>(Func<ICalculatorService, T> action)
    {
        var binding = new BasicHttpBinding();
        var endpoint = new EndpointAddress(ServiceUrl);
        
        using var channelFactory = new ChannelFactory<ICalculatorService>(binding, endpoint);
        ICalculatorService client = null;

        try
        {
            client = channelFactory.CreateChannel();
            // WCF calls in .NET Core are often synchronous on the interface, but we wrap them in Task.Run if needed
            // or better, generate async interface. For simplicity here, we assume sync interface but wrap in Task.
            return await Task.Run(() => action(client));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"WCF Error: {ex.Message}");
            throw;
        }
        finally
        {
            if (client is ICommunicationObject communicationObject)
            {
                if (communicationObject.State == CommunicationState.Faulted)
                {
                    communicationObject.Abort();
                }
                else
                {
                    try
                    {
                        communicationObject.Close();
                    }
                    catch
                    {
                        communicationObject.Abort();
                    }
                }
            }
        }
    }
}

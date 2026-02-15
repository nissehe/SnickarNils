using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;

namespace Api;

public class Program
{
    public static void Main(string[] args)
    {
        var host = new HostBuilder()
            .ConfigureFunctionsWorkerDefaults()
            .ConfigureServices(services =>
            {
                services.AddSingleton<ImageCollectionService>();

                string blobStorageConnectionString = Debugger.IsAttached
                    ? Environment.GetEnvironmentVariable("AzureWebJobsStorage")
                    : Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING");

                services.AddAzureClients(clientBuilder =>
                {
                    // Register clients for each service
                    clientBuilder.AddBlobServiceClient(blobStorageConnectionString);
                });
            })
            .ConfigureLogging(logging =>
            {
                // Customize logging here
            })
            .Build();

        host.Run();
    }
}

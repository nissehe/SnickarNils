using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

[assembly: FunctionsStartup(typeof(Api.Startup))]

namespace Api;

public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        builder.Services.AddSingleton<IProductData, ProductData>();
        builder.Services.AddSingleton<ImageCollectionService>();

        string blobStorageConnectionString = Debugger.IsAttached
                ? Environment.GetEnvironmentVariable("AzureWebJobsStorage")
                : Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING");

        builder.Services.AddAzureClients(clientBuilder =>
        {
            // Register clients for each service
            clientBuilder.AddBlobServiceClient(blobStorageConnectionString);
        });
    }
}

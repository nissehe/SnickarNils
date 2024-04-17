using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Api;

public class ProductsGet
{
    private readonly IProductData productData;
    private readonly BlobServiceClient blobServiceClient;

    public ProductsGet(IProductData productData, BlobServiceClient blobServiceClient)
    {
        this.productData = productData;
        this.blobServiceClient = blobServiceClient;
    }

    [FunctionName("ProductsGet")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "products")] HttpRequest req)
    {
        var containers = blobServiceClient.GetBlobContainersAsync();

        await foreach (Page<BlobContainerItem> page in containers.AsPages())
        {
            foreach (BlobContainerItem container in page.Values)
            {
                try
                {
                    var containerClient = blobServiceClient.GetBlobContainerClient(container.Name);

                    var coverImage = containerClient.GetBlobs()
                        .Where(b => b.Name.StartsWith("cover.", StringComparison.OrdinalIgnoreCase))
                        .FirstOrDefault();

                    var descriptionBlob = containerClient.GetBlobs()
                        .Where(b => b.Name.StartsWith("description.txt", StringComparison.OrdinalIgnoreCase))
                        .FirstOrDefault();

                    var descriptionBlobClient = containerClient.GetBlobClient(descriptionBlob.Name);
                    BlobDownloadResult downloadResult = await descriptionBlobClient.DownloadContentAsync();
                    string description = downloadResult.Content.ToString();

                    var imageSet = new ImageCollectionSummary()
                    {
                        ContainerName = container.Name,
                        CoverImageUri = new Uri(containerClient.Uri, coverImage.Name).ToString(),
                        Description = description
                    };
                }
                catch (Exception)
                {
                }
            }
        }

        var products = await productData.GetProducts();
        return new OkObjectResult(products);
    }
}
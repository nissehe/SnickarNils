using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;
using Azure;
using Data;
using System.Linq;
using System.Collections.Generic;

namespace Api
{
    public class ImageCollectionSummaryGet
    {
        private readonly BlobServiceClient blobServiceClient;

        public ImageCollectionSummaryGet(BlobServiceClient blobServiceClient)
        {
            this.blobServiceClient = blobServiceClient;
        }

        [FunctionName("ImageSetSummaryGet")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            var imageSetSummaries = new List<ImageColectionSummary>();

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

                        var imageSet = new ImageColectionSummary()
                        {
                            ContainerName = container.Name,
                            CoverImageUri = new Uri(containerClient.Uri, coverImage.Name).ToString(),
                            Description = description
                        };

                        imageSetSummaries.Add(imageSet);
                    }
                    catch (Exception ex)
                    {
                        log.LogError($"Failed to create ImageSetSummary for {container.Name} - {ex.Message}");
                    }
                }
            }

            return new OkObjectResult(imageSetSummaries);
        }
    }
}

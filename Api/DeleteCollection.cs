using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging;

namespace Api;

internal class DeleteCollection
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly ImageCollectionService _imageCollectionService;
    private readonly ILogger<DeleteCollection> _log;

    public DeleteCollection(BlobServiceClient blobServiceClient, ImageCollectionService imageCollectionService, ILogger<DeleteCollection> log)
    {
        _blobServiceClient = blobServiceClient;
        _imageCollectionService = imageCollectionService;
        _log = log;
    }

    [Function("DeleteCollection")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "delete-collection/{containerName}")] HttpRequestData req,
        string containerName)
    {
        try
        {
            // Require password in header X-Upload-Password
            string providedPassword = null;
            if (req.Headers.TryGetValues("X-Upload-Password", out var headerVals))
            {
                providedPassword = headerVals.FirstOrDefault();
            }

            var configuredPassword = Environment.GetEnvironmentVariable("UPLOAD_PASSWORD");

            if (string.IsNullOrEmpty(configuredPassword) || string.IsNullOrEmpty(providedPassword))
            {
                var badResp = req.CreateResponse(HttpStatusCode.Unauthorized);
                await badResp.WriteStringAsync("Unauthorized");
                return badResp;
            }

            // Basic constant-time comparison
            bool match = configuredPassword.Length == providedPassword.Length;
            if (match)
            {
                for (int i = 0; i < configuredPassword.Length; i++)
                {
                    match &= configuredPassword[i] == providedPassword[i];
                }
            }

            if (!match)
            {
                var badResp = req.CreateResponse(HttpStatusCode.Unauthorized);
                await badResp.WriteStringAsync("Unauthorized");
                return badResp;
            }

            if (string.IsNullOrWhiteSpace(containerName))
            {
                var badResp = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResp.WriteStringAsync("Missing container name");
                return badResp;
            }

            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

            var deleted = await containerClient.DeleteIfExistsAsync();

            if (!deleted)
            {
                var notFound = req.CreateResponse(HttpStatusCode.NotFound);
                await notFound.WriteStringAsync("Container not found");
                return notFound;
            }

            // reload cache so UI updates
            await _imageCollectionService.ReloadCache();

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteStringAsync("ok");
            return response;
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Error deleting container");
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteStringAsync($"Error: {ex.Message}");
            return response;
        }
    }
}
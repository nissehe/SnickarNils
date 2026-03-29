using System.Net;
using System.Linq;
using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace Api;

internal class UpdateCollectionOrder
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly ImageCollectionService _imageCollectionService;
    private readonly ILogger<UpdateCollectionOrder> _log;

    private const string MetaDataContainerName = "metadata";
    private const string ImageCollectionOrderBlobName = "image_collection_order.txt";

    public UpdateCollectionOrder(BlobServiceClient blobServiceClient, ImageCollectionService imageCollectionService, ILogger<UpdateCollectionOrder> log)
    {
        _blobServiceClient = blobServiceClient;
        _imageCollectionService = imageCollectionService;
        _log = log;
    }

    [Function("UpdateCollectionOrder")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "collections/order")] HttpRequestData req)
    {
        try
        {
            // Validate header password
            if (!PasswordValidator.ValidateRequest(req))
            {
                var badResp = req.CreateResponse(HttpStatusCode.Unauthorized);
                await badResp.WriteStringAsync("Unauthorized");
                return badResp;
            }

            var orderedList = await req.ReadFromJsonAsync<List<string>>();

            if (orderedList == null)
            {
                var bad = req.CreateResponse(HttpStatusCode.BadRequest);
                await bad.WriteStringAsync("Invalid payload. Expecting JSON array of container names.");
                return bad;
            }

            var metaContainer = _blobServiceClient.GetBlobContainerClient(MetaDataContainerName);

            // Ensure container exists
            await metaContainer.CreateIfNotExistsAsync();

            var blobClient = metaContainer.GetBlobClient(ImageCollectionOrderBlobName);

            var content = string.Join(Environment.NewLine, orderedList);

            await blobClient.UploadAsync(BinaryData.FromString(content), overwrite: true);

            // Refresh service cache so the change is visible immediately
            await _imageCollectionService.ReloadCache();

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteStringAsync("ok");
            return response;
        }
        catch (Exception ex)
        {
            _log.LogError($"Failed to update collection order - {ex.Message}");
            var resp = req.CreateResponse(HttpStatusCode.InternalServerError);
            await resp.WriteStringAsync("error");
            return resp;
        }
    }
}
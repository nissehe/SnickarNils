using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace Api;

internal class ImageCollectionGet
{
    private readonly ImageCollectionService _imageCollectionService;

    public ImageCollectionGet(ImageCollectionService imageCollectionService)
    {
        _imageCollectionService = imageCollectionService;
    }

    [Function("ImageSetGet")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "ImageCollection/{containerName}")] HttpRequestData req,
        ILogger log, string containerName)
    {
        var imageCollections = await _imageCollectionService.GetCollection(containerName);

        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "application/json");

        await response.WriteStringAsync(JsonSerializer.Serialize(imageCollections));

        return response;
    }
}

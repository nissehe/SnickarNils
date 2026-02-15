using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace Api;

internal class ImageCollectionGet
{
    private readonly ImageCollectionService _imageCollectionService;
    private readonly ILogger<ImageCollectionGet> _logger;

    public ImageCollectionGet(ImageCollectionService imageCollectionService, ILogger<ImageCollectionGet> logger)
    {
        _imageCollectionService = imageCollectionService;
        _logger = logger;
    }

    [Function("ImageSetGet")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "ImageCollection/{containerName}")] HttpRequestData req,
        ILogger log, string containerName)
    {
        var imageCollections = await _imageCollectionService.GetCollection(containerName);

        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "application/json");

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        await response.WriteStringAsync(JsonSerializer.Serialize(imageCollections, options));

        return response;
    }
}

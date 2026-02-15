using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace Api;

internal class ImageCollectionSummariesGet
{
    private readonly ImageCollectionService _imageCollectionService;
    private readonly ILogger<ImageCollectionSummariesGet> _logger;

    public ImageCollectionSummariesGet(ImageCollectionService imageCollectionService, ILogger<ImageCollectionSummariesGet> logger)
    {
        _imageCollectionService = imageCollectionService;
        _logger = logger;
    }

    [Function("ImageCollectionSummariesGet")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "ImageCollectionSummaries")] HttpRequestData req)
    {
        _logger.LogInformation("Handling ImageCollectionSummariesGet request");

        var summaries = await _imageCollectionService.GetSummaries();

        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "application/json");

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        await response.WriteStringAsync(JsonSerializer.Serialize(summaries, options));

        return response;
    }
}

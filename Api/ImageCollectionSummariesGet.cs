using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace Api;

internal class ImageCollectionSummariesGet
{
    private readonly ImageCollectionService _imageCollectionService;

    public ImageCollectionSummariesGet(ImageCollectionService imageCollectionService)
    {
        _imageCollectionService = imageCollectionService;
    }

    [Function("ImageCollectionSummariesGet")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "ImageCollectionSummaries")] HttpRequestData req)
    {
        var summaries = await _imageCollectionService.GetSummaries();

        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "application/json");

        await response.WriteStringAsync(JsonSerializer.Serialize(summaries));

        return response;
    }
}

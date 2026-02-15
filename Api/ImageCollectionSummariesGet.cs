using System.Net;
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
        await response.WriteAsJsonAsync(summaries);

        return response;
    }
}

using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace Api;

internal class Reload
{
    private readonly ImageCollectionService _imageCollectionService;

    public Reload(ImageCollectionService imageCollectionService)
    {
        _imageCollectionService = imageCollectionService;
    }

    [Function("Reload")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "reload")] HttpRequestData req)
    {
        await _imageCollectionService.ReloadCache();

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteStringAsync("ok");

        return response;
    }
}
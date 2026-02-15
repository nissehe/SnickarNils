using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace Api;

internal class Reload
{
    private readonly ImageCollectionService _imageCollectionService;
    private readonly ILogger<Reload> _logger;

    public Reload(ImageCollectionService imageCollectionService, ILogger<Reload> logger)
    {
        _imageCollectionService = imageCollectionService;
        _logger = logger;
    }

    [Function("Reload")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "reload")] HttpRequestData req)
    {
        _logger.LogInformation("Reload invoked");

        await _imageCollectionService.ReloadCache();

        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
        await response.WriteStringAsync("ok");

        return response;
    }
}

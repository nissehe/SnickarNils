using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Api;

internal class ImageCollectionSummariesGet
{
    private readonly ImageCollectionService _imageCollectionService;

    public ImageCollectionSummariesGet(ImageCollectionService imageCollectionService)
    {
        _imageCollectionService = imageCollectionService;
    }

    [FunctionName("ImageCollectionSummariesGet")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "ImageCollectionSummaries")] HttpRequest req,
        ILogger log)
    {

        var summaries = await _imageCollectionService.GetSummaries();

        return new OkObjectResult(summaries);
    }
}

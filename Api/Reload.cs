using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Api;
internal class Reload
{
    private readonly ImageCollectionService _imageCollectionService;

    public Reload(ImageCollectionService imageCollectionService)
    {
        _imageCollectionService = imageCollectionService;
    }

    [FunctionName("Reload")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "reload")] HttpRequest req,
        ILogger log)
    {
        await _imageCollectionService.ReloadCache();

        return new OkObjectResult("ok");
    }
}

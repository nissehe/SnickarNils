using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Api
{
    internal class ImageCollectionGet
    {
        private readonly ImageCollectionService _imageCollectionService;

        public ImageCollectionGet(ImageCollectionService imageCollectionService)
        {
            _imageCollectionService = imageCollectionService;
        }

        [FunctionName("ImageSetGet")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "ImageCollection/{containerName}")] HttpRequest req,
            ILogger log, string containerName)
        {
            var imageCollections = await _imageCollectionService.GetCollection(containerName);

            return new OkObjectResult(imageCollections);
        }
    }
}

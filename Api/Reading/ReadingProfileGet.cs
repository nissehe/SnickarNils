using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace Api.Reading;

internal class ReadingProfileGet
{
    private readonly ReadingService _readingService;

    public ReadingProfileGet(ReadingService readingService)
    {
        _readingService = readingService;
    }

    [Function("ReadingProfileGet")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "reading/profiles/{name}")] HttpRequestData req, string name)
    {
        var profile = await _readingService.GetProfile(name);

        if (profile == null)
        {
            var notFound = req.CreateResponse(HttpStatusCode.NotFound);
            await notFound.WriteStringAsync("Profile not found.");
            return notFound;
        }

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(profile);

        return response;
    }
}

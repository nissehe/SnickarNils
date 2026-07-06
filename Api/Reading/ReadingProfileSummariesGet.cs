using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace Api.Reading;

internal class ReadingProfileSummariesGet
{
    private readonly ReadingService _readingService;

    public ReadingProfileSummariesGet(ReadingService readingService)
    {
        _readingService = readingService;
    }

    [Function("ReadingProfileSummariesGet")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "reading/profiles")] HttpRequestData req)
    {
        var summaries = await _readingService.GetProfileSummaries();

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(summaries);

        return response;
    }
}

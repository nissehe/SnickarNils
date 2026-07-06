using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace Api.Reading;

internal class ReadingStartDateGet
{
    private readonly ReadingService _readingService;

    public ReadingStartDateGet(ReadingService readingService)
    {
        _readingService = readingService;
    }

    [Function("ReadingStartDateGet")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "reading/start-date")] HttpRequestData req)
    {
        var startDate = await _readingService.GetGlobalStartDate();

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(new { StartDate = startDate });

        return response;
    }
}

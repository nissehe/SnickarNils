using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace Api.Reading;

internal class ReadingLogPost
{
    private class LogRequest
    {
        public int Minutes { get; set; }
    }

    private readonly ReadingService _readingService;

    public ReadingLogPost(ReadingService readingService)
    {
        _readingService = readingService;
    }

    [Function("ReadingLogPost")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "reading/profiles/{name}/log")] HttpRequestData req, string name)
    {
        var payload = await req.ReadFromJsonAsync<LogRequest>();

        if (payload == null || payload.Minutes == 0)
        {
            var bad = req.CreateResponse(HttpStatusCode.BadRequest);
            await bad.WriteStringAsync("Minutes must not be zero.");
            return bad;
        }

        var summary = await _readingService.AddLogEntry(name, payload.Minutes);

        if (summary == null)
        {
            var notFound = req.CreateResponse(HttpStatusCode.NotFound);
            await notFound.WriteStringAsync("Profile not found.");
            return notFound;
        }

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(summary);

        return response;
    }
}

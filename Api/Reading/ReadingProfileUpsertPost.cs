using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace Api.Reading;

internal class ReadingProfileUpsertPost
{
    private class UpsertRequest
    {
        public int GoalMinutes { get; set; }
    }

    private readonly ReadingService _readingService;

    public ReadingProfileUpsertPost(ReadingService readingService)
    {
        _readingService = readingService;
    }

    [Function("ReadingProfileUpsertPost")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "reading/profiles/{name}")] HttpRequestData req, string name)
    {
        if (!PasswordValidator.ValidateRequest(req))
        {
            var unauthorized = req.CreateResponse(HttpStatusCode.Unauthorized);
            await unauthorized.WriteStringAsync("Unauthorized");
            return unauthorized;
        }

        var payload = await req.ReadFromJsonAsync<UpsertRequest>();

        if (payload == null || payload.GoalMinutes <= 0)
        {
            var bad = req.CreateResponse(HttpStatusCode.BadRequest);
            await bad.WriteStringAsync("GoalMinutes must be greater than zero.");
            return bad;
        }

        var summary = await _readingService.UpsertProfile(name, payload.GoalMinutes);

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(summary);

        return response;
    }
}

using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace Api.Reading;

internal class ReadingBookPost
{
    private class BookRequest
    {
        public string Title { get; set; }
    }

    private readonly ReadingService _readingService;

    public ReadingBookPost(ReadingService readingService)
    {
        _readingService = readingService;
    }

    [Function("ReadingBookPost")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "reading/profiles/{name}/books")] HttpRequestData req, string name)
    {
        var payload = await req.ReadFromJsonAsync<BookRequest>();

        if (payload == null || string.IsNullOrWhiteSpace(payload.Title))
        {
            var bad = req.CreateResponse(HttpStatusCode.BadRequest);
            await bad.WriteStringAsync("Title is required.");
            return bad;
        }

        var books = await _readingService.AddBook(name, payload.Title.Trim());

        if (books == null)
        {
            var notFound = req.CreateResponse(HttpStatusCode.NotFound);
            await notFound.WriteStringAsync("Profile not found.");
            return notFound;
        }

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(books);

        return response;
    }
}

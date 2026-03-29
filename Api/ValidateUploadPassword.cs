using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace Api;

internal class ValidateUploadPassword
{
    private class PasswordRequest
    {
        public string Password { get; set; }
    }

    [Function("ValidateUploadPassword")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "validate-upload-password")] HttpRequestData req)
    {
        var response = req.CreateResponse();

        try
        {
            var body = await req.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var payload = JsonSerializer.Deserialize<PasswordRequest>(body, options);

            var configured = Environment.GetEnvironmentVariable("UPLOAD_PASSWORD");

            if (string.IsNullOrEmpty(configured) || payload == null || string.IsNullOrEmpty(payload.Password))
            {
                response = req.CreateResponse(HttpStatusCode.Unauthorized);
                await response.WriteStringAsync("Unauthorized");
                return response;
            }

            // Constant-time comparison to reduce timing attack leakage (basic)
            bool match = configured.Length == payload.Password.Length;
            if (match)
            {
                for (int i = 0; i < configured.Length; i++)
                {
                    match &= configured[i] == payload.Password[i];
                }
            }

            if (match)
            {
                response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteStringAsync("ok");
                return response;
            }

            response = req.CreateResponse(HttpStatusCode.Unauthorized);
            await response.WriteStringAsync("Unauthorized");
            return response;
        }
        catch
        {
            response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteStringAsync("Error");
            return response;
        }
    }
}
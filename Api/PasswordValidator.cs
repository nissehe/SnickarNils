using System.Linq;
using Microsoft.Azure.Functions.Worker.Http;

namespace Api;

internal static class PasswordValidator
{
    private const string HeaderName = "X-Upload-Password";

    public static bool ValidateRequest(HttpRequestData req, string passwordFromBody = null)
    {
        // Try header first
        string providedPassword = null;
        if (req.Headers.TryGetValues(HeaderName, out var headerVals))
        {
            providedPassword = headerVals.FirstOrDefault();
        }

        if (string.IsNullOrEmpty(providedPassword))
        {
            providedPassword = passwordFromBody;
        }

        var configuredPassword = Environment.GetEnvironmentVariable("UPLOAD_PASSWORD");

        if (string.IsNullOrEmpty(configuredPassword) || string.IsNullOrEmpty(providedPassword))
        {
            return false;
        }

        // Constant-time comparison
        if (configuredPassword.Length != providedPassword.Length)
        {
            return false;
        }

        bool match = true;
        for (int i = 0; i < configuredPassword.Length; i++)
        {
            match &= configuredPassword[i] == providedPassword[i];
        }

        return match;
    }
}
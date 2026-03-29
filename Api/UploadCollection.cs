using System.Net;
using System.Text.Json;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace Api;

internal class UploadCollection
{
    private readonly BlobServiceClient _blobServiceClient;

    public UploadCollection(BlobServiceClient blobServiceClient)
    {
        _blobServiceClient = blobServiceClient;
    }

    private class UploadImageDto
    {
        public string FileName { get; set; }
        public string Base64 { get; set; }
    }

    private class UploadCollectionRequest
    {
        public string ContainerName { get; set; }
        public string Description { get; set; }
        public string CoverFileName { get; set; }
        public string CoverBase64 { get; set; }
        public List<UploadImageDto> Images { get; set; } = new();
    }

    [Function("UploadCollection")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "upload-collection")] HttpRequestData req)
    {
        try
        {
            var json = await req.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var upload = JsonSerializer.Deserialize<UploadCollectionRequest>(json, options);

            if (upload == null || string.IsNullOrWhiteSpace(upload.ContainerName))
            {
                var bad = req.CreateResponse(HttpStatusCode.BadRequest);
                await bad.WriteStringAsync("Missing container name");
                return bad;
            }

            var containerName = upload.ContainerName.Trim();

            // Create container if it doesn't exist. Make blobs publicly readable.
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

            // Upload description if present
            if (!string.IsNullOrEmpty(upload.Description))
            {
                var descBlob = containerClient.GetBlobClient("description.txt");
                await descBlob.UploadAsync(BinaryData.FromString(upload.Description), overwrite: true);
            }

            // Upload cover if provided. Name as 'cover.<ext>'
            if (!string.IsNullOrEmpty(upload.CoverBase64) && !string.IsNullOrEmpty(upload.CoverFileName))
            {
                var ext = System.IO.Path.GetExtension(upload.CoverFileName) ?? string.Empty;
                var coverBlobName = $"cover{ext}";
                var coverBlob = containerClient.GetBlobClient(coverBlobName);
                var coverBytes = Convert.FromBase64String(upload.CoverBase64);
                using var coverStream = new MemoryStream(coverBytes);
                await coverBlob.UploadAsync(coverStream, overwrite: true);
            }

            // Upload additional images
            if (upload.Images != null)
            {
                foreach (var img in upload.Images)
                {
                    if (string.IsNullOrEmpty(img?.FileName) || string.IsNullOrEmpty(img?.Base64))
                        continue;

                    var blobClient = containerClient.GetBlobClient(img.FileName);
                    var bytes = Convert.FromBase64String(img.Base64);
                    using var ms = new MemoryStream(bytes);
                    await blobClient.UploadAsync(ms, overwrite: true);
                }
            }

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteStringAsync("ok");
            return response;
        }
        catch (Exception ex)
        {
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteStringAsync($"Error: {ex.Message}");
            return response;
        }
    }
}
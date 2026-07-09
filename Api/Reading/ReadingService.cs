using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Data.Reading;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Api.Reading;

internal class ReadingService
{
    private const string ContainerName = "reading";

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly BlobServiceClient _blobServiceClient;
    private readonly ILogger<ReadingService> _log;

    public ReadingService(BlobServiceClient blobServiceClient, ILogger<ReadingService> log)
    {
        _blobServiceClient = blobServiceClient;
        _log = log;
    }

    private class ProfileStorage
    {
        public int GoalMinutes { get; set; }
        public int DailyTargetMinutes { get; set; } = 20;
        public List<ReadingLogEntry> Entries { get; set; } = new();
        public List<ReadingBook> Books { get; set; } = new();
    }

    public async Task<List<ReadingProfileSummary>> GetProfileSummaries()
    {
        var summaries = new List<ReadingProfileSummary>();

        var containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName);

        if (!await containerClient.ExistsAsync())
        {
            return summaries;
        }

        await foreach (BlobItem blob in containerClient.GetBlobsAsync())
        {
            var name = Path.GetFileNameWithoutExtension(blob.Name);

            var storage = await DownloadProfile(containerClient, blob.Name);

            if (storage == null)
            {
                continue;
            }

            summaries.Add(ToSummary(name, storage));
        }

        return summaries.OrderBy(s => s.Name).ToList();
    }

    public async Task<ReadingProfile> GetProfile(string name)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName);

        var storage = await DownloadProfile(containerClient, GetBlobName(name));

        if (storage == null)
        {
            return null;
        }

        var summary = ToSummary(name, storage);

        return new ReadingProfile(summary, storage.Entries, storage.Books);
    }

    public async Task<ReadingProfileSummary> AddLogEntry(string name, int minutes)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName);

        var blobName = GetBlobName(name);

        var storage = await DownloadProfile(containerClient, blobName);

        if (storage == null)
        {
            return null;
        }

        storage.Entries.Add(new ReadingLogEntry { Date = DateTime.UtcNow, Minutes = minutes });

        await UploadProfile(containerClient, blobName, storage);

        return ToSummary(name, storage);
    }

    public async Task<List<ReadingBook>> AddBook(string name, string title)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName);

        var blobName = GetBlobName(name);

        var storage = await DownloadProfile(containerClient, blobName);

        if (storage == null)
        {
            return null;
        }

        storage.Books.Add(new ReadingBook { Title = title });

        await UploadProfile(containerClient, blobName, storage);

        return storage.Books;
    }

    public async Task<DateTime?> GetGlobalStartDate()
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName);

        if (!await containerClient.ExistsAsync())
        {
            return null;
        }

        DateTime? earliest = null;

        await foreach (BlobItem blob in containerClient.GetBlobsAsync())
        {
            var storage = await DownloadProfile(containerClient, blob.Name);

            if (storage == null || !storage.Entries.Any())
            {
                continue;
            }

            var profileEarliest = storage.Entries.Min(e => e.Date);

            if (earliest == null || profileEarliest < earliest)
            {
                earliest = profileEarliest;
            }
        }

        return earliest;
    }

    public async Task<ReadingProfileSummary> UpsertProfile(string name, int goalMinutes, int dailyTargetMinutes)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName);

        await containerClient.CreateIfNotExistsAsync();

        var blobName = GetBlobName(name);

        var storage = await DownloadProfile(containerClient, blobName) ?? new ProfileStorage();

        storage.GoalMinutes = goalMinutes;
        storage.DailyTargetMinutes = dailyTargetMinutes;

        await UploadProfile(containerClient, blobName, storage);

        return ToSummary(name, storage);
    }

    private static string GetBlobName(string name) => $"{name}.json";

    private static ReadingProfileSummary ToSummary(string name, ProfileStorage storage)
    {
        int totalMinutes = storage.Entries.Sum(e => e.Minutes);

        return new ReadingProfileSummary
        {
            Name = name,
            GoalMinutes = storage.GoalMinutes,
            TotalMinutesRead = totalMinutes,
            RemainingMinutes = Math.Max(0, storage.GoalMinutes - totalMinutes),
            DailyTargetMinutes = storage.DailyTargetMinutes
        };
    }

    private async Task<ProfileStorage> DownloadProfile(BlobContainerClient containerClient, string blobName)
    {
        try
        {
            var blobClient = containerClient.GetBlobClient(blobName);

            if (!await blobClient.ExistsAsync())
            {
                return null;
            }

            BlobDownloadResult downloadResult = await blobClient.DownloadContentAsync();

            return JsonSerializer.Deserialize<ProfileStorage>(downloadResult.Content.ToString(), JsonOptions);
        }
        catch (Exception ex)
        {
            _log.LogError($"Failed to download reading profile {blobName} - {ex.Message}");
            return null;
        }
    }

    private static async Task UploadProfile(BlobContainerClient containerClient, string blobName, ProfileStorage storage)
    {
        var blobClient = containerClient.GetBlobClient(blobName);
        var json = JsonSerializer.Serialize(storage);
        await blobClient.UploadAsync(BinaryData.FromString(json), overwrite: true);
    }
}

using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Data;
using Microsoft.Extensions.Logging;

namespace Api;

internal class ImageCollectionService
{
    private const string _descriptionBlobName = "description.txt";
    private const string _coverPhotoBlobNameStart = "cover.";
    private const string _metaDataContainerName = "metadata";
    private const string _imageCollectionOrderBlobName = "image_collection_order.txt";

    private readonly BlobServiceClient _blobServiceClient;
    private readonly ILogger _log;

    private Cache _cache;

    public ImageCollectionService(BlobServiceClient blobServiceClient, ILogger log)
    {
        _blobServiceClient = blobServiceClient;
        _log = log;
        _cache = new();

        System.Diagnostics.Debug.WriteLine("ImageCollectionService constructor");
    }

    public async Task<List<ImageCollectionSummary>> GetSummaries()
    {
        if(_cache.ImageCollectionSummaries != null)
        {
            return _cache.ImageCollectionSummaries;
        }

        var imageCollectionSummaries = new List<ImageCollectionSummary>();

        var containers = _blobServiceClient.GetBlobContainersAsync();

        await foreach (Page<BlobContainerItem> page in containers.AsPages())
        {
            foreach (BlobContainerItem container in page.Values.OrderBy(c => c.Name))
            {
                var imageSet = await GetSummary(container.Name);

                if(imageSet != null)
                {
                    imageCollectionSummaries.Add(imageSet);
                }
            }
        }

        var imageCollectionOrder = await GetImageCollectionOrder();

        var orderedImageCollectionSummaries = OrderImageCollections(imageCollectionSummaries, imageCollectionOrder);

        _cache.ImageCollectionSummaries = orderedImageCollectionSummaries;

        return orderedImageCollectionSummaries;
    }

    public async Task<ImageCollection> GetCollection(string containerName)
    {
        var collectionCache = _cache.GetImageCollection(containerName);

        if (collectionCache != null)
        {
            return collectionCache;
        }

        var imageCollectionSummary = await GetSummary(containerName);

        var imageUris = GetImageUris(containerName);

        var imageCollection = new ImageCollection(imageCollectionSummary, imageUris);

        _cache.AddImageCollection(containerName, imageCollection);

        return imageCollection;
    }

    public async Task ReloadCache()
    {
        _cache.Wipe();

        await GetSummaries();
    }

    private async Task<ImageCollectionSummary> GetSummary(string containerName)
    {
        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

            string coverImageUri = GetCoverImageUri(containerClient);

            string description = await GetDescriptionText(containerClient);

            if (coverImageUri == null && description == null)
            {
                return null;
            }

            return new ImageCollectionSummary()
            {
                ContainerName = containerName,
                CoverImageUri = coverImageUri,
                Description = description
            };
        }
        catch (Exception ex)
        {
            _log.LogError($"Failed to create ImageSetSummary for {containerName} - {ex.Message}");
            return null;
        }
    }

    private List<string> GetImageUris(string containerName)
    {
        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

            var imageBlobs = containerClient.GetBlobs().Where(b => !b.Name.Equals(_descriptionBlobName, StringComparison.OrdinalIgnoreCase)
                                                                && !b.Name.StartsWith(_coverPhotoBlobNameStart, StringComparison.OrdinalIgnoreCase));

            var imageUris = imageBlobs.OrderBy(b => b.Name)
                .Select(b => GetBlobUri(containerClient, b))
                .ToList();

            return imageUris;
        }
        catch (Exception ex)
        {
            _log.LogError($"Failed to get image uris for {containerName} - {ex.Message}");
            return null;
        }
    }

    private async Task<List<string>> GetImageCollectionOrder()
    {
        try
        {
            var metaDataContainer = _blobServiceClient?.GetBlobContainerClient(_metaDataContainerName);

            var imageCollectionOrderClient = metaDataContainer?.GetBlobClient(_imageCollectionOrderBlobName);

            BlobDownloadResult downloadResult = await imageCollectionOrderClient?.DownloadContentAsync();
            var imageSetOrder = downloadResult?.Content?.ToString();

            var imageCollectionOrderList = imageSetOrder
                ?.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
                ?.ToList();

            return imageCollectionOrderList ?? new List<string>();
        }
        catch (Exception)
        {
            return new List<string>();
        }
    }

    private List<ImageCollectionSummary> OrderImageCollections(List<ImageCollectionSummary> imageCollections, List<string> imageCollectionOrder)
    {

        var sortedCollections = new List<ImageCollectionSummary>();

        foreach(string containerName in imageCollectionOrder)
        {
            var imageCollection = imageCollections.FirstOrDefault(x => x.ContainerName == containerName);

            if(imageCollection == null)
            {
                continue;
            }

            imageCollections.Remove(imageCollection);

            sortedCollections.Add(imageCollection);
        }

        sortedCollections.AddRange(imageCollections);

        return sortedCollections;
    }

    private static async Task<string> GetDescriptionText(BlobContainerClient containerClient)
    {
        string description = null;

        var descriptionBlob = containerClient.GetBlobs()
            .Where(b => b.Name.Equals(_descriptionBlobName, StringComparison.OrdinalIgnoreCase))
            .FirstOrDefault();

        if (descriptionBlob != null)
        {
            var descriptionBlobClient = containerClient.GetBlobClient(descriptionBlob.Name);
            BlobDownloadResult downloadResult = await descriptionBlobClient.DownloadContentAsync();
            description = downloadResult.Content.ToString();
        }

        return description;
    }

    private static string GetCoverImageUri(BlobContainerClient containerClient)
    {
        string coverImageUri = null;

        var coverImage = containerClient.GetBlobs()
            .Where(b => b.Name.StartsWith(_coverPhotoBlobNameStart, StringComparison.OrdinalIgnoreCase))
            .FirstOrDefault();

        if (coverImage != null)
        {
            coverImageUri = GetBlobUri(containerClient, coverImage);
        }

        return coverImageUri;
    }

    private static string GetBlobUri(BlobContainerClient containerClient, BlobItem blobItem)
    {
        return $"{containerClient.Uri}/{blobItem.Name}";
    }
}

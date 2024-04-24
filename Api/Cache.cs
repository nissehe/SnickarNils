using Data;

namespace Api;

internal class Cache
{
    private Dictionary<string, ImageCollection> _imageCollections = new();

    public List<ImageCollectionSummary> ImageCollectionSummaries;

    public ImageCollection GetImageCollection(string containerName)
    {
        return _imageCollections.ContainsKey(containerName)
            ? _imageCollections[containerName]
            : null;
    }

    public void AddImageCollection(string containerName, ImageCollection imageCollection)
    {
        _imageCollections[containerName] = imageCollection;
    }

    public void Wipe()
    {
        ImageCollectionSummaries = null;
        _imageCollections.Clear();
    }
}

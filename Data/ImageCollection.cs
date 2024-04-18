using System.Collections.Generic;

namespace Data
{
    public class ImageCollection : ImageCollectionSummary
    {
        public List<string> ImageUris { get; set; }

        public ImageCollection()
        {
        }

        public ImageCollection(ImageCollectionSummary imageCollectionSummary, List<string> imageUris)
        {
            ContainerName = imageCollectionSummary.ContainerName;
            CoverImageUri = imageCollectionSummary.CoverImageUri;
            Description = imageCollectionSummary.Description;
            ImageUris = imageUris;
        }
    }
}

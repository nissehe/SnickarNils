
# Photo gallery
A Blazor static web app feteches collections of image urls and descriptions from an Azure functions API.
The API in turn expect to find image collections and desciptions in Azure Storage containers.
The storage account containing the containers must allow blob anonymous access, and the containters must have anonymous read access for the blobs.
https://learn.microsoft.com/en-us/azure/storage/blobs/anonymous-read-access-configure?tabs=portal

## Image structure and naming convention
Place a collection of images in its own storage container.
Name the cover photo _cover.*_
Name the blob containing the collection's description _description.txt_
The container must contain at least one of the two special blobs above to be considered by the application.
The rest of the images in the container are displayed in name order.
The summaries on the overview page are displayed in container name order, within the pages from the storage api.

A storage container named 'metadata' may contain a file named 'image_collection_order.txt'. This file can contain a newline separated list of image container names that will control the order of the image collections in the frontend. 
Containers not included in the list will be displayed in alphabetical order after the specified containers.

## Cache
The API keeps a cache of the contents of the Azure storage containers. To wipe the cache after making changes to the containers, go to the page /reload

## Deploy
CI/CD via Azure pipeline
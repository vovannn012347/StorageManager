using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.Extensions.Options;
using System.Web;

namespace StorageManager
{
    //kernel.Bind<IStorageManager>().ToMethod(ctx =>
    //{
    //return InitStorage();
    //}).InSingletonScope();

    public class CloudSasResult
    {
        public string Url { get; internal set; }
        public DateTimeOffset UrlExpire { get; internal set; }
    }

    public interface IAzureBlobOptions
    {
        public string StorageBlobConnectionString { get; set; }

        public string ContainerName { get; set; }
        public string StaticContainerName { get; set; }
        public string SharedContainerName { get; set; }
    }

    public interface IAzureStorageService<T> where T : IAzureBlobOptions
    {
        public string StaticContainer { get; }
        public string PrivateContainer { get; }
        public string SharedContainer { get; }
        CloudSasResult GetFileSas(string container, string fileName);
        CloudSasResult GetFileSas(string container, string fileName, BlobSasPermissions permissions);
        Task<bool> CheckFileExists(string container, string fileName);
        Task TransferFile(string containerFrom, string fileNameFrom, string containerTo, string fileNameTo, bool overwrite);
        Task UploadFileAsync(string container, string fileName, Stream stream, bool overwrite);
    }

    public class AzureStorageService<T> : IAzureStorageService<T> where T : IAzureBlobOptions
    {
        private readonly BlobServiceClient _blobServiceClient;

        public string StaticContainer { get; }
        public string SharedContainer { get; }
        public string PrivateContainer { get; }

        public AzureStorageService(IOptionsMonitor<T> optionsAccessor)
        {
            PrivateContainer = optionsAccessor.CurrentValue.ContainerName;
            _blobServiceClient = new BlobServiceClient(optionsAccessor.CurrentValue.StorageBlobConnectionString);

            StaticContainer = string.IsNullOrEmpty(optionsAccessor.CurrentValue.SharedContainerName) ? "static-container" : optionsAccessor.CurrentValue.SharedContainerName;
            SharedContainer = string.IsNullOrEmpty(optionsAccessor.CurrentValue.ContainerName) ? "shared-container" : optionsAccessor.CurrentValue.ContainerName;

        }

        public async Task<bool> CheckFileExists(string container, string genFileName)
        {
            var blobContainerClient = _blobServiceClient.GetBlobContainerClient(container);
            var blobClient = blobContainerClient.GetBlobClient(genFileName);

            return await blobClient.ExistsAsync();
        }

        public CloudSasResult GetFileSas(string container, string fileName)
        {
            return GetFileSas(container, fileName, BlobSasPermissions.Read);
        }

        public CloudSasResult GetFileSas(string container, string fileName, BlobSasPermissions permissions)
        {
            var blobContainerClient = _blobServiceClient.GetBlobContainerClient(container);
            var blobClient = blobContainerClient.GetBlobClient(fileName);
            var expiresOn = DateTimeOffset.UtcNow.AddHours(1);
            BlobSasBuilder sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = container.ToLower(),
                BlobName = fileName,
                Resource = "b",
                StartsOn = DateTimeOffset.UtcNow,
                ExpiresOn = expiresOn,
                Protocol = SasProtocol.Https

            };
            sasBuilder.SetPermissions(permissions);
            Uri sasToken = blobClient.GenerateSasUri(sasBuilder);

            return new CloudSasResult{
                Url = sasToken.ToString(),
                UrlExpire = expiresOn
            };
        }

        public async Task TransferFile(string containerFrom, string fileNameFrom, string containerTo, string fileNameTo, bool overwrite)
        {
            var fromContainerClient = _blobServiceClient.GetBlobContainerClient(containerFrom);
            var toContainerClient = _blobServiceClient.GetBlobContainerClient(containerTo);

            var sourceBlobClient = fromContainerClient.GetBlobClient(fileNameFrom);
            var destinationBlobClient = toContainerClient.GetBlobClient(fileNameTo);

            await destinationBlobClient.StartCopyFromUriAsync(sourceBlobClient.Uri);
            await sourceBlobClient.DeleteIfExistsAsync();
        }

        public async Task UploadFileAsync(string container, string fileName, Stream stream, bool overwrite)
        {
            var blobContainerClient = _blobServiceClient.GetBlobContainerClient(container);
            var blobClient = blobContainerClient.GetBlobClient(fileName);

            await blobClient.UploadAsync(stream, overwrite);
        }

        //public Stream GetFileStream(string fileName, string defaultVirtualFilePath)
        //{
        //    try
        //    {
        //        var decodedName = HttpUtility.UrlDecode(fileName);
        //        var blockBlob = _container.GetBlockBlobReference(decodedName);
        //        var memoryStream = new MemoryStream();
        //        blockBlob.OpenRead().CopyTo(memoryStream);
        //        memoryStream.Seek(0, SeekOrigin.Begin);
        //        return memoryStream;
        //    }
        //    catch (Exception ex)
        //    {
        //        LogHolder.MainLog.ErrorException("Azure GetFileStream exception", ex);
        //        // TODO: change this to return null or rethink
        //        using (var stream = new FileStream(HostingEnvironment.MapPath(defaultVirtualFilePath ?? "~/Content/images/image-not-available.png"), FileMode.Open, FileAccess.Read))
        //        {
        //            var memoryStream = new MemoryStream();
        //            stream.CopyTo(memoryStream);
        //            memoryStream.Seek(0, SeekOrigin.Begin);
        //            return memoryStream;
        //        }
        //    }
        //}

        //public long GetFileSize(string fileName)
        //{
        //    try
        //    {
        //        var decodedName = HttpUtility.UrlDecode(fileName);
        //        var blockBlob = _container.GetBlockBlobReference(decodedName);

        //        blockBlob.FetchAttributes();
        //        return blockBlob.Properties.Length;
        //    }
        //    catch (Exception ex)
        //    {
        //        LogHolder.MainLog.ErrorException("Azure GetFileSize exception", ex);
        //        return -1;
        //    }
        //}
    }
}

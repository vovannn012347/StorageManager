using Azure.Storage.Sas;
using StorageManager.Models;

namespace StorageManager.Interfaces
{
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
}

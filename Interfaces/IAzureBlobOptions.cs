namespace StorageManager.Interfaces
{
    public interface IAzureBlobOptions
    {
        public string StorageBlobConnectionString { get; set; }

        public string ContainerName { get; set; }
        public string StaticContainerName { get; set; }
        public string SharedContainerName { get; set; }
    }
}

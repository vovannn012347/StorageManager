namespace StorageManager.Models
{
    public class CloudSasResult
    {
        public string Url { get; internal set; }
        public DateTimeOffset UrlExpire { get; internal set; }
    }
}

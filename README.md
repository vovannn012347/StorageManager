# StorageManager
Handmade private storage package, made for Finbuckle.Multitenant, requires per-tenant Options definition that implements IAzureBlobOptions

# Example code

Options class definition (requires container names)
~~~
public class AzureBlobOptions : IAzureBlobOptions
{
	public string StorageBlobConnectionString { get; set; }
	public string ContainerName { get; set; }
	public string StaticContainerName { get; set; }
	public string SharedContainerName { get; set; }
}
~~~

Example code where container names are resolved from tenant options
~~~
builder.Services.ConfigurePerTenant<AzureBlobOptions, AppTenantInfo>((options, tenantInfo) =>
{
    options.StorageBlobConnectionString = tenantInfo.StorageBlobConnectionString;
    options.ContainerName = tenantInfo.ContainerName;
    options.StaticContainerName = tenantInfo.StaticContainerName;
    options.SharedContainerName = tenantInfo.SharedContainerName;
});

builder.Services.AddSingleton<IAzureStorageService<AzureBlobOptions>, AzureStorageService<AzureBlobOptions>>();
~~~
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
namespace GuestBook.Web.Services;
public class BlobStorageService
{
    private readonly BlobContainerClient _container;
    public BlobStorageService(IConfiguration config)
    {
        var connection = config.GetConnectionString("TableStorage") ?? "UseDevelopmentStorage=true";
        _container = new BlobServiceClient(connection).GetBlobContainerClient("guestbookpics");
        _container.CreateIfNotExists(PublicAccessType.None);
    }
    public async Task<string> UploadPhotoAsync(IFormFile file)
    {
        if (file.Length > 5 * 1024 * 1024) throw new InvalidOperationException("Фото не более 5 МБ");
        if (file.ContentType is not ("image/jpeg" or "image/png" or "image/webp")) throw new InvalidOperationException("Допустимы JPG, PNG, WEBP");
        var name = $"photo_{Guid.NewGuid():N}" + (file.ContentType == "image/png" ? ".png" : file.ContentType == "image/webp" ? ".webp" : ".jpg");
        var client = _container.GetBlobClient(name);
        await using var stream = file.OpenReadStream();
        await client.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType });
        return name;
    }
    public async Task<List<string>> GetAllBlobNamesAsync()
    {
        var names = new List<string>();
        await foreach (var item in _container.GetBlobsAsync()) names.Add(item.Name);
        return names;
    }
    public async Task<(Stream Stream, string ContentType)?> OpenAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Contains('/') || name.Contains('\\')) return null;
        var client = _container.GetBlobClient(name);
        if (!await client.ExistsAsync()) return null;
        var result = await client.DownloadStreamingAsync();
        return (result.Value.Content, result.Value.Details.ContentType ?? "application/octet-stream");
    }
    public Task DeleteBlobAsync(string name) => _container.GetBlobClient(name).DeleteIfExistsAsync();
}

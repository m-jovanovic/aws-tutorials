namespace Stackable.Api.Services;

public interface IFileStorageService
{
    Task<string> UploadFileAsync(IFormFile file, string fileName);
    Task<Stream> DownloadFileAsync(string fileName);
    Task DeleteFileAsync(string fileName);
} 
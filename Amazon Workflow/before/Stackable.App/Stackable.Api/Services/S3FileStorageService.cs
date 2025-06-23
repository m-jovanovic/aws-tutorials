using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using Stackable.Api.Models;

namespace Stackable.Api.Services;

public class S3FileStorageService : IFileStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly S3Settings _settings;

    public S3FileStorageService(IAmazonS3 s3Client, IOptions<S3Settings> settings)
    {
        _s3Client = s3Client;
        _settings = settings.Value;
    }

    public async Task<string> UploadFileAsync(IFormFile file, string fileName)
    {
        using var stream = file.OpenReadStream();
        
        var putRequest = new PutObjectRequest
        {
            BucketName = _settings.BucketName,
            Key = fileName,
            InputStream = stream,
            ContentType = file.ContentType
        };

        await _s3Client.PutObjectAsync(putRequest);

        return fileName;
    }

    public async Task<Stream> DownloadFileAsync(string fileName)
    {
        var getRequest = new GetObjectRequest
        {
            BucketName = _settings.BucketName,
            Key = fileName
        };

        var response = await _s3Client.GetObjectAsync(getRequest);
        return response.ResponseStream;
    }

    public async Task DeleteFileAsync(string fileName)
    {
        var deleteRequest = new DeleteObjectRequest
        {
            BucketName = _settings.BucketName,
            Key = fileName
        };

        await _s3Client.DeleteObjectAsync(deleteRequest);
    }
} 
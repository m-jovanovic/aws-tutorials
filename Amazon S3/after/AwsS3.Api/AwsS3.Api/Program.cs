using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using AwsS3.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors();

builder.Services.Configure<S3Settings>(builder.Configuration.GetSection("S3Settings"));

builder.Services.AddSingleton<IAmazonS3>(sp =>
{
    var s3Settings = sp.GetRequiredService<IOptions<S3Settings>>().Value;
    var config = new AmazonS3Config
    {
        RegionEndpoint = RegionEndpoint.GetBySystemName(s3Settings.Region)
    };

    return new AmazonS3Client(config);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseCors(policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
}

app.MapPost("images", async ([FromForm] IFormFile file, IAmazonS3 s3Client, IOptions<S3Settings> s3Settings) =>
{
    if (file.Length == 0)
    {
        return Results.BadRequest("No file uploaded");
    }

    using var stream = file.OpenReadStream();

    var key = Guid.NewGuid();
    var putRequest = new PutObjectRequest
    {
        BucketName = s3Settings.Value.BucketName,
        Key = $"images/{key}",
        InputStream = stream,
        ContentType = file.ContentType,
        Metadata =
        {
            ["file-name"] = file.FileName
        }
    };

    await s3Client.PutObjectAsync(putRequest);

    return Results.Ok(key);
})
.DisableAntiforgery();

app.MapGet("images/{key}", async (string key, IAmazonS3 s3Client, IOptions<S3Settings> s3Settings) =>
{
    var getRequest = new GetObjectRequest
    {
        BucketName = s3Settings.Value.BucketName,
        Key = $"images/{key}"
    };

    var response = await s3Client.GetObjectAsync(getRequest);

    return Results.File(response.ResponseStream, response.Headers.ContentType, response.Metadata["file-name"]);
});

app.MapDelete("images/{key}", async (string key, IAmazonS3 s3Client, IOptions<S3Settings> s3Settings) =>
{
    var getRequest = new DeleteObjectRequest
    {
        BucketName = s3Settings.Value.BucketName,
        Key = $"images/{key}"
    };
    
    await s3Client.DeleteObjectAsync(getRequest);
    
    return Results.Ok($"File {key} deleted successfully");
});

app.MapGet("images/{key}/presigned", (string key, IAmazonS3 s3Client, IOptions<S3Settings> s3Settings) =>
{
    try
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = s3Settings.Value.BucketName,
            Key = $"images/{key}",
            Verb = HttpVerb.GET,
            Expires = DateTime.UtcNow.AddMinutes(15)
        };

        string preSignedUrl = s3Client.GetPreSignedURL(request);

        return Results.Ok(new { key, url = preSignedUrl });
    }
    catch (AmazonS3Exception ex)
    {
        return Results.BadRequest($"S3 error generating pre-signed URL: {ex.Message}");
    }
});

app.MapPost("images/presigned", (
    string fileName,
    string contentType,
    IAmazonS3 s3Client,
    IOptions<S3Settings> s3Settings) =>
{
    try
    {
        var key = Guid.NewGuid();
        var request = new GetPreSignedUrlRequest
        {
            BucketName = s3Settings.Value.BucketName,
            Key = $"images/{key}",
            Verb = HttpVerb.PUT,
            Expires = DateTime.UtcNow.AddMinutes(15),
            ContentType = contentType,
            Metadata =
            {
                ["file-name"] = fileName
            }
        };

        string preSignedUrl = s3Client.GetPreSignedURL(request);

        return Results.Ok(new { key, url = preSignedUrl });
    }
    catch (AmazonS3Exception ex)
    {
        return Results.BadRequest($"S3 error generating pre-signed URL: {ex.Message}");
    }
});

// Multipart upload
app.MapPost("images/start-multipart", async (
    string fileName,
    string contentType,
    IAmazonS3 s3Client,
    IOptions<S3Settings> s3Settings) =>
{
    try
    {
        var key = Guid.NewGuid();
        var request = new InitiateMultipartUploadRequest
        {
            BucketName = s3Settings.Value.BucketName,
            Key = $"images/{key}",
            ContentType = contentType,
            Metadata =
            {
                ["file-name"] = fileName
            }
        };

        var response = await s3Client.InitiateMultipartUploadAsync(request);

        return Results.Ok(new { key, uploadId = response.UploadId });
    }
    catch (AmazonS3Exception ex)
    {
        return Results.BadRequest($"S3 error starting multipart upload: {ex.Message}");
    }
});

app.MapPost("images/{key}/presigned-part", (
    string key,
    string uploadId,
    int partNumber,
    IAmazonS3 s3Client,
    IOptions<S3Settings> s3Settings) =>
{
    try
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = s3Settings.Value.BucketName,
            Key = $"images/{key}",
            Verb = HttpVerb.PUT,
            Expires = DateTime.UtcNow.AddMinutes(15),
            UploadId = uploadId,
            PartNumber = partNumber
        };

        string preSignedUrl = s3Client.GetPreSignedURL(request);

        return Results.Ok(new { key, url = preSignedUrl });
    }
    catch (AmazonS3Exception ex)
    {
        return Results.BadRequest($"S3 error generating pre-signed URL for part: {ex.Message}");
    }
});

app.MapPost("images/{key}/complete-multipart", async (
    string key,
    CompleteMultipartUpload complete,
    IAmazonS3 s3Client,
    IOptions<S3Settings> s3Settings) =>
{
    try
    {
        var request = new CompleteMultipartUploadRequest
        {
            BucketName = s3Settings.Value.BucketName,
            Key = $"images/{key}",
            UploadId = complete.UploadId,
            PartETags = complete.Parts.Select(p => new PartETag(p.PartNumber, p.ETag)).ToList()
        };

        var response = await s3Client.CompleteMultipartUploadAsync(request);

        return Results.Ok(new { key, location = response.Location });
    }
    catch (AmazonS3Exception ex)
    {
        return Results.BadRequest($"S3 error completing multipart upload: {ex.Message}");
    }
});

app.Run();

using Amazon;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using GitHub.Api;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

string keyPrefix = $"{builder.Environment.EnvironmentName}/{builder.Environment.ApplicationName}/";
builder.Configuration.AddSecretsManager(
    region: RegionEndpoint.EUWest3,
    configurator: options =>
    {
        options.KeyGenerator = (_, secretName) => secretName.Replace(keyPrefix, string.Empty).Replace("__", ":");

        //options.SecretFilter = secret => secret.Name.StartsWith(keyPrefix);

        options.ListSecretsFilters = [new Filter { Key = FilterNameStringType.Name, Values = [keyPrefix] }];

        //options.PollingInterval = TimeSpan.FromHours(1);
    });

builder.Services.AddOptions<GitHubSettings>()
    .BindConfiguration(GitHubSettings.ConfigurationSection)
    .ValidateDataAnnotations();

builder.Services.AddTransient<GitHubAuthenticationHandler>();

builder.Services.AddHttpClient<GitHubService>((sp, httpClient) =>
    {
        var settings = sp.GetRequiredService<IOptions<GitHubSettings>>().Value;

        httpClient.BaseAddress = new Uri(settings.BaseAddress);
    })
    .AddHttpMessageHandler<GitHubAuthenticationHandler>();

builder.Configuration.AddUserSecrets<Program>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("users/{username}", async (string username, GitHubService gitHubService) =>
{
    var user = await gitHubService.GetByUserNameAsync(username);

    return Results.Ok(user);
});

app.MapPatch("users/bio", async (UpdateBioRequest request, GitHubService gitHubService) =>
{
    var response = await gitHubService.UpdateBioAsync(request);

    response.EnsureSuccessStatusCode();

    return Results.NoContent();
});

app.Run();

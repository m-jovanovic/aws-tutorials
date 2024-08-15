using Amazon;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Microsoft.Extensions.Options;

namespace GitHub.Api;

public class GitHubAuthenticationHandler(IOptions<GitHubSettings> options) : DelegatingHandler
{
    private readonly GitHubSettings _gitHubSettings = options.Value;

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        //IAmazonSecretsManager client = new AmazonSecretsManagerClient(RegionEndpoint.EUWest3);

        //GetSecretValueRequest secretRequest = new GetSecretValueRequest
        //{
        //    SecretId = "GitHubAccessToken"
        //};

        //GetSecretValueResponse secretResponse = await client.GetSecretValueAsync(secretRequest);

        request.Headers.Add("Authorization", _gitHubSettings.AccessToken);
        request.Headers.Add("User-Agent", _gitHubSettings.UserAgent);

        return await base.SendAsync(request, cancellationToken);
    }
}

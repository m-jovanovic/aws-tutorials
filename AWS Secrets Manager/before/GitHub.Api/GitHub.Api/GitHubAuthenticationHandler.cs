using Microsoft.Extensions.Options;

namespace GitHub.Api;

public class GitHubAuthenticationHandler(IOptions<GitHubSettings> options) : DelegatingHandler
{
    private readonly GitHubSettings _gitHubSettings = options.Value;

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        request.Headers.Add("Authorization", _gitHubSettings.AccessToken);
        request.Headers.Add("User-Agent", _gitHubSettings.UserAgent);

        return await base.SendAsync(request, cancellationToken);
    }
}

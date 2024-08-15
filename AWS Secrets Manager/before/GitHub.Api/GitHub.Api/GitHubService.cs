namespace GitHub.Api;

public sealed class GitHubService(HttpClient httpClient)
{
    public async Task<GitHubUser?> GetByUserNameAsync(string username)
    {
        var user = await httpClient.GetFromJsonAsync<GitHubUser>($"users/{username}");

        return user;
    }

    public async Task<HttpResponseMessage> UpdateBioAsync(UpdateBioRequest request)
    {
        var response = await httpClient.PatchAsJsonAsync("user", request);

        return response;
    }
}

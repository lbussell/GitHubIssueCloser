namespace GitHubIssueCloser;

using Octokit;
using System.Collections.Generic;

public static class GitHubHelper
{
    private const int RequestsPerPage = 100;

    public static string GetEnvToken()
    {
        var githubToken = Environment.GetEnvironmentVariable(Settings.TokenEnvironmentVariable);
        if (string.IsNullOrEmpty(githubToken))
        {
            throw new Exception($"Please set {Settings.TokenEnvironmentVariable} environment variable.");
        }
        return githubToken;
    }

    public static IGitHubClient GetClient(string token)
    {
        var client = new GitHubClient(new ProductHeaderValue(Settings.Id));
        var credentials = new Credentials(token);
        client.Credentials = credentials;
        return client;
    }

    public static Task<IReadOnlyList<Issue>> GetIssues(IGitHubClient client, string owner, string repo)
    {
        return client.Issue.GetAllForRepository(owner, repo);
    }

    public static async Task<IEnumerable<IssueCloseData>> GetIssuesByLabel(
        IGitHubClient client, string owner, string repo, string? label = null)
    {
        SearchIssuesRequest request = CreateSearchRequest(owner, repo, label);

        List<SearchIssuesResult> results = [];

        var result = await client.Search.SearchIssues(request);
        results.Add(result);

        // If we recieved the max number of results, get more pages
        while (result.Items.Count == RequestsPerPage && request.Page < 10)
        {
            request.Page += 1;
            result = await client.Search.SearchIssues(request);
            results.Add(result);
        }

        return results.SelectMany(result => 
            result.Items.Select(issue =>
                new IssueCloseData(owner, repo, issue.Number, issue.Title, issue.HtmlUrl)));
    }

    private static SearchIssuesRequest CreateSearchRequest(string owner, string repo, string? label)
    {
        var request = new SearchIssuesRequest();
        request.Repos.Add(owner, repo);
        request.Type = IssueTypeQualifier.Issue;
        request.State = ItemState.Open;

        if (label != null)
        {
            request.Labels = [label];
        }

        request.PerPage = RequestsPerPage;
        return request;
    }

    public static async Task<bool> TryCloseIssue(IIssuesClient issuesClient, IssueCloseData issue)
    {
        try
        {
            await issuesClient.Update(
                issue.Owner,
                issue.Repo,
                issue.Number,
                new IssueUpdate { State = ItemState.Closed });

            Console.WriteLine($"Closed issue {issue.Number}.");

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to close issue {issue.Number}: {ex.Message}");
            return false;
        }
    }
}

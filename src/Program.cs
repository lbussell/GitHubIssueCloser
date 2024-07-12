namespace GitHubIssueCloser;

using System.Text.Json;
using Octokit;

public class Program
{
    private const string GetIssuesCommand = "get";
    private const string CloseIssuesCommand = "Close";

    public static async Task Main(string[] args)
    {
        string pat = GitHubHelper.GetEnvToken();
        IGitHubClient github = GitHubHelper.GetClient(pat);

        string verb = ParseVerb(args);
        switch (verb)
        {
            case GetIssuesCommand:
                await GetAsync(github, args[1..]);
                break;
            case CloseIssuesCommand:
                await CloseAsync(github, args[1..]);
                break;
            default:
                throw new Exception($"Unknown command: {verb}");
        }
    }

    private static async Task GetAsync(IGitHubClient client, string[] args)
    {
        string owner;
        string repo;
        string outputPath;
        string? label = null;

        if (args.Length < 2)
        {
            throw new Exception("Please provide arguments: owner, repo, (optional) label, output");
        }
        else if (args.Length == 3)
        {
            owner = args[0];
            repo = args[1];
            outputPath = args[2];
        }
        else
        {
            owner = args[0];
            repo = args[1];
            label = args[2];
            outputPath = args[3];
        }

        IEnumerable<IssueCloseData> issues = 
            await GitHubHelper.GetIssuesByLabel(client, owner, repo, label);
        WriteJson(issues, outputPath);
    }

    private static async Task CloseAsync(IGitHubClient client, string[] args)
    {
        if (args.Length != 1)
        {
            throw new Exception("Please provide json file containing issues to delete.");
        }

        string issuesJsonFilePath = args[0];
        string issuesJson = File.ReadAllText(issuesJsonFilePath);
        IEnumerable<IssueCloseData> issues = JsonSerializer.Deserialize<IEnumerable<IssueCloseData>>(issuesJson)
            ?? throw new Exception($"Failed to deserialize issues from {issuesJsonFilePath}");

        Task<bool>[] closedIssues = issues
            .Select(async issue => await GitHubHelper.TryCloseIssue(client.Issue, issue))
            .ToArray();
        await Task.WhenAll(closedIssues);

        int closed = closedIssues.Count(closed => closed.Result);
        Console.WriteLine($"Closed {closed}/{closedIssues.Length} issues.");
    }

    private static string ParseVerb(string[] args)
    {
        if (args.Length == 0 || string.IsNullOrEmpty(args[0]))
        {
            throw new Exception($"""
                Please provide a command.
                Available commands are: {GetIssuesCommand}, {CloseIssuesCommand}
                """);
        }

        return args[0];
    }

    private static void WriteJson(IEnumerable<IssueCloseData> issues, string outputPath)
    {
        string issuesJson = JsonSerializer.Serialize(issues, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(outputPath, issuesJson);
        Console.WriteLine($"{issues.Count()} issues written to: " + outputPath);
    }
}

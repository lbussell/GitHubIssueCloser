namespace GitHubIssueCloser;

public record IssueCloseData(string Owner, string Repo, int Number, string Title, string Url);

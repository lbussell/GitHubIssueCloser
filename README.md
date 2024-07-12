# GitHub Issue Closer

Tool for programmatically closing GitHub issues.
Good for one-time cleanup of large amounts of issues.

> [!CAUTION]
> Read the source code carefully before using this program.
> Use at your own risk.
> I am not responsible if this program doesn't do what you think it does.

## Usage

First, set `GITHUB_TOKEN` to a valid GitHub PAT.

```pwsh
PS> $Env:GITHUB_TOKEN = "your gh pat here"
```

The `get` command writes a JSON file containing which issues were found.
Give this JSON back to the `close` command to close all the specified issues.

Find issues:

```
dotnet run -- get <owner> <repo> <output json file> 
```

Find issues by label:

```
dotnet run -- get <owner> <repo> <label> <output json file> 
```

Close issues:

```
dotnet run -- close issues-to-close.json
```

## Json Schema

The `Title` and `Url` fields are not read for closing purposes and may be left blank if desired.
They exist to make it easier to tell which issues were found by your search query.

```json
[
  {
    "Owner": "owner",
    "Repo": "repo",
    "Number": 1,
    "Title": "My issue",
    "Url": "https://github.com/owner/repo/issues/1"
  },
  {
    "Owner": "owner",
    "Repo": "repo",
    "Number": 9999,
    "Title": "My other issue",
    "Url": "https://github.com/owner/repo/issues/9999"
  }
]
```

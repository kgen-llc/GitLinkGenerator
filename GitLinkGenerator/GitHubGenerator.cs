namespace GitLinkGenerator;

public static class GitHubGenerator
{
    public static bool IsGitHub(string repository) {
        return new Uri(repository).Host == "github.com";
    }
    public static string GitHubGeneratePath(string filePath, string projectUrl, string branch, string repository)
    {
        if(branch.StartsWith("refs/remotes/origin/")) {
            branch = branch.Substring("refs/remotes/origin/".Length);
        }
        return  $"{projectUrl}/blob/{branch}/{filePath.Substring(repository.Length+1).Replace('\\','/')}";
    }
}

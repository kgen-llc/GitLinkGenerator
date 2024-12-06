using GitLinkGenerator;

namespace GitLinkGeneratorTest;

[TestClass]
public class GitHubGeneratorTests
{
    [TestMethod]
    public void IsGitHubTest()
    {
        // assert
        Assert.IsTrue(GitHubGenerator.IsGitHub("https://github.com"));
        Assert.IsTrue(GitHubGenerator.IsGitHub("http://github.com"));
        Assert.IsFalse(GitHubGenerator.IsGitHub("https://notgithub.com"));
    }
}
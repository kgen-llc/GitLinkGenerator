using GeneratorGitHubSample;

namespace GitLinkGeneratorTest;

[TestClass]
public class GeneratedCodeTests
{
    [TestMethod]
    public void ValidateGeneratedAssemblyProperty()
    {
        // assert
        Assert.AreEqual("https://github.com/kgen-llc/GitLinkGenerator/blob/dev/GeneratorGitHubSample/Class1.cs", Class1.GitPath);
    }
}
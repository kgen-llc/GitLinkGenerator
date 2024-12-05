namespace GeneratorGitHubSample;

[AttributeUsage(AttributeTargets.Class)]
public class GitPathAttribute : Attribute {}

[GitPath]
public partial class Class1
{

}

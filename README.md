# GitLinkGenerator

Generate a static field representing the hyperlink to the related github content.

For the following class :
```csharp
[GitPathGenerator.GitPath]
public partial class Class1
{

}
```

an static const  ```GitPath``` is added and can used.
For example:
```csharp
Assert.AreEqual("https://github.com/kgen-llc/GitLinkGenerator/blob/dev/GeneratorGitHubSample/Class1.cs", Class1.GitPath);
```

in order to work, csproj MUST contains PackageProjectUrl property:
```xml
<PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <PackageProjectUrl>https://github.com/kgen-llc/GitLinkGenerator</PackageProjectUrl>
</PropertyGroup>
```

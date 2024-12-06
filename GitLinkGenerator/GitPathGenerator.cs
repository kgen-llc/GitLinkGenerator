namespace GitLinkGenerator;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;

[Generator]
public class GitPathGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(static postInitializationContext => {
            postInitializationContext.AddSource("GitPathGenerator.generated.cs", SourceText.From("""
                using System;

                namespace GitPathGenerator
                {
                    [AttributeUsage(AttributeTargets.Class)]
                    internal sealed class GitPathAttribute : Attribute
                    {
                    }
                }
                """, Encoding.UTF8));
        });

        // control logging via analyzerconfig
        var gitInfo = context.AnalyzerConfigOptionsProvider.
            Select ( extraGitInfo);

       var pipeline = context.SyntaxProvider.ForAttributeWithMetadataName(
            fullyQualifiedMetadataName: "GitPathGenerator.GitPathAttribute",
            predicate: static (syntaxNode, cancellationToken) => syntaxNode is ClassDeclarationSyntax,
            transform: static (context, cancellationToken) =>
            {
                return new Model(
                    // Note: this is a simplified example. You will also need to handle the case where the type is in a global namespace, nested, etc.
                    Namespace: context.TargetSymbol.ContainingNamespace?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted)),
                    ClassName: context.TargetSymbol.Name,
                    FilePath: context.TargetNode.SyntaxTree.FilePath);
            }
        );

        context.RegisterSourceOutput(pipeline.Combine(gitInfo), static (context, args) =>
        {
            var gitInfo = args.Right;
            var model = args.Left;
                string? filePath = null;

                if(GitHubGenerator.IsGitHub(gitInfo.Value.projectUrl))
                {
                    filePath = GitHubGenerator.GitHubGeneratePath(model.FilePath, gitInfo.Value.projectUrl, gitInfo.Value.branch, gitInfo.Value.repository);
                }

                if(filePath != null) {
                    var source = $@"
                        namespace {model.Namespace}
                        {{
                            public partial class {model.ClassName}
                            {{
                                public const string GitPath = ""{filePath}"";
                            }}
                        }}";

                context.AddSource($"{model.ClassName}_GitPath.cs", SourceText.From(source, Encoding.UTF8));
                }
        });
    }

    private record Model(string? Namespace, string ClassName, string FilePath);

    private static (string repository, string branch, string projectUrl)? extraGitInfo(AnalyzerConfigOptionsProvider provider, CancellationToken token)
    {
        if(provider.GlobalOptions.TryGetValue("build_property.GitRootDirectory", out var repository)
        && provider.GlobalOptions.TryGetValue("build_property.GitBranch", out var branch)
        && provider.GlobalOptions.TryGetValue("build_property.PackageProjectUrl", out var projectUrl))
        {
            return (repository, branch, projectUrl);
        }

        return null;
    }
}
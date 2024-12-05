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
        // control logging via analyzerconfig
        var gitInfo = context.AnalyzerConfigOptionsProvider.
            Select ( extraGitInfo);

        var classDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => IsClassWithAttributes(s),
                transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx))
            .Where(static m => m is not null);

        var compilationAndClasses = context.CompilationProvider.Combine(classDeclarations.Collect());

        context.RegisterSourceOutput(compilationAndClasses.Combine(gitInfo), static (spc, source) => Execute(source.Left.Left, source.Left.Right, source.Right, spc));
    }
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

    private static bool IsClassWithAttributes(SyntaxNode node)
    {
        return node is ClassDeclarationSyntax classDeclarationSyntax && classDeclarationSyntax.AttributeLists.Count > 0;
    }

    private static ClassDeclarationSyntax GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
    {
        var classDeclarationSyntax = (ClassDeclarationSyntax)context.Node;
        return classDeclarationSyntax;
    }

    private static void Execute(Compilation compilation, ImmutableArray<ClassDeclarationSyntax> classes, (string repository,string branch, string projectUrl)? gitInfo, SourceProductionContext context)
    {
        if (classes.IsDefaultOrEmpty)
            return;

        var attributeSymbol = compilation.GetTypeByMetadataName("GeneratorGitHubSample.GitPathAttribute");
        if (attributeSymbol == null)
            return;

        foreach (var classDeclaration in classes)
        {
            var model = compilation.GetSemanticModel(classDeclaration.SyntaxTree);
            var classSymbol = model.GetDeclaredSymbol(classDeclaration);

            var attributeData = classSymbol.GetAttributes().FirstOrDefault(ad => ad.AttributeClass.Equals(attributeSymbol, SymbolEqualityComparer.Default));
            if (attributeData != null)
            {
                var filePath = classDeclaration.SyntaxTree.FilePath.Substring(gitInfo.Value.repository.Length+1);
                var source = $@"
namespace {classSymbol.ContainingNamespace.ToDisplayString()}
{{
    public partial class {classSymbol.Name}
    {{
        public const string GitPath = ""{gitInfo.Value.projectUrl}/{gitInfo.Value.branch}/{filePath}"";
    }}
}}";

                context.AddSource($"{classSymbol.Name}_GitPath.cs", SourceText.From(source, Encoding.UTF8));
            }
        }
    }
}

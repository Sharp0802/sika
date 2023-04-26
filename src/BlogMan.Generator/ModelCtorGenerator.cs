using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace BlogMan.Generator;

[Generator]
public class ModelCtorGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
    }

    public void Execute(GeneratorExecutionContext context)
    {
        foreach (var nts in context.Compilation.SyntaxTrees
                                   .Select(tree => tree.GetRoot())
                                   .SelectMany(root => root.DescendantNodes())
                                   .Select(node => context.Compilation
                                                          .GetSemanticModel(node.SyntaxTree)
                                                          .GetSymbolInfo(node).Symbol)
                                   .OfType<INamedTypeSymbol>()
                                   .Where(symbol => symbol.ContainingNamespace
                                                          .ToDisplayString()
                                                          .Equals("BlogMan.Models", StringComparison.Ordinal))
                                   .Distinct(SymbolEqualityComparer.Default)
                                   .OfType<INamedTypeSymbol>()
                )
        {
            var src =
                $$"""
namespace {{nts.ContainingNamespace.ToDisplayString()}};

public partial class {{nts.Name}}
{
    [Obsolete]
    public {{nts.Name}}() {}
}
""";
            context.AddSource($"{nts.Name}.g.cs", SourceText.From(src, Encoding.UTF8));
        }
    }
}
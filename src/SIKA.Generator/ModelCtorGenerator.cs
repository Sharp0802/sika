//    Copyright (C) 2023  Yeong-won Seo
// 
//     SIKA is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
// 
//     SIKA is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
// 
//     You should have received a copy of the GNU General Public License
//     along with SIKA.  If not, see <https://www.gnu.org/licenses/>.

using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace SIKA.Generator;

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
                                                          .Equals("SIKA.Models", StringComparison.Ordinal))
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
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Globalization;
using System.Linq;

namespace StateMachineSourceGenerator
{
    [Generator]
    class OpenInEditorGenerator : IIncrementalGenerator
    {
        private static readonly DiagnosticDescriptor MissingPartialDiagnostic = new(
            id: "FriendryStates001",
            title: "Missing 'partial' modifier",
            messageFormat: CultureInfo.CurrentCulture.TwoLetterISOLanguageName == "ja" ?
                "'{0}'クラスはpartial指定されていないため、ステートマシンエディタの一部機能を利用できません。" :
                "The class '{0}' is not marked as partial and some features of the state machine editor may not work.",
            category: "Friendry States",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true
        );

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var source = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => node is TypeDeclarationSyntax,
                transform: static (ctx, _) =>
                {
                    var decl = (TypeDeclarationSyntax)ctx.Node;
                    var symbol = ctx.SemanticModel.GetDeclaredSymbol(decl)!;
                    return (Node: decl, Symbol: symbol);
                }
            )
            .Where(static ctx => ctx.Symbol.AllInterfaces.Any(i => i.ToDisplayString() is "FriendSea.StateMachine.IBehaviour" or "FriendSea.StateMachine.Controls.Transition.ICondition"));

            context.RegisterSourceOutput(source, (productionContext, syntaxContext) =>
            {
                if (!syntaxContext.Node.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)))
                {
                    productionContext.ReportDiagnostic(Diagnostic.Create(
                        MissingPartialDiagnostic,
                        syntaxContext.Node.Identifier.GetLocation(),
                        syntaxContext.Symbol.Name
                    ));
                    return;
                }

                var outerClass = syntaxContext.Symbol.ContainingType?.Name;
                var code = $$"""
				{{(syntaxContext.Symbol.ContainingNamespace.IsGlobalNamespace ? "" : $"namespace {syntaxContext.Symbol.ContainingNamespace.ToDisplayString()} {{")}}
				{{(outerClass == null ? null : $"partial class {outerClass} {{")}}
				partial class {{syntaxContext.Symbol.Name}} {
				#if UNITY_EDITOR
				#pragma warning disable CS0414
				    static string sourcePathForEditor = @"{{syntaxContext.Node.SyntaxTree.FilePath}}";
				    static int sourceLineForEditor = {{syntaxContext.Node.GetLocation().GetLineSpan().StartLinePosition.Line}};
				#endif
				}
				{{(outerClass == null ? null : "}")}}
				{{(syntaxContext.Symbol.ContainingNamespace.IsGlobalNamespace ? "" : "}")}}
				""";
                productionContext.AddSource($"{syntaxContext.Symbol.ToDisplayString()}.Editor.g.cs", code);
            });
        }
    }
}

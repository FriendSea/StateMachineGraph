using System;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace StateMachineSourceGenerator
{
	[Generator]
	public class StateMachineSourceGenerator : IIncrementalGenerator
	{
		static string GetFullName(ISymbol symbol) => string.IsNullOrEmpty(symbol?.ContainingNamespace?.Name) ?
			symbol.Name :
			$"{GetFullName(symbol.ContainingNamespace)}.{symbol.Name}";
		public void Initialize(IncrementalGeneratorInitializationContext context)
		{
			context.RegisterPostInitializationOutput(static context =>
			{
				context.AddSource("InjectContextAttribute.g.cs", """
					using System;
					namespace FriendSea.StateMachine {
						[AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
						sealed class InjectContextAttribute : Attribute {
							public InjectContextAttribute(){}
						}
					}
					""");
			});

			var provider = context.SyntaxProvider.CreateSyntaxProvider(static (node, token) =>
			{
				return node is ClassDeclarationSyntax;
			},
			static (context, token) => context);

			context.RegisterSourceOutput(provider, (productionContext, syntaxContext) => {
				var symbol = Microsoft.CodeAnalysis.CSharp.CSharpExtensions.GetDeclaredSymbol(syntaxContext.SemanticModel, syntaxContext.Node as ClassDeclarationSyntax);
				if(!symbol.AllInterfaces.Any(i => GetFullName(i) == "FriendSea.StateMachine.IInjectable")) return;
				var members = symbol.GetMembers().OfType<IFieldSymbol>();
				if (members.Count() == 0) return;
				var code = $$"""
				{{(symbol.ContainingNamespace.IsGlobalNamespace ? "" : $"namespace {GetFullName(symbol.ContainingNamespace)} {{")}}
				partial class {{symbol.Name}} {
					public void OnSetup(FriendSea.StateMachine.IContextContainer ctx){
						base.OnSetup(ctx);
						{{string.Join("\n", members.Select(s => $"{s.Name} = ctx.Get<{GetFullName(s.Type)}>();"))}}
					}
				}
				{{(symbol.ContainingNamespace.IsGlobalNamespace ? "" : "}")}}
				""";
				productionContext.AddSource($"{GetFullName(symbol)}.Inject.g.cs", code);
            });
		}
	}
}

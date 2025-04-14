using Microsoft.CodeAnalysis;
using System.Linq;
using System.Collections.Immutable;

namespace StateMachineSourceGenerator
{
	[Generator]
	public class StateMachineSourceGenerator : IIncrementalGenerator
	{
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

			var source = context.SyntaxProvider.ForAttributeWithMetadataName(
					"FriendSea.StateMachine.InjectContextAttribute",
					static (node, token) => true,
					static (context, _) => (IFieldSymbol)context.TargetSymbol
                ).Collect()
				.SelectMany((methods, _) =>
				{
					return methods
						.GroupBy(m => m.ContainingType, SymbolEqualityComparer.Default)
						.Select(group => (Type: group.Key, Fields: group.ToImmutableArray()))
						.ToImmutableArray();
				});

            context.RegisterSourceOutput(source, (productionContext, syntaxContext) =>
			{	
				var code = $$"""
				{{(syntaxContext.Type.ContainingNamespace.IsGlobalNamespace ? "" : $"namespace {syntaxContext.Type.ContainingNamespace.ToDisplayString()} {{")}}
				partial class {{syntaxContext.Type.Name}} : FriendSea.StateMachine.IInjectable {
					public void OnSetup(FriendSea.StateMachine.IContextContainer ctx){
						{{string.Join("\n\t\t", syntaxContext.Fields.Select(s => $"{s.Name} = ctx.Get<{s.Type.ToDisplayString()}>();"))}}
					}
				}
				{{(syntaxContext.Type.ContainingNamespace.IsGlobalNamespace ? "" : "}")}}
				""";
				productionContext.AddSource($"{syntaxContext.Type.ToDisplayString()}.Inject.g.cs", code);
            });
		}
	}
}

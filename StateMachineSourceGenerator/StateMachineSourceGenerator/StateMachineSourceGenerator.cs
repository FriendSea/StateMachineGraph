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
	public class StateMachineSourceGenerator : ISourceGenerator
	{
		private const string AttributeText = 
			"""
			using System;
			namespace FriendSea.StateMachine {
				[AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
				sealed class InjectContextAttribute : Attribute {
					public InjectContextAttribute(){}
				}
			}

			""";

		static string GetFullName(ISymbol symbol) => string.IsNullOrEmpty(symbol?.ContainingNamespace?.Name) ?
			symbol.Name :
			$"{GetFullName(symbol.ContainingNamespace)}.{symbol.Name}";

		public void Execute(GeneratorExecutionContext context)
		{
			try
			{
				var classDecs = (context.SyntaxReceiver as SyntexContextReciever).ClassDecs;

				var code = 
					$$"""
					/*
					Auto Generated : {{DateTime.Now}}
					*/

					""";
				code += AttributeText;
				foreach(var dec in classDecs)
				{
					var sementicModel = context.Compilation.GetSemanticModel(dec.SyntaxTree);
					var symbol = sementicModel.GetDeclaredSymbol(dec);

					if (GetFullName(symbol.BaseType) != "FriendSea.StateMachine.BehaviourBase") continue;

					var fields = new Dictionary<string, string>();

					foreach(var sym in symbol.GetMembers())
					{
						var fieldSymbol = sym as IFieldSymbol;
						if (fieldSymbol == null) continue;
						if (!fieldSymbol.GetAttributes().Any(s => s.AttributeClass.Name == "InjectContextAttribute")) continue;
						fields.Add(fieldSymbol.Name, GetFullName(fieldSymbol.Type));
					}

					if (fields.Count == 0) continue;

					var decl = 
						$$"""
						partial class {{symbol.Name}} {
							protected override void OnSetup(IContextContainer obj, int frameCount) {
								base.OnSetup(obj, frameCount);
								{{string.Join("\n", fields.Select(s => $"{s.Key} = obj.Get<{s.Value}>();"))}}
							}
						}
						""";
					if (!string.IsNullOrEmpty(symbol.ContainingNamespace?.Name))
						decl = 
							$$"""
							namespace {{GetFullName(symbol.ContainingNamespace)}} {
							{{decl}}
							}
							""";
					code += decl;
				}
				context.AddSource(
					hintName: "InjectContextAttribute.g.cs",
					sourceText: SourceText.From(code, Encoding.UTF8));
			}
			catch(Exception e)
			{
				context.AddSource(
					hintName: "InjectContextAttribute.g.cs",
					sourceText: SourceText.From(AttributeText + $"/* {e.Message}:{e.StackTrace} */", Encoding.UTF8));
			}
		}

		public void Initialize(GeneratorInitializationContext context) {
			context.RegisterForSyntaxNotifications(new SyntaxReceiverCreator(() => new SyntexContextReciever()));
		}

		class SyntexContextReciever : ISyntaxReceiver
		{
			public List<ClassDeclarationSyntax> ClassDecs { get; } = new List<ClassDeclarationSyntax>();

			public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
			{
				if (!(syntaxNode is ClassDeclarationSyntax)) return;
				ClassDecs.Add(syntaxNode as ClassDeclarationSyntax);
			}
		}
	}
}

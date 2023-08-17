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
@"
using System;
namespace FriendSea.StateMachine
{
	[AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
	sealed class InjectContextAttribute : Attribute
	{
		public InjectContextAttribute()
		{
		}
	}
}
";

		public void Execute(GeneratorExecutionContext context)
		{
			try
			{
				var classDecs = (context.SyntaxReceiver as SyntexContextReciever).ClassDecs;

				var code = AttributeText;
				foreach(var dec in classDecs)
				{
					var sementicModel = context.Compilation.GetSemanticModel(dec.SyntaxTree);
					code += $"/* {sementicModel.GetDeclaredSymbol(dec).Name} : {sementicModel.GetDeclaredSymbol(dec).BaseType.Name} */\n";
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

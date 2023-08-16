using System;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

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
			context.AddSource(
				hintName: "InjectContextAttribute.g.cs",
				sourceText: SourceText.From(AttributeText, Encoding.UTF8));
		}

		public void Initialize(GeneratorInitializationContext context) { }
	}
}

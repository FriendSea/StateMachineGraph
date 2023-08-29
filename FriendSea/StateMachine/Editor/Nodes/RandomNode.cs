using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

namespace FriendSea.StateMachine
{
	[System.Serializable]
	[DisplayName("Controls/Random")]
	public class RandomNode : IStateMachineNode
	{
		public ISerializableStateReference GenerateReferenceForImport(GraphViewData data, GraphViewData.Node node, Dictionary<string, NodeAsset> id2asset) =>
			new Controls.Random()
			{
				targets = node.GetConnectedNodes()
						.OrderBy(n => n.position.y)
						.Select(n => StateMachineImporter.GenerateTransition(data, n, id2asset)).ToArray(),
			};
	}

	public class RandomNodeInitializer : StateMachineNodeInitializerBase
	{
		public override Type TargetType => typeof(RandomNode);
		public override void Initialize(GraphNode node)
		{
			node.title = "Random";
			node.style.width = 150f;
			SetupInputPort(node);
			SetupOutputPort(node);
			InitializeInternal(node);
			node.mainContainer.style.backgroundColor = StateMavhineGraphSettings.GetColor(typeof(RandomNode));
			node.extensionContainer.Clear();
		}
	}
}

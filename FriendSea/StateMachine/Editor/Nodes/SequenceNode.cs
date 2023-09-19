using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using FriendSea.GraphViewSerializer;

namespace FriendSea.StateMachine
{
	[System.Serializable]
	[DisplayName("Controls/Sequence")]
	class SequenceNode : IStateMachineNode
	{
		public ISerializableStateReference GenerateReferenceForImport(GraphViewData data, GraphViewData.Node node, Dictionary<string, NodeAsset> id2asset) =>
			new Controls.Sequence()
			{
				targets = node.GetConnectedNodes()
						.OrderBy(n => n.position.y)
						.Select(n => StateMachineImporter.GenerateTransition(data, n, id2asset)).ToArray(),
			};
	}

	class SequenceNodeInitializer : StateMachineNodeInitializerBase
	{
		public override Type TargetType => typeof(SequenceNode);
		public override void Initialize(GraphNode node)
		{
			node.title = "Sequence";
			node.style.width = 150f;
			SetupInputPort(node);
			SetupOutputPort(node);
			InitializeInternal(node);
			node.mainContainer.style.backgroundColor = StateMachineGraphSettings.GetColor(typeof(SequenceNode));
			node.extensionContainer.Clear();
		}
	}
}

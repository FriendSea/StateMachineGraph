using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace FriendSea.StateMachine
{
	[System.Serializable]
	public class TransitionNode : IStateMachineNode
	{
		[SerializeReference]
		internal State.Transition.ICondition transition;
	}

	public class TransitionNodeInitializer : StateMachineNodeInitializerBase
	{
		public override Type TargetType => typeof(TransitionNode);
		public override void Initialize(GraphNode node)
		{
			node.title = "Transition";
			node.style.width = 150f;
			SetupInputPort(node);
			SetupOutputPort(node);
			InitializeInternal(node);
			node.mainContainer.style.backgroundColor = StateMavhineGraphSettings.GetColor(typeof(TransitionNode));
		}
	}

	public class TransitionNodeReferenceGenerator : StateMachineImporter.IStateReferenceGenerator
	{
		public Type Target => typeof(TransitionNode);
		public State.IStateReference Generate(GraphViewData data, GraphViewData.Node node, Dictionary<string, NodeAsset> id2asset) =>
			new State.Transition()
			{
				condition = (node.data as TransitionNode).transition,
				targets = node.GetConnectedNodes()
						.OrderBy(n => n.position.y)
						.Select(n => StateMachineImporter.GenerateTransition(data, n, id2asset)).ToArray(),
			};
	}
}

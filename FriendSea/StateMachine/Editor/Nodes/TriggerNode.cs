using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace FriendSea.StateMachine
{
	public class TriggerNode : IStateMachineNode
	{
		[SerializeField]
		TriggerLabel label;

		public State.IStateReference GenerateReferenceForImport(GraphViewData data, GraphViewData.Node node, Dictionary<string, NodeAsset> id2asset) =>
			new State.Trigger() {
				label = label,
				targets = node.GetConnectedNodes()
						.OrderBy(n => n.position.y)
						.Select(n => StateMachineImporter.GenerateTransition(data, n, id2asset)).ToArray(),
			};

		public class TriggerNodeInitializer : StateMachineNodeInitializerBase
		{
			public override Type TargetType => typeof(TriggerNode);
			public override void Initialize(GraphNode node)
			{
				node.title = "Trigger";
				node.style.width = 150f;
				SetupInputPort(node);
				SetupOutputPort(node);
				InitializeInternal(node);
				node.mainContainer.style.backgroundColor = StateMavhineGraphSettings.GetColor(typeof(TriggerNode));
			}
		}
	}
}

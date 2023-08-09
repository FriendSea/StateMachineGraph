using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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
			node.title = "Condition";
			node.style.width = 150f;
			SetupInputPort(node);
			SetupOutputPort(node);
			InitializeInternal(node);
			node.mainContainer.style.backgroundColor = StateMavhineGraphSettings.GetColor(typeof(TransitionNode));
		}
	}
}

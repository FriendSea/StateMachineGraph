using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace FriendSea.StateMachine
{
	[System.Serializable]
	public class StateNode : IStateMachineNode
	{
		[SerializeField, HideInInspector]
		internal string name;
		[SerializeReference]
		internal State.IBehaviour[] behaviours;
	}

	public class StateNodeInitializer : StateMachineNodeInitializerBase
	{
		public override Type TargetType => typeof(StateNode);
		public override void Initialize(GraphNode node)
		{
			node.SetupRenamableTitle("data.name");
			node.style.width = 300f;
			SetupInputPort(node);
			SetupOutputPort(node);
			InitializeInternal(node);
			node.mainContainer.style.backgroundColor = StateMavhineGraphSettings.GetColor(typeof(StateNode));
		}
	}
}

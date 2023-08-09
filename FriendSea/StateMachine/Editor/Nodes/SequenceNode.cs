using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace FriendSea.StateMachine
{
	[System.Serializable]
	public class SequenceNode : IStateMachineNode { }

	public class SequenceNodeInitializer : StateMachineNodeInitializerBase
	{
		public override Type TargetType => typeof(SequenceNode);
		public override void Initialize(GraphNode node)
		{
			node.title = "Sequence";
			node.style.width = 150f;
			SetupInputPort(node);
			SetupOutputPort(node);
			InitializeInternal(node);
			node.mainContainer.style.backgroundColor = StateMavhineGraphSettings.GetColor(typeof(SequenceNode));
			node.extensionContainer.Clear();
		}
	}
}

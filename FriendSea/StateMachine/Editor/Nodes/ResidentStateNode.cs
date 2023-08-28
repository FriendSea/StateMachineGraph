using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace FriendSea.StateMachine
{
	[System.Serializable]
	[DisplayName("States/Resident State")]
	public class ResidentStateNode : IStateMachineNode
	{
		[SerializeField, HideInInspector]
		internal string name;
		[SerializeReference]
		internal IBehaviour[] behaviours;

		public ISerializableStateReference GenerateReferenceForImport(GraphViewData data, GraphViewData.Node node, Dictionary<string, NodeAsset> id2asset) =>
			throw new NotImplementedException("ResidentState should not has inputs.");
	}

	public class ResidentStateNodeInitializer : StateMachineNodeInitializerBase
	{
		public override Type TargetType => typeof(ResidentStateNode);
		public override void Initialize(GraphNode node)
		{
			node.SetupRenamableTitle("data.name");
			node.style.width = 300f;
			SetupOutputPort(node);
			InitializeInternal(node);
			node.mainContainer.style.backgroundColor = StateMavhineGraphSettings.GetColor(typeof(ResidentStateNode));
		}
	}
}

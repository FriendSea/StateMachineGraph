using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor.Experimental.GraphView;

namespace FriendSea.StateMachine
{
	[System.Serializable]
	public class ComponentTransitionNode : IStateMachineNode
	{
		[SerializeField]
		internal StateMachineAsset asset;
	}

	public class ComponentTransitionNodeInitializer : GraphNode.IInitializer
	{
		public Type TargetType => typeof(ComponentTransitionNode);
		public void Initialize(GraphNode node)
		{
			node.title = "Component Transition";

			// add input port

			var inport = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(object));
			inport.userData = "enter";
			inport.portType = typeof(State.IStateReference);
			inport.portColor = Color.white;
			inport.portName = "";
			node.inputContainer.Add(inport);

			// force expanded
			node.titleButtonContainer.Clear();
			node.expanded = true;
			node.RefreshExpandedState();

			node.topContainer.Insert(1, node.titleContainer);

			node.mainContainer.style.backgroundColor = StateMavhineGraphSettings.GetColor(typeof(ComponentTransitionNode));
		}
	}

	public class ComponentTransitionNodeReferenceGenerator : StateMachineImporter.IStateReferenceGenerator
	{
		public Type Target => typeof(ComponentTransitionNode);
		public State.IStateReference Generate(GraphViewData data, GraphViewData.Node node, Dictionary<string, NodeAsset> id2asset) =>
			new ComponentTransition();
	}
}

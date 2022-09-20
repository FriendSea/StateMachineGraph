using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor.Experimental.GraphView;

namespace FriendSea.StateMachine
{
	public class EntryNode
	{
	}
	public class FallbackNode
	{
	}

	public class EntryNodeInitializer : GraphNode.IInitializer
	{
		public Type TargetType => typeof(EntryNode);

		public void Initialize(GraphNode node)
		{
			node.capabilities ^= Capabilities.Deletable | Capabilities.Copiable;
			node.mainContainer.style.backgroundColor = Color.blue;
			node.title = "Entry";
			var outp = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(object));
			outp.userData = "transitions";
			outp.portType = typeof(State.IStateReference);
			outp.portColor = Color.white;
			outp.portName = "";
			node.outputContainer.Add(outp);

			node.topContainer.Insert(1, node.titleContainer);
		}
	}

	public class FallbackNodeInitializer : GraphNode.IInitializer
	{
		public Type TargetType => typeof(FallbackNode);

		public void Initialize(GraphNode node)
		{
			node.capabilities ^= Capabilities.Deletable | Capabilities.Copiable;
			node.mainContainer.style.backgroundColor = Color.red;
			node.title = "Fallback";
			var outp = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(object));
			outp.userData = "transitions";
			outp.portType = typeof(State.IStateReference);
			outp.portColor = Color.white;
			outp.portName = "";
			node.outputContainer.Add(outp);

			node.topContainer.Insert(1, node.titleContainer);
		}
	}
}

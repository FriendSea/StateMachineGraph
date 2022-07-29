using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor.Experimental.GraphView;

namespace FriendSea
{
	public class StateMachineEntryNode
	{
	}
	public class StateMachineFallbackNode
	{
	}

	public class StateMachineEntryNodeInitializer : GraphNode.IInitializer
	{
		public Type TargetType => typeof(StateMachineEntryNode);

		public void Initialize(GraphNode node)
		{
			node.capabilities ^= Capabilities.Deletable | Capabilities.Copiable;
			node.mainContainer.style.backgroundColor = Color.blue;
			node.title = "Entry";
			var outp = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(object));
			outp.userData = "transitions";
			outp.portType = typeof(StateMachineStateNode);
			outp.portColor = new Color(1, 0.5f, 0);
			outp.portName = "";
			node.outputContainer.Add(outp);
		}
	}

	public class StateMachineFallbackNodeInitializer : GraphNode.IInitializer
	{
		public Type TargetType => typeof(StateMachineFallbackNode);

		public void Initialize(GraphNode node)
		{
			node.capabilities ^= Capabilities.Deletable | Capabilities.Copiable;
			node.mainContainer.style.backgroundColor = Color.red;
			node.title = "Fallback";
			var outp = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(object));
			outp.userData = "transitions";
			outp.portType = typeof(StateMachineStateNode);
			outp.portColor = new Color(1, 0.5f, 0);
			outp.portName = "";
			node.outputContainer.Add(outp);
		}
	}
}

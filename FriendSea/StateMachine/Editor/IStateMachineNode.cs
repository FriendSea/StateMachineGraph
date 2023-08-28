using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace FriendSea.StateMachine
{
	public interface IStateMachineNode {
		ISerializableStateReference GenerateReferenceForImport(GraphViewData data, GraphViewData.Node node, Dictionary<string, NodeAsset> id2asset);
	}

	public abstract class StateMachineNodeInitializerBase : GraphNode.IInitializer
	{
		public abstract Type TargetType { get; }
		public void SetupInputPort(GraphNode node)
		{
			var inport = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(object));
			inport.userData = "enter";
			inport.portType = typeof(ISerializableStateReference);
			inport.portColor = Color.white;
			inport.portName = "";
			node.inputContainer.Add(inport);
		}
		public void SetupOutputPort(GraphNode node)
		{
			var outport = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(object));
			outport.userData = "transition";
			outport.portType = typeof(ISerializableStateReference);
			outport.portColor = Color.white;
			outport.portName = "";
			node.outputContainer.Add(outport);
		}
		public void InitializeInternal(GraphNode node)
		{
			// add fields
			node.extensionContainer.style.overflow = Overflow.Hidden;
			node.extensionContainer.Add(new PropertyField(node.GetProperty().FindPropertyRelative("data")));

			// force expanded
			node.titleButtonContainer.Clear();
			node.expanded = true;
			node.RefreshExpandedState();

			node.topContainer.Insert(1, node.titleContainer);
		}

		public abstract void Initialize(GraphNode node);
	}
}

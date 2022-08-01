using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;

namespace FriendSea
{
	public interface IStateMachineNode {
	}

	[System.Serializable]
	public class StateMachineStateNode : IStateMachineNode
	{
		[SerializeField, HideInInspector]
		internal string name;
		[SerializeReference]
		internal StateMachineState.IBehaviour[] behaviours;
	}

	[System.Serializable]
	public class StateMachineTransitionNode : IStateMachineNode
	{
		[SerializeReference]
		internal StateMachineState.Transition.ICondition transition;
	}

	public abstract class StateMachineNodeInitializerBase : GraphNode.IInitializer
	{
		public abstract Type TargetType { get; }
		public void InitializeInternal(GraphNode node)
		{
			var nodeTypes = new Dictionary<System.Type, Color> {
					{ typeof(StateMachineStateNode), new Color(1, 0.5f, 0) },
					{ typeof(StateMachineTransitionNode), Color.green },
					{ typeof(StateMachineState.IStateReference), Color.white },
				};

			// add output port

			var outport = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(object));
			outport.userData = "transition";
			outport.portType = typeof(StateMachineState.IStateReference);
			outport.portColor = Color.white;
			outport.portName = "";
			node.outputContainer.Add(outport);

			// add input port

			var inport = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(object));
			inport.userData = "enter";
			inport.portType = typeof(StateMachineState.IStateReference);
			inport.portColor = Color.white;
			inport.portName = "";
			node.inputContainer.Add(inport);

			// add fields

			node.extensionContainer.style.overflow = Overflow.Hidden;
			node.extensionContainer.Add(new IMGUIContainer(() =>
			{
				node.GetProperty().serializedObject.Update();
				var prop = node.GetProperty().FindPropertyRelative("data");
				EditorGUILayout.PropertyField(prop, new GUIContent(prop.managedReferenceFullTypename), true);
				node.GetProperty().serializedObject.ApplyModifiedProperties();
			}));

			// force expanded
			node.titleButtonContainer.Clear();
			node.expanded = true;
			node.RefreshExpandedState();

			node.topContainer.Insert(1, node.titleContainer);
		}

		public abstract void Initialize(GraphNode node);
	}

	public class StateMachineStateNodeInitializer : StateMachineNodeInitializerBase
	{
		public override Type TargetType => typeof(StateMachineStateNode);
		public override void Initialize(GraphNode node)
		{
			node.SetupRenamableTitle("data.name");
			InitializeInternal(node);
			node.mainContainer.style.backgroundColor = new Color(1, 0.5f, 0) / 2f;
		}
	}

	public class StateMachineTransitionNodeInitializer : StateMachineNodeInitializerBase
	{
		public override Type TargetType => typeof(StateMachineTransitionNode);
		public override void Initialize(GraphNode node)
		{
			node.title = "Transition";
			node.style.width = 150f;
			InitializeInternal(node);
			node.mainContainer.style.backgroundColor = Color.green / 2f;
		}
	}
}

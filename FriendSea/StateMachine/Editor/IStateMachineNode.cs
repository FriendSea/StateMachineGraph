using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;

namespace FriendSea.StateMachine
{
	public interface IStateMachineNode {
	}

	[System.Serializable]
	public class StateNode : IStateMachineNode
	{
		[SerializeField, HideInInspector]
		internal string name;
		[SerializeReference]
		internal State.IBehaviour[] behaviours;
	}

	[System.Serializable]
	public class TransitionNode : IStateMachineNode
	{
		[SerializeReference]
		internal State.Transition.ICondition transition;
	}

	[System.Serializable]
	public class StateMachineReferenceNode : IStateMachineNode
	{
		[SerializeField]
		internal StateMachineAsset asset;
	}

	[System.Serializable]
	public class ComponentTransitionNode : IStateMachineNode
	{
		[SerializeField]
		internal StateMachineAsset asset;
	}

	public abstract class StateMachineNodeInitializerBase : GraphNode.IInitializer
	{
		public abstract Type TargetType { get; }
		public void InitializeInternal(GraphNode node)
		{
			var nodeTypes = new Dictionary<System.Type, Color> {
					{ typeof(StateNode), new Color(1, 0.5f, 0) },
					{ typeof(TransitionNode), Color.green },
					{ typeof(State.IStateReference), Color.white },
				};

			// add output port

			var outport = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(object));
			outport.userData = "transition";
			outport.portType = typeof(State.IStateReference);
			outport.portColor = Color.white;
			outport.portName = "";
			node.outputContainer.Add(outport);

			// add input port

			var inport = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(object));
			inport.userData = "enter";
			inport.portType = typeof(State.IStateReference);
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

	public class StateNodeInitializer : StateMachineNodeInitializerBase
	{
		public override Type TargetType => typeof(StateNode);
		public override void Initialize(GraphNode node)
		{
			node.SetupRenamableTitle("data.name");
			InitializeInternal(node);
			node.mainContainer.style.backgroundColor = new Color(1, 0.5f, 0) / 2f;
		}
	}

	public class TransitionNodeInitializer : StateMachineNodeInitializerBase
	{
		public override Type TargetType => typeof(TransitionNode);
		public override void Initialize(GraphNode node)
		{
			node.title = "Transition";
			node.style.width = 150f;
			InitializeInternal(node);
			node.mainContainer.style.backgroundColor = Color.green / 2f;
		}
	}

	public class StateMachineReferenceNodeInitializer : GraphNode.IInitializer
	{
		public Type TargetType => typeof(StateMachineReferenceNode);
		public void Initialize(GraphNode node)
		{
			node.title = "StateMachine Reference";

			// add input port

			var inport = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(object));
			inport.userData = "enter";
			inport.portType = typeof(State.IStateReference);
			inport.portColor = Color.white;
			inport.portName = "";
			node.inputContainer.Add(inport);

			// add fields

			node.extensionContainer.style.overflow = Overflow.Hidden;
			node.extensionContainer.Add(new IMGUIContainer(() =>
			{
				node.GetProperty().serializedObject.Update();
				var prop = node.GetProperty().FindPropertyRelative("data").FindPropertyRelative("asset");
				EditorGUILayout.PropertyField(prop, true);
				node.GetProperty().serializedObject.ApplyModifiedProperties();
			}));

			// force expanded
			node.titleButtonContainer.Clear();
			node.expanded = true;
			node.RefreshExpandedState();

			node.topContainer.Insert(1, node.titleContainer);

			node.mainContainer.style.backgroundColor = Color.blue / 2f;
		}
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

			node.mainContainer.style.backgroundColor = Color.blue / 2f;
		}
	}
}

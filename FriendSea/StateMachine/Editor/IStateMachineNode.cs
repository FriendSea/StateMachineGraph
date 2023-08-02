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
	public class SequenceNode : IStateMachineNode
	{
	}

	[System.Serializable]
	public class ResidentStateNode : IStateMachineNode
	{
		[SerializeField, HideInInspector]
		internal string name;
		[SerializeReference]
		internal State.IBehaviour[] behaviours;
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
		public void SetupInputPort(GraphNode node)
		{
			var inport = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(object));
			inport.userData = "enter";
			inport.portType = typeof(State.IStateReference);
			inport.portColor = Color.white;
			inport.portName = "";
			node.inputContainer.Add(inport);
		}
		public void SetupOutputPort(GraphNode node)
		{
			var outport = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(object));
			outport.userData = "transition";
			outport.portType = typeof(State.IStateReference);
			outport.portColor = Color.white;
			outport.portName = "";
			node.outputContainer.Add(outport);
		}
		public void InitializeInternal(GraphNode node)
		{
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
			node.style.width = 300f;
			SetupInputPort(node);
			SetupOutputPort(node);
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
			SetupInputPort(node);
			SetupOutputPort(node);
			InitializeInternal(node);
			node.mainContainer.style.backgroundColor = Color.green / 2f;
		}
	}

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
			node.mainContainer.style.backgroundColor = Color.black / 2f;
			node.extensionContainer.Clear();
		}
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
			node.mainContainer.style.backgroundColor = Color.yellow / 2f;
		}
	}

	public class StateMachineReferenceNodeInitializer : GraphNode.IInitializer
	{
		public Type TargetType => typeof(StateMachineReferenceNode);
		public void Initialize(GraphNode node)
		{
			node.title = node?.GetProperty()?.FindPropertyRelative("data")?.FindPropertyRelative("asset")?.objectReferenceValue?.name ?? "StateMachine Reference";

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
				var before = prop.objectReferenceValue;
				EditorGUILayout.PropertyField(prop, GUIContent.none, true);
				var changed = prop.objectReferenceValue;
				node.GetProperty().serializedObject.ApplyModifiedProperties();
				if (before != changed)
					node.title = changed.name;
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

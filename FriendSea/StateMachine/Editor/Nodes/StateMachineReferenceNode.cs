using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEditor;
using UnityEngine.UIElements;
using FriendSea.GraphViewSerializer;

namespace FriendSea.StateMachine
{
	[System.Serializable]
	[DisplayName("States/StateMachine Reference")]
	class StateMachineReferenceNode : IStateMachineNode
	{
		[SerializeField]
		internal StateMachineAsset asset;

		public ISerializableStateReference GenerateReferenceForImport(GraphViewData data, GraphViewData.Node node, Dictionary<string, NodeAsset> id2asset) =>
			(node.data as StateMachineReferenceNode).asset?.entryState;
	}

	class StateMachineReferenceNodeDropHandler : IStateMachineDropHandler
	{
		public Type TargetType => typeof(StateMachineAsset);

		public object CreateNodeData(UnityEngine.Object obj)
		{
			var asset = obj as StateMachineAsset;
			return new StateMachineReferenceNode(){
				asset = asset,
			};
		}
	}

	class StateMachineReferenceNodeInitializer : GraphNode.IInitializer
	{
		public Type TargetType => typeof(StateMachineReferenceNode);
		public void Initialize(GraphNode node)
		{
			node.title = node?.GetProperty()?.FindPropertyRelative("data")?.FindPropertyRelative("asset")?.objectReferenceValue?.name ?? "StateMachine Reference";

			// add input port

			var inport = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(object));
			inport.userData = "enter";
			inport.portType = typeof(ISerializableStateReference);
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

			node.mainContainer.style.backgroundColor = StateMachineGraphSettings.GetColor(typeof(StateMachineReferenceNode));
		}
	}
}

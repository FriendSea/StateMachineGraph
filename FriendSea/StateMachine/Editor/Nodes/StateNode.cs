using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace FriendSea.StateMachine
{
	[System.Serializable]
	public class StateNode : IStateMachineNode
	{
		[SerializeField, HideInInspector]
		internal string name;
		[SerializeReference]
		internal State.IBehaviour[] behaviours;

		public State.IStateReference GenerateReferenceForImport(GraphViewData data, GraphViewData.Node node, Dictionary<string, NodeAsset> id2asset) =>
			new State.StateReference() { nodeAsset = id2asset[node.id.id] };
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
			node.mainContainer.style.backgroundColor = StateMavhineGraphSettings.GetColor(typeof(StateNode));
		}
	}

	[CustomPropertyDrawer(typeof(StateNode))]
	[CustomPropertyDrawer(typeof(ResidentStateNode))]
	class StateDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			position.y -= EditorGUIUtility.singleLineHeight;
			position.x -= 30f;
			position.width += 30f;
			var listProp = property.FindPropertyRelative("behaviours");
			listProp.isExpanded = true;
			EditorGUI.PropertyField(position, listProp, null, true);
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			var listProp = property.FindPropertyRelative("behaviours");
			listProp.isExpanded = true;
			return EditorGUI.GetPropertyHeight(listProp, null, true) - EditorGUIUtility.singleLineHeight;
		}
	}
}

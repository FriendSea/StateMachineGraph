using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditorInternal;

namespace FriendSea.StateMachine
{
	[System.Serializable]
	[DisplayName("State")]
	public class StateNode : IStateMachineNode
	{
		[SerializeField, HideInInspector]
		internal string name;
		[SerializeReference]
		internal IBehaviour[] behaviours;

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
	class StateNodeDrawer : PropertyDrawer
	{
		Dictionary<SerializedObject, Dictionary<string, ReorderableList>> lists = new Dictionary<SerializedObject, Dictionary<string, ReorderableList>>();

		ReorderableList CreateReorderableList(SerializedProperty property)
		{
			var prop = property.FindPropertyRelative("behaviours");
			var list = new ReorderableList(property.serializedObject, prop);
			list.drawElementCallback += (Rect rect, int index, bool isActive, bool isFocused) =>
				EditorGUI.PropertyField(rect, prop.GetArrayElementAtIndex(index));
			list.elementHeightCallback += index => EditorGUI.GetPropertyHeight(prop.GetArrayElementAtIndex(index));
			list.headerHeight = 0;
			return list;
		}

		ReorderableList GetReorderableList(SerializedProperty property)
		{
			if (!lists.ContainsKey(property.serializedObject))
				lists.Add(property.serializedObject, new Dictionary<string, ReorderableList>());
			var dict = lists[property.serializedObject];
			if (!dict.ContainsKey(property.propertyPath))
				dict.Add(property.propertyPath, CreateReorderableList(property));
			return dict[property.propertyPath];
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) =>
			GetReorderableList(property).DoList(position);

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) =>
			GetReorderableList(property).GetHeight();
	}

	[CustomPropertyDrawer(typeof(State))]
	class StateDrawer : StateNodeDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			var height = base.GetPropertyHeight(property, label);
			var conditionHeight = position.height - height;
			position.height = height;

			EditorGUI.DrawRect(position, StateMavhineGraphSettings.GetColor(typeof(StateNode)));
			base.OnGUI(ShrinkRect(position, 5f), property, label);

			position.y += height;
			position.height = conditionHeight;

			EditorGUI.DrawRect(position, StateMavhineGraphSettings.GetColor(typeof(TransitionNode)));
			EditorGUI.PropertyField(ShrinkRect(position, 5f), property.FindPropertyRelative("transition.condition"));
		}

		Rect ShrinkRect(Rect rect, float amount)
		{
			rect.x += amount;
			rect.width -= amount * 2f;
			return rect;
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return
				base.GetPropertyHeight(property, label) +
				EditorGUI.GetPropertyHeight(property.FindPropertyRelative("transition.condition"));
		}
	}
}

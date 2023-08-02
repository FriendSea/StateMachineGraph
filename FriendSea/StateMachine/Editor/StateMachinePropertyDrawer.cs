using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Linq;

namespace FriendSea.StateMachine
{
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

	[CustomPropertyDrawer(typeof(TransitionNode))]
	class TransitionDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			position.x -= 10f;
			position.width += 10f;
			var listProp = property.FindPropertyRelative("transition");
			listProp.isExpanded = true;
			EditorGUI.PropertyField(position, listProp, null, true);
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			var listProp = property.FindPropertyRelative("transition");
			listProp.isExpanded = true;
			return EditorGUI.GetPropertyHeight(listProp, null, true);
		}
	}

	[CustomPropertyDrawer(typeof(State.IBehaviour), true)]
	class StateBehaviourDrawer : SubclassDrawerDrawer<State.IBehaviour> { }

	[CustomPropertyDrawer(typeof(State.Transition.ICondition), true)]
	class StateTransitionDrawer : SubclassDrawerDrawer<State.Transition.ICondition> { }

	class SubclassDrawerDrawer<T> : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			var dropPos = position;
			dropPos.x += EditorGUIUtility.singleLineHeight;
			dropPos.width -= EditorGUIUtility.singleLineHeight;
			dropPos.height = EditorGUIUtility.singleLineHeight;
			var types = EditorUtils.GetSubClasses(typeof(T));
			var currentIndex = types.IndexOf(property.managedReferenceValue?.GetType());
			var newIndex = EditorGUI.Popup(dropPos, currentIndex, types.Select(t => t.Name).ToArray());
			if (newIndex != currentIndex)
			{
				property.managedReferenceValue = System.Activator.CreateInstance(types[newIndex]);
			}

			var original = EditorGUIUtility.labelWidth;
			EditorGUIUtility.labelWidth = 50;
			EditorGUI.PropertyField(position, property, new GUIContent(""), true);
			EditorGUIUtility.labelWidth = original;
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) =>
			EditorGUI.GetPropertyHeight(property, true);
	}
}

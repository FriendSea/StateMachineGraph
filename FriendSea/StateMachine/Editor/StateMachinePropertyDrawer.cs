using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Linq;

namespace FriendSea
{
	[CustomPropertyDrawer(typeof(StateMachineStateNode))]
	class StateDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			position.y -= EditorGUIUtility.singleLineHeight;
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

	[CustomPropertyDrawer(typeof(StateMachineState.IBehaviour), true)]
	class StateBehaviourDrawer : SubclassDrawerDrawer<StateMachineState.IBehaviour> { }

	[CustomPropertyDrawer(typeof(StateMachineState.ITransition), true)]
	class StateTransitionDrawer : SubclassDrawerDrawer<StateMachineState.ITransition> { }

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

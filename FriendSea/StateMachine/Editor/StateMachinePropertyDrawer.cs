using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Linq;

namespace FriendSea.StateMachine
{
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
			var newIndex = EditorGUI.Popup(dropPos, currentIndex, types.Select(t => t.GetDisplayName()).ToArray());
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

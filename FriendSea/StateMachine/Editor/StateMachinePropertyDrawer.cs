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
		static List<System.Type> types;
		static string[] typeNames;

		static GUIStyle openScriptStyle = new GUIStyle() { 
			padding = new RectOffset(2,2,2,2),
		};

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			if (types == null)
			{
				var dict = TypeCache.GetTypesDerivedFrom<T>()
					.Where(t => !t.IsAbstract)
					.Select(t => (t.GetDisplayName(), t))
					.Where(t => !t.Item1.StartsWith("Hidden/"))
					.ToDictionary(pair => pair.Item1, pair => pair.t);
				types = dict.Values.ToList();
				typeNames = dict.Keys.ToArray();
			}

			var dropPos = position;
			dropPos.x += EditorGUIUtility.singleLineHeight;
			dropPos.width -= EditorGUIUtility.singleLineHeight * 2f;
			dropPos.height = EditorGUIUtility.singleLineHeight;
			var currentIndex = types.IndexOf(property.managedReferenceValue?.GetType());
			var newIndex = EditorGUI.Popup(dropPos, currentIndex, typeNames);
			if (newIndex != currentIndex)
			{
				property.managedReferenceValue = System.Activator.CreateInstance(types[newIndex]);
			}

			dropPos.x += dropPos.width;
			dropPos.width = EditorGUIUtility.singleLineHeight;
			if(property.managedReferenceValue is InjectableObjectBase)
			{
				if (GUI.Button(dropPos, EditorGUIUtility.IconContent("cs Script Icon"), openScriptStyle))
				{
					var type = property.managedReferenceValue.GetType();
					var PathInfo = type.GetField("sourcePathForEditor", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
					var path = PathInfo.GetValue(null) as string;
					var LineInfo = type.GetField("sourceLineForEditor", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
					var line = (int)LineInfo.GetValue(null);
					InternalEditorUtility.OpenFileAtLineExternal(path, line);
				}
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

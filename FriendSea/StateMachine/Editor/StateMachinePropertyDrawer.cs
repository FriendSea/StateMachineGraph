using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Linq;
using FriendSea.GraphViewSerializer;
using System.IO;

namespace FriendSea.StateMachine
{
	[CustomPropertyDrawer(typeof(IBehaviour), true)]
	class StateBehaviourDrawer : SubclassDrawerDrawer<IBehaviour> { }

	[CustomPropertyDrawer(typeof(Controls.Transition.ICondition), true)]
	class StateTransitionDrawer : SubclassDrawerDrawer<Controls.Transition.ICondition> { }

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
			if (currentIndex == -1)
			{
				GUI.Label(dropPos, EditorGUIUtility.IconContent("Warning"));
				dropPos.x += EditorGUIUtility.singleLineHeight;
				GUI.Label(dropPos, "<Null>");
				dropPos.x -= EditorGUIUtility.singleLineHeight;
			}
			if (newIndex != currentIndex)
				property.managedReferenceValue = System.Activator.CreateInstance(types[newIndex]);

			dropPos.x += dropPos.width;
			dropPos.width = EditorGUIUtility.singleLineHeight;
			if (GUI.Button(dropPos, EditorGUIUtility.IconContent("_Menu"), openScriptStyle))
			{
				var menu = new GenericMenu();

				if (property.managedReferenceValue is IBehaviour)
					menu.AddItem(new GUIContent("Open in Editor"), false, () =>
					{
						var type = property.managedReferenceValue.GetType();
						var PathInfo = type.GetField("sourcePathForEditor", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
						var path = PathInfo.GetValue(null) as string;
						var LineInfo = type.GetField("sourceLineForEditor", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
						var line = (int)LineInfo.GetValue(null);
						InternalEditorUtility.OpenFileAtLineExternal(path, line);
					});
				else
					menu.AddDisabledItem(new GUIContent("Open in Editor"));

				if(property.managedReferenceValue != null)
					menu.AddItem(new GUIContent("Copy"), false, () => {
						var type = property.managedReferenceValue.GetType();
						GUIUtility.systemCopyBuffer = $"{type.Assembly.FullName}\n{type.FullName}\n{JsonUtility.ToJson(property.managedReferenceValue)}";
					});
				else
					menu.AddDisabledItem(new GUIContent("Copy"));

				var obj = ParseClipBoard(GUIUtility.systemCopyBuffer);
				if (obj != null)
					menu.AddItem(new GUIContent("Paste"), false, () =>
					{
						property.serializedObject.Update();
						property.managedReferenceValue = obj;
						property.serializedObject.ApplyModifiedProperties();
					});
				else
					menu.AddDisabledItem(new GUIContent("Paste"));

				menu.ShowAsContext();
			}

			var original = EditorGUIUtility.labelWidth;
			EditorGUIUtility.labelWidth = 50;
			EditorGUI.PropertyField(position, property, new GUIContent(""), true);
			EditorGUIUtility.labelWidth = original;
		}

		static object ParseClipBoard(string clipBoard)
		{
			var lines = clipBoard.Split("\n");
			if (lines.Length < 3) return null;
			if (!File.Exists(lines[0]))return null;
			var type = System.Reflection.Assembly.Load(lines[0])?.GetType(lines[1]);
			if (type == null) return null;
			return JsonUtility.FromJson(lines[2], type);
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) =>
			EditorGUI.GetPropertyHeight(property, true);
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;

namespace FriendSea.StateMachine.Behaviours {
	class VariablesContext {
		Dictionary<Int64, int> values = new Dictionary<Int64, int>();

		public int this[Int64 index] {
			get => values.ContainsKey(index) ?
					values[index]:
					default;
			set => values[index] = value;
		}
	}

	[DisplayName("Variables/Set")]
	class SetVariableValue : IBehaviour
	{
		[SerializeField, VariableId]
		Int64 variable;
		[SerializeField]
		int value;

		public void OnEnter(IContextContainer obj) =>
			obj.GetOrCreate<VariablesContext>()[variable] = value;

		public void OnExit(IContextContainer obj){}
		public void OnUpdate(IContextContainer obj){}
	}

	[DisplayName("Variables/Add")]
	class AddVariableValue : IBehaviour
	{
		[SerializeField, VariableId]
		Int64 variable;
		[SerializeField]
		int value;

		public void OnEnter(IContextContainer obj) =>
			obj.GetOrCreate<VariablesContext>()[variable] += value;

		public void OnExit(IContextContainer obj) { }
		public void OnUpdate(IContextContainer obj) { }
	}

	[AttributeUsage(System.AttributeTargets.Field, AllowMultiple = true)]
	class VariableIdAttribute : PropertyAttribute {}

#if UNITY_EDITOR
	[UnityEditor.CustomPropertyDrawer(typeof(VariableIdAttribute))]
	class VariableIdDrawer : UnityEditor.PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
			var ids = new List<Int64>();
			var names = new List<string>();
			var array = property.serializedObject.FindProperty("data.variables");
			for(int i=0;i < array.arraySize;i++){
				var prop = array.GetArrayElementAtIndex(i);
				ids.Add(prop.FindPropertyRelative("id").longValue);
				names.Add(prop.FindPropertyRelative("name").stringValue);
			}
			var newIndex = EditorGUI.Popup(position, label.text, ids.IndexOf(property.longValue), names.ToArray());
			if(newIndex >= 0)
				property.longValue = ids[newIndex];
        }
    }
#endif
}

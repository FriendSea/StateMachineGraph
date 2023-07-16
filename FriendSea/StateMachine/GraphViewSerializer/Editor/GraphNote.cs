using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace FriendSea
{
	[SerializableGraphView.TargetData(typeof(GraphViewData.StickyNote))]
	public class GraphNote : UnityEditor.Experimental.GraphView.StickyNote, SerializableGraphView.ISerializableElement, SerializableGraphView.IPositionableElement
	{
		public string id { get; private set; }

		public SerializedProperty parentProperty { get; private set; }

		public void Initialize(SerializedProperty property, SerializableGraphView graphView)
		{
			// initialize
			var path = property.propertyPath.Split('.');
			parentProperty = property.serializedObject.FindProperty(path[0]);
			for (int i = 1; i < path.Length - 1; i++)
				this.parentProperty = this.parentProperty.FindPropertyRelative(path[i]);
			id = property.FindPropertyRelative("id").FindPropertyRelative("id").stringValue;
			contents = property.FindPropertyRelative("content").stringValue;

			title = null;

			fontSize = UnityEditor.Experimental.GraphView.StickyNoteFontSize.Medium;
			SetPosition(new Rect(property.FindPropertyRelative("position").vector2Value, property.FindPropertyRelative("size").vector2Value));
			capabilities |= UnityEditor.Experimental.GraphView.Capabilities.Resizable;

			var field = this.Q("contents-field") as TextField;
			field.isDelayed = true;
			field.RegisterValueChangedCallback(e => {
				contents = e.newValue;
				parentProperty.serializedObject.Update();
				this.GetProperty().FindPropertyRelative("content").stringValue = e.newValue;
				parentProperty.serializedObject.ApplyModifiedProperties();
			});
		}

		public void UpdatePosition()
		{
			this.GetProperty().FindPropertyRelative("position").vector2Value = GetPosition().min;
		}

		public override void OnResized()
		{
			parentProperty.serializedObject.Update();
			this.GetProperty().FindPropertyRelative("size").vector2Value = GetPosition().size;
			parentProperty.serializedObject.ApplyModifiedProperties();
		}
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEditor;
using UnityEngine.UIElements;

namespace FriendSea
{
	[SerializableGraphView.TargetData(typeof(GraphViewData.Edge), 100)]
	public class GraphEdge : Edge, SerializableGraphView.ISerializableElement
	{
		public SerializedProperty parentProperty { get; private set; }
		public string id { get; private set; }

		public void Initialize(SerializedProperty property, SerializableGraphView graphView)
		{
			var path = property.propertyPath.Split('.');
			parentProperty = property.serializedObject.FindProperty(path[0]);
			for (int i = 1; i < path.Length - 1; i++)
				this.parentProperty = this.parentProperty.FindPropertyRelative(path[i]);
			id = property.FindPropertyRelative("id").FindPropertyRelative("id").stringValue;

			output = graphView.GetNode(property.FindPropertyRelative("outputNode").FindPropertyRelative("id").stringValue).GetOutputPort(property.FindPropertyRelative("outputPort").stringValue);
			input = graphView.GetNode(property.FindPropertyRelative("inputNode").FindPropertyRelative("id").stringValue).GetInputPort(property.FindPropertyRelative("inputPort").stringValue);
			output.Connect(this);
			input.Connect(this);
		}
	}
}

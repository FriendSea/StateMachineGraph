using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEditor;
using UnityEngine.UIElements;
using System.Linq;

namespace FriendSea.GraphViewSerializer
{
	[SerializableGraphView.TargetData(typeof(GraphViewData.Group))]
	public class GraphGroup : Group, SerializableGraphView.ISerializableElement, SerializableGraphView.IPositionableElement
	{
		public SerializedProperty parentProperty { get; private set; }
		public string id { get; private set; }

		bool initialized;

		public void Initialize(SerializedProperty property, SerializableGraphView graphView)
		{
			var path = property.propertyPath.Split('.');
			parentProperty = property.serializedObject.FindProperty(path[0]);
			for (int i = 1; i < path.Length - 1; i++)
				this.parentProperty = this.parentProperty.FindPropertyRelative(path[i]);
			id = property.FindPropertyRelative("id").FindPropertyRelative("id").stringValue;
			title = property.FindPropertyRelative("name").stringValue;

			var nodesProp = property.FindPropertyRelative("nodes");
			for (int i = 0; i < nodesProp.arraySize; i++)
				AddElement(graphView.GetNode(nodesProp.GetArrayElementAtIndex(i).FindPropertyRelative("id").stringValue));

			var text = headerContainer.Children().First().Children().FirstOrDefault(element => element is TextField) as TextField;
			text.RegisterCallback<FocusInEvent>(evt => { Input.imeCompositionMode = IMECompositionMode.On; });
			text.RegisterCallback<FocusOutEvent>(evt => { Input.imeCompositionMode = IMECompositionMode.Auto; });

			capabilities |= Capabilities.Renamable;
			this.AddManipulator(new ContextualMenuManipulator(e => graphView.BuildContextualMenu(e)));

			style.backgroundColor = new StyleColor(new Color(0,0,0,0.5f));

			initialized = true;
		}

		public void UpdatePosition()
		{
			var elements = new HashSet<GraphElement>();
			CollectElements(elements, element => element is GraphNode);
			foreach (var child in elements)
				(child as GraphNode)?.UpdatePosition();
		}

		protected override void OnElementsAdded(IEnumerable<GraphElement> elements)
		{
			if (!initialized) return;
			parentProperty.serializedObject.Update();
			var array = this.GetProperty().FindPropertyRelative("nodes");
			foreach (var element in elements)
			{
				if (!(element is GraphNode)) continue;
				array.InsertArrayElementAtIndex(0);
				array.GetArrayElementAtIndex(0).FindPropertyRelative("id").stringValue = (element as GraphNode).id;
			}
			parentProperty.serializedObject.ApplyModifiedProperties();
		}

		protected override void OnGroupRenamed(string oldName, string newName)
		{
			parentProperty.serializedObject.Update();
			this.GetProperty().FindPropertyRelative("name").stringValue = string.IsNullOrEmpty(newName) ? "Group" : newName;
			parentProperty.serializedObject.ApplyModifiedProperties();
		}
	}
}

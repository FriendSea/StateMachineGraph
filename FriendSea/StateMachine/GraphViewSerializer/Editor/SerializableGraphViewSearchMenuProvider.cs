using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEditor;
using System;
using UnityEngine.UIElements;

namespace FriendSea
{
	public class SerializableGraphViewSearchMenuProvider : ScriptableObject, ISearchWindowProvider
	{
		SerializableGraphView graphView;
		EditorWindow editorWindow;
		Type dataType;

		public void Initialize(SerializableGraphView graphView, EditorWindow editorWindow, System.Type dataType)
		{
			this.graphView = graphView;
			this.editorWindow = editorWindow;
			this.dataType = dataType;
		}

		public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
		{
			var entries = new List<SearchTreeEntry>();
			entries.Add(new SearchTreeGroupEntry(new GUIContent("Create Node")));

			foreach (var type in EditorUtils.GetSubClasses(dataType))
				entries.Add(new SearchTreeEntry(new GUIContent(type.Name)) { level = 1, userData = type });

			return entries;
		}

		public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
		{
			var type = searchTreeEntry.userData as Type;
			var worldMousePosition = editorWindow.rootVisualElement.ChangeCoordinatesTo(editorWindow.rootVisualElement.parent, context.screenMousePosition - editorWindow.position.position);
			var localMousePosition = graphView.contentViewContainer.WorldToLocal(worldMousePosition);
			graphView.AddNode(Activator.CreateInstance(type), localMousePosition);
			return true;
		}
	}
}

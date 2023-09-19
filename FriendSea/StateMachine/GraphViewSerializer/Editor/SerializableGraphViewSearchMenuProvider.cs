using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEditor;
using System;
using UnityEngine.UIElements;
using System.Linq;
using System.ComponentModel;

namespace FriendSea.GraphViewSerializer
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
			entries.AddRange(CreateEntries(EditorUtils.GetSubClasses(dataType).ToDictionary(t => t.GetDisplayName(), t => t), 1));
			return entries;
		}

		List<SearchTreeEntry> CreateEntries(Dictionary<string, System.Type> types, int level)
		{
			var result = new List<SearchTreeEntry>();
			foreach(var group in types.GroupBy(t => GetGroupName(t.Key)).OrderBy(g => g.Key)){
				if (string.IsNullOrEmpty(group.Key))
					result.AddRange(group.Select(pair => new SearchTreeEntry(new GUIContent(pair.Key)) { level = level, userData = pair.Value }));
				else
					result.AddRange(CreateEntries(group.ToDictionary(pair => pair.Key.Replace($"{group.Key}/", ""), pair => pair.Value), level + 1)
						.Prepend(new SearchTreeGroupEntry(new GUIContent(group.Key)) { level = level }));
			}
			return result;
		}

		string GetGroupName(string path) =>
			path.Contains('/') ?
				path.Split('/').First() :
				"";

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

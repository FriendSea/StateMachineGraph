using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.AssetImporters;
using System.Linq;
using System.IO;

namespace FriendSea
{
	[ScriptedImporter(0, "friendseastatemachine")]
	public class StateMachineImporter : ScriptedImporter
	{
		[SerializeField]
		GraphViewData<object> data = new GraphViewData<object>();

		public override void OnImportAsset(AssetImportContext ctx)
		{
			// initialize when created

			if (data.nodes.Count() <= 0)
			{
				data.nodes = new GraphViewData<object>.Node[] {
					new GraphViewData<object>.Node() { id = System.Guid.NewGuid().ToString(), data = new StateMachineEntryNode() },
					new GraphViewData<object>.Node() { id = System.Guid.NewGuid().ToString(), data = new StateMachineFallbackNode() },
				};
			}

			EditorUtility.SetDirty(this);

			// create asset

			var main = ScriptableObject.CreateInstance<StateMachineAsset>();
			ctx.AddObjectToAsset("main", main);
			ctx.SetMainObject(main);
			var mainEditor = Editor.CreateEditor(main);


			var assets = new Dictionary<string, Editor>();
			string entryId = null, fallbackId = null;
			foreach(var node in data.nodes)
			{
				if(node.data is StateMachineEntryNode)
				{
					entryId = node.id;
					continue;
				}
				if(node.data is StateMachineFallbackNode)
				{
					fallbackId = node.id;
					continue;
				}

				var state = ScriptableObject.CreateInstance<StateMachineNodeAsset>();
				state.name = node.data.GetType().Name;
				ctx.AddObjectToAsset(node.id, state);
				var editor = Editor.CreateEditor(state);
				editor.serializedObject.FindProperty("data").managedReferenceValue = node.data;
				editor.serializedObject.ApplyModifiedPropertiesWithoutUndo();
				assets.Add(node.id, editor);
			}

			// construct edges

			foreach(var edge in data.edges)
			{
				if(edge.outputNode == entryId)
				{
					mainEditor.serializedObject.FindProperty("entryState").objectReferenceValue = assets[edge.inputNode].target;
					continue;
				}
				if (edge.outputNode == fallbackId)
				{
					mainEditor.serializedObject.FindProperty("fallbackState").objectReferenceValue = assets[edge.inputNode].target;
					continue;
				}
				assets[edge.outputNode].serializedObject.Update();
				assets[edge.outputNode].serializedObject.FindProperty("data").FindPropertyRelative(edge.outputPort).objectReferenceValue = assets[edge.inputNode].target;
				assets[edge.outputNode].serializedObject.ApplyModifiedPropertiesWithoutUndo();
			}

			mainEditor.serializedObject.ApplyModifiedPropertiesWithoutUndo();
		}

		[MenuItem("Assets/Create/FriendSea/StateMachine")]
		static void CreateFile()
		{
			TryGetActiveFolderPath(out string path);

			File.CreateText(Path.Combine(path, "New StateMachine.friendseastatemachine")).Close();
			AssetDatabase.Refresh();
		}

		private static bool TryGetActiveFolderPath(out string path)
		{
			var _tryGetActiveFolderPath = typeof(ProjectWindowUtil).GetMethod("TryGetActiveFolderPath", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);

			object[] args = new object[] { null };
			bool found = (bool)_tryGetActiveFolderPath.Invoke(null, args);
			path = (string)args[0];

			return found;
		}
	}

	public class StateMachineEntryNode
	{
		[SerializeField]
		StateMachineNodeAsset entry;
	}
	public class StateMachineFallbackNode
	{
		[SerializeField]
		StateMachineNodeAsset fallback;
	}
}

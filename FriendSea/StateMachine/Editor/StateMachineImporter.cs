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
		internal GraphViewData data = new GraphViewData();

		public override void OnImportAsset(AssetImportContext ctx)
		{
			// initialize when created

			if (data.elements.Count() <= 0)
			{
				data.elements = new List<GraphViewData.ElementData> {
					new GraphViewData.Node() { id = new GraphViewData.Id(System.Guid.NewGuid().ToString()), data = new StateMachineEntryNode() },
					new GraphViewData.Node() { id = new GraphViewData.Id(System.Guid.NewGuid().ToString()), data = new StateMachineFallbackNode() },
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
			foreach (var node in data.elements.Where(e => e is GraphViewData.Node).Select(e => e as GraphViewData.Node))
			{
				if (node.data is StateMachineEntryNode)
				{
					entryId = node.id.id;
					continue;
				}
				if (node.data is StateMachineFallbackNode)
				{
					fallbackId = node.id.id;
					continue;
				}

				if (node.data is IStateMachineNode.Transition)
				{
					continue;
				}

				var state = ScriptableObject.CreateInstance<StateMachineNodeAsset>();
				state.name = (node.data as IStateMachineNode.State).name;
				ctx.AddObjectToAsset(node.id.id, state);
				var editor = Editor.CreateEditor(state);
				state.data = new StateMachineState() { behaviours = (node.data as IStateMachineNode.State).behaviours };
				assets.Add(node.id.id, editor);
			}

			// construct edges

			mainEditor.serializedObject.Update();

			foreach (var edge in data.elements.Where(e => e is GraphViewData.Edge).Select(e => e as GraphViewData.Edge))
			{
				if (edge.outputNode.id == entryId)
				{
					mainEditor.serializedObject.FindProperty("entryState").objectReferenceValue = assets[edge.inputNode.id].target;
					continue;
				}
				if (edge.outputNode.id == fallbackId)
				{
					mainEditor.serializedObject.FindProperty("fallbackState").objectReferenceValue = assets[edge.inputNode.id].target;
					continue;
				}
			}

			mainEditor.serializedObject.ApplyModifiedPropertiesWithoutUndo();

			foreach (var pair in assets)
			{
				var targets = GetConnectedNodes(pair.Key)
					.OrderBy(n => n.position.y)
					.Select(n =>
					{
						var id = GetConnectedNodes(n.id.id).FirstOrDefault()?.id.id ?? string.Empty;
						return new StateMachineState.Transition()
						{
							conditions = (n.data as IStateMachineNode.Transition).transitions,
							target = assets.ContainsKey(id) ?
								assets[id].target as StateMachineNodeAsset :
								null
						};
					});
				(pair.Value.target as StateMachineNodeAsset).data.transitions = targets.ToArray();
			}
		}

		IEnumerable<GraphViewData.Edge> GetConnectedEdges(string nodeId) =>
			data.elements.Where(e => e is GraphViewData.Edge).Select(e => e as GraphViewData.Edge).Where(e => e.outputNode.id == nodeId);

		GraphViewData.Node GetConnectedNode(GraphViewData.Edge edge) =>
			data.elements.Where(e => e is GraphViewData.Node).Select(e => e as GraphViewData.Node).Where(n => n.id == edge.inputNode).FirstOrDefault();

		IEnumerable<GraphViewData.Node> GetConnectedNodes(string nodeId) =>
			GetConnectedEdges(nodeId).Select(e => GetConnectedNode(e));

		[MenuItem("Assets/Create/FriendSea/StateMachine")]
		static void CreateFile()
		{
			ProjectWindowUtil.CreateAssetWithContent("New StateMachine.friendseastatemachine", "");
			AssetDatabase.Refresh();
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

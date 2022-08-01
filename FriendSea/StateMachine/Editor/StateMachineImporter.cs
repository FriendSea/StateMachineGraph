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

				if (node.data is StateMachineTransitionNode)
				{
					continue;
				}

				var state = ScriptableObject.CreateInstance<StateMachineNodeAsset>();
				state.name = (node.data as StateMachineStateNode).name;
				ctx.AddObjectToAsset(node.id.id, state);
				var editor = Editor.CreateEditor(state);
				state.data = new StateMachineState() { behaviours = (node.data as StateMachineStateNode).behaviours };
				assets.Add(node.id.id, editor);
			}

			// construct edges

			main.entryState =
				new StateMachineState.Transition()
				{
					condition = new ImmediateTransition(),
					targets =
					GetConnectedNodes(entryId)
					.OrderBy(n => n.position.y)
					.Select(n => GenerateTransition(n, assets)).ToArray(),
				};
			main.fallbackState =
				new StateMachineState.Transition()
				{
					condition = new ImmediateTransition(),
					targets =
					GetConnectedNodes(fallbackId)
					.OrderBy(n => n.position.y)
					.Select(n => GenerateTransition(n, assets)).ToArray(),
				};

			foreach (var pair in assets)
			{
				(pair.Value.target as StateMachineNodeAsset).data.transition =
					new StateMachineState.Transition()
					{
						condition = new ImmediateTransition(),
						targets =
							GetConnectedNodes(pair.Key)
							.OrderBy(n => n.position.y)
							.Select(n => GenerateTransition(n, assets)).ToArray(),
					};
			}
		}

		StateMachineState.IStateReference GenerateTransition(GraphViewData.Node node, Dictionary<string, Editor> id2asset)
		{
			if (node.data is StateMachineStateNode)
				return new StateMachineState.StateReference() {
					nodeAsset = id2asset[node.id.id].target as StateMachineNodeAsset,
				};
			return
				new StateMachineState.Transition() {
					condition = (node.data as StateMachineTransitionNode).transition,
					targets =
						GetConnectedNodes(node.id.id)
						.OrderBy(n => n.position.y)
						.Select(n => GenerateTransition(n, id2asset)).ToArray(),
				};
		}

		IEnumerable<GraphViewData.Edge> GetConnectedEdges(string nodeId) =>
			data.elements.Where(e => e is GraphViewData.Edge).Select(e => e as GraphViewData.Edge).Where(e => e.outputNode.id == nodeId);

		GraphViewData.Node GetConnectedNode(GraphViewData.Edge edge) =>
			data.elements.Where(e => e is GraphViewData.Node).Select(e => e as GraphViewData.Node).Where(n => n.id.id == edge.inputNode.id).FirstOrDefault();

		IEnumerable<GraphViewData.Node> GetConnectedNodes(string nodeId) =>
			GetConnectedEdges(nodeId).Select(e => GetConnectedNode(e));

		[MenuItem("Assets/Create/FriendSea/StateMachine")]
		static void CreateFile()
		{
			ProjectWindowUtil.CreateAssetWithContent("New StateMachine.friendseastatemachine", "");
			AssetDatabase.Refresh();
		}
	}
}

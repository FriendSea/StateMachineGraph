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
		public override void OnImportAsset(AssetImportContext ctx)
		{
			var data = JsonUtility.FromJson<GraphViewData>(File.ReadAllText(assetPath));

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
					GetConnectedNodes(data, entryId)
					.OrderBy(n => n.position.y)
					.Select(n => GenerateTransition(data, n, assets)).ToArray(),
				};
			main.fallbackState =
				new StateMachineState.Transition()
				{
					condition = new ImmediateTransition(),
					targets =
					GetConnectedNodes(data, fallbackId)
					.OrderBy(n => n.position.y)
					.Select(n => GenerateTransition(data, n, assets)).ToArray(),
				};

			foreach (var pair in assets)
			{
				(pair.Value.target as StateMachineNodeAsset).data.transition =
					new StateMachineState.Transition()
					{
						condition = new ImmediateTransition(),
						targets =
							GetConnectedNodes(data, pair.Key)
							.OrderBy(n => n.position.y)
							.Select(n => GenerateTransition(data, n, assets)).ToArray(),
					};
			}
		}

		StateMachineState.IStateReference GenerateTransition(GraphViewData data, GraphViewData.Node node, Dictionary<string, Editor> id2asset)
		{
			if (node.data is StateMachineStateNode)
				return new StateMachineState.StateReference() {
					nodeAsset = id2asset[node.id.id].target as StateMachineNodeAsset,
				};
			return
				new StateMachineState.Transition() {
					condition = (node.data as StateMachineTransitionNode).transition,
					targets =
						GetConnectedNodes(data, node.id.id)
						.OrderBy(n => n.position.y)
						.Select(n => GenerateTransition(data, n, id2asset)).ToArray(),
				};
		}

		IEnumerable<GraphViewData.Edge> GetConnectedEdges(GraphViewData data, string nodeId) =>
			data.elements.Where(e => e is GraphViewData.Edge).Select(e => e as GraphViewData.Edge).Where(e => e.outputNode.id == nodeId);

		GraphViewData.Node GetConnectedNode(GraphViewData data, GraphViewData.Edge edge) =>
			data.elements.Where(e => e is GraphViewData.Node).Select(e => e as GraphViewData.Node).Where(n => n.id.id == edge.inputNode.id).FirstOrDefault();

		IEnumerable<GraphViewData.Node> GetConnectedNodes(GraphViewData data, string nodeId) =>
			GetConnectedEdges(data, nodeId).Select(e => GetConnectedNode(data, e));

		[MenuItem("Assets/Create/❏➡❏ fStateMachine Asset")]
		static void CreateFile()
		{
			ProjectWindowUtil.CreateAssetWithContent("New StateMachine.friendseastatemachine", JsonUtility.ToJson(new GraphViewData() {
				elements = new List<GraphViewData.ElementData>()
				{
					new GraphViewData.Node(){id = new GraphViewData.Id(System.Guid.NewGuid().ToString()), data = new StateMachineEntryNode()},
					new GraphViewData.Node(){id = new GraphViewData.Id(System.Guid.NewGuid().ToString()), data = new StateMachineFallbackNode(), position = new Vector2(0, 100)},
				}
			}));
			AssetDatabase.Refresh();
		}
	}
}

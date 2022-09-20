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

			var entryId = data.GetElements<GraphViewData.Node>().FirstOrDefault(n => n.data is StateMachineEntryNode).id.id;
			var fallbackId = data.GetElements<GraphViewData.Node>().FirstOrDefault(n => n.data is StateMachineFallbackNode).id.id;

			var assets = new Dictionary<string, Editor>();
			foreach (var node in data.GetElements<GraphViewData.Node>().Where(n => n.data is StateMachineStateNode))
			{
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

			ctx.AddObjectToAsset("main", main);
			ctx.SetMainObject(main);
			var deps = EditorUtility.CollectDependencies(new Object[] { main });
			foreach (var dep in deps)
			{
				var path = AssetDatabase.GetAssetPath(dep);
				if (string.IsNullOrEmpty(path)) continue;
				ctx.DependsOnSourceAsset(path);
			}
		}

		StateMachineState.IStateReference GenerateTransition(GraphViewData data, GraphViewData.Node node, Dictionary<string, Editor> id2asset)
		{
			if (node.data is StateMachineStateNode)
				return new StateMachineState.StateReference() {
					nodeAsset = id2asset[node.id.id].target as StateMachineNodeAsset,
				};

			if (node.data is StateMachineReferenceNode)
				return (node.data as StateMachineReferenceNode).asset?.entryState;

			if (node.data is StateMachineComponentTransitionNode)
				return new StateMachineComponentTransition();

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

		[MenuItem("Assets/Create/🔶➡🔶 fStateMachine Asset")]
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

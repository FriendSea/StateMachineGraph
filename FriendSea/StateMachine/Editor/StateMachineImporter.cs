using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.AssetImporters;
using System.Linq;
using System.IO;

namespace FriendSea.StateMachine
{
	[ScriptedImporter(0, "friendseastatemachine")]
	public class StateMachineImporter : ScriptedImporter
	{
		public override void OnImportAsset(AssetImportContext ctx)
		{
			var data = new GraphViewData();
			EditorJsonUtility.FromJsonOverwrite(File.ReadAllText(assetPath), data);
			data.InjectRootForParse();

			// create asset

			var main = ScriptableObject.CreateInstance<StateMachineAsset>();

			var entryNode = data.GetElements<GraphViewData.Node>().FirstOrDefault(n => n.data is EntryNode);
			var fallbackNode = data.GetElements<GraphViewData.Node>().FirstOrDefault(n => n.data is FallbackNode);

			var assets = new Dictionary<string, Editor>();
			foreach (var node in data.GetElements<GraphViewData.Node>().Where(n => n.data is StateNode))
			{
				var state = ScriptableObject.CreateInstance<NodeAsset>();
				state.name = (node.data as StateNode).name;
				ctx.AddObjectToAsset(node.id.id, state);
				var editor = Editor.CreateEditor(state);
				state.data = new State() { 
					behaviours = (node.data as StateNode).behaviours,
					residentStates = new State.ResitentStateRefernce() { 
						stateMachine = main,
						guids = node.GetContainingGroup().GetChildNodes().Where(e => e.data is ResidentStateNode).Select(e => e.id.id).ToArray(),
					},
					id = AssetDatabase.AssetPathToGUID(assetPath) + node.id.id,
				};
				assets.Add(node.id.id, editor);
			}

			// construct edges

			main.entryState =
				new State.Transition()
				{
					condition = new ImmediateTransition(),
					targets = entryNode.GetConnectedNodes()
						.OrderBy(n => n.position.y)
						.Select(n => GenerateTransition(data, n, assets)).ToArray(),
				};
			main.fallbackState =
				new State.Transition()
				{
					condition = new ImmediateTransition(),
					targets = fallbackNode.GetConnectedNodes()
						.OrderBy(n => n.position.y)
						.Select(n => GenerateTransition(data, n, assets)).ToArray(),
				};
			main.residentStates = data.elements
				.Where(e => e is GraphViewData.Node)
				.Where(e => (e as GraphViewData.Node).data is ResidentStateNode)
				.Select(e => e as GraphViewData.Node)
				.Select(e => new State() { 
					behaviours = (e.data as ResidentStateNode).behaviours,
					transition = new State.Transition()
					{
						condition = new ImmediateTransition(),
						targets = e.GetConnectedNodes()
							.OrderBy(n => n.position.y)
							.Select(n => GenerateTransition(data, n, assets)).ToArray(),
					},
					id = e.id.id,
				}).ToArray();

			foreach (var pair in assets)
			{
				(pair.Value.target as NodeAsset).data.transition =
					new State.Transition()
					{
						condition = new ImmediateTransition(),
						targets = data.GetElement<GraphViewData.Node>(pair.Key).GetConnectedNodes()
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
				if (!(dep is StateMachineAsset)) continue;
				ctx.DependsOnSourceAsset(path);
			}
		}

		State.IStateReference GenerateTransition(GraphViewData data, GraphViewData.Node node, Dictionary<string, Editor> id2asset)
		{
			if (node.data is StateNode)
				return new State.StateReference() {
					nodeAsset = id2asset[node.id.id].target as NodeAsset,
				};

			if (node.data is StateMachineReferenceNode)
				return (node.data as StateMachineReferenceNode).asset?.entryState;

			if (node.data is ComponentTransitionNode)
				return new ComponentTransition();

			if (node.data is SequenceNode)
				return new State.Sequence()
				{
					targets = node.GetConnectedNodes()
						.OrderBy(n => n.position.y)
						.Select(n => GenerateTransition(data, n, id2asset)).ToArray(),
				};

			return
				new State.Transition() {
					condition = (node.data as TransitionNode).transition,
					targets = node.GetConnectedNodes()
						.OrderBy(n => n.position.y)
						.Select(n => GenerateTransition(data, n, id2asset)).ToArray(),
				};
		}

		[MenuItem("Assets/Create/🔶➡🔶 fStateMachine Asset")]
		static void CreateFile()
		{
			ProjectWindowUtil.CreateAssetWithContent("New StateMachine.friendseastatemachine", EditorJsonUtility.ToJson(new GraphViewData() {
				elements = new List<GraphViewData.ElementData>()
				{
					new GraphViewData.Node(){id = new GraphViewData.Id(System.Guid.NewGuid().ToString()), data = new EntryNode()},
					new GraphViewData.Node(){id = new GraphViewData.Id(System.Guid.NewGuid().ToString()), data = new FallbackNode(), position = new Vector2(0, 100)},
				}
			}));
			AssetDatabase.Refresh();
		}
	}
}

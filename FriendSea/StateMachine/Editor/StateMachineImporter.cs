﻿using System.Collections;
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

			// create asset

			var main = ScriptableObject.CreateInstance<StateMachineAsset>();

			var entryId = data.GetElements<GraphViewData.Node>().FirstOrDefault(n => n.data is EntryNode).id.id;
			var fallbackId = data.GetElements<GraphViewData.Node>().FirstOrDefault(n => n.data is FallbackNode).id.id;

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
						guids = GetChildNodes(data, GetContainingGroup(data, node.id.id)?.id?.id).Where(e => e.data is ResidentStateNode).Select(e => e.id.id).ToArray(),
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
					targets = GetConnectedNodes(data, entryId)
						.OrderBy(n => n.position.y)
						.Select(n => GenerateTransition(data, n, assets)).ToArray(),
				};
			main.fallbackState =
				new State.Transition()
				{
					condition = new ImmediateTransition(),
					targets = GetConnectedNodes(data, fallbackId)
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
						targets = GetConnectedNodes(data, e.id.id)
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
						targets = GetConnectedNodes(data, pair.Key)
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
					targets = GetConnectedNodes(data, node.id.id)
						.OrderBy(n => n.position.y)
						.Select(n => GenerateTransition(data, n, id2asset)).ToArray(),
				};

			return
				new State.Transition() {
					condition = (node.data as TransitionNode).transition,
					targets = GetConnectedNodes(data, node.id.id)
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

		GraphViewData.Group GetContainingGroup(GraphViewData data, string nodeId) =>
			data.elements.Where(e => e is GraphViewData.Group).FirstOrDefault(e => (e as GraphViewData.Group).nodes.Select(n => n.id).Contains(nodeId)) as GraphViewData.Group;

		IEnumerable<GraphViewData.Node> GetChildNodes(GraphViewData data, string groupId) =>
			(data.elements.FirstOrDefault(e => e.id.id == groupId) as GraphViewData.Group)?.nodes?.Select(id => data.elements.FirstOrDefault(e => e.id.id == id.id) as GraphViewData.Node) ?? new List<GraphViewData.Node>();

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

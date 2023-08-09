using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace FriendSea
{
	public static class GraphViewDataExtension
	{
		public static void InjectRootForParse(this GraphViewData root)
		{
			foreach (var element in root.elements)
				element.root = root;
		}

		public static IEnumerable<T> GetElements<T>(this GraphViewData root) where T : GraphViewData.ElementData => 
			root.elements.Where(e => e is T).Select(e => e as T);

		public static T GetElement<T>(this GraphViewData root, string id) where T : GraphViewData.ElementData =>
			root.elements.Where(e => e is T).Select(e => e as T).FirstOrDefault(e => e.id.id == id);

		public static IEnumerable<GraphViewData.Edge> GetConnectedEdges(this GraphViewData.Node node) =>
			node.root.elements.Where(e => e is GraphViewData.Edge).Select(e => e as GraphViewData.Edge).Where(e => e.outputNode.id == node.id.id);

		public static GraphViewData.Node GetConnectedNode(this GraphViewData.Edge edge) =>
			edge.root.elements.Where(e => e is GraphViewData.Node).Select(e => e as GraphViewData.Node).Where(n => n.id.id == edge.inputNode.id).FirstOrDefault();

		public static IEnumerable<GraphViewData.Node> GetConnectedNodes(this GraphViewData.Node node) =>
			node.GetConnectedEdges().Select(e => e.GetConnectedNode());

		public static GraphViewData.Group GetContainingGroup(this GraphViewData.Node node) =>
			node.root.elements.Where(e => e is GraphViewData.Group).FirstOrDefault(e => (e as GraphViewData.Group).nodes.Select(n => n.id).Contains(node.id.id)) as GraphViewData.Group;

		public static IEnumerable<GraphViewData.Node> GetChildNodes(this GraphViewData.Group group) =>
			 group.nodes.Select(id => group.root.elements.FirstOrDefault(e => e.id.id == id.id) as GraphViewData.Node) ?? new List<GraphViewData.Node>();
	}
}

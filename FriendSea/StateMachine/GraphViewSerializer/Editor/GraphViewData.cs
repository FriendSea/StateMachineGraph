using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace FriendSea.GraphViewSerializer
{
	[System.Serializable]
	public class GraphViewData
	{
		[System.Serializable]
		public class Id
		{
			public string id;

			public Id(string id) => this.id = id;
		}

		[System.Serializable]
		public abstract class ElementData
		{
			[SerializeField]
			public Id id;
			[System.NonSerialized]
			public GraphViewData root;

			public virtual IEnumerable<Id> CollectDependentGuids() { yield break; }
		}

		public abstract class PositionableElementData : ElementData
		{
			[SerializeField]
			public Vector2 position;
		}

		[UnityEngine.Scripting.APIUpdating.MovedFrom(false, sourceNamespace: "FriendSea", sourceClassName: "GraphViewData/Node")]
		[System.Serializable]
		public class Node : PositionableElementData
		{
			[SerializeReference]
			public object data;
		}

		[UnityEngine.Scripting.APIUpdating.MovedFrom(false, sourceNamespace: "FriendSea", sourceClassName: "GraphViewData/Edge")]
		[System.Serializable]
		public class Edge : ElementData
		{
			[SerializeField]
			public Id outputNode;
			[SerializeField]
			public string outputPort;
			[SerializeField]
			public Id inputNode;
			[SerializeField]
			public string inputPort;

			public override IEnumerable<Id> CollectDependentGuids()
			{
				yield return outputNode;
				yield return inputNode;
			}
		}

		[UnityEngine.Scripting.APIUpdating.MovedFrom(false, sourceNamespace: "FriendSea", sourceClassName: "GraphViewData/Group")]
		[System.Serializable]
		public class Group : ElementData
		{
			[SerializeField]
			public string name;
			[SerializeField]
			public Id[] nodes;

			public override IEnumerable<Id> CollectDependentGuids()
			{
				foreach (var i in nodes)
					yield return i;
			}
		}

		[UnityEngine.Scripting.APIUpdating.MovedFrom(false, sourceNamespace: "FriendSea", sourceClassName: "GraphViewData/StickyNote")]
		public class StickyNote : PositionableElementData
		{
			[SerializeField]
			public string content;
			[SerializeField]
			public Vector2 size;
		}

		[SerializeReference]
		public List<ElementData> elements = new List<ElementData>();

		[SerializeField]
		Vector3 viewPosition;
		[SerializeField]
		Vector3 viewScale = Vector3.one;
	}
}

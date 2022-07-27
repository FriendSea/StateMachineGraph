using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace FriendSea
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

			public virtual IEnumerable<Id> CollectUsedGuids()
			{
				yield return id;
			}
		}

		public abstract class PositionableElementData : ElementData
		{
			[SerializeField]
			public Vector2 position;
		}

		[System.Serializable]
		public class Node : PositionableElementData
		{
			[SerializeReference]
			public object data;
		}

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

			public override IEnumerable<Id> CollectUsedGuids()
			{
				yield return id;
				yield return outputNode;
				yield return inputNode;
			}
		}

		[System.Serializable]
		public class Group : ElementData
		{
			[SerializeField]
			public string name;
			[SerializeField]
			public Id[] nodes;

			public override IEnumerable<Id> CollectUsedGuids()
			{
				yield return id;
				foreach (var i in nodes)
					yield return i;
			}
		}

		[SerializeReference]
		public List<ElementData> elements = new List<ElementData>();

		public IEnumerable<T> GetElements<T>() where T : class => elements.Where(e => e is T).Select(e => e as T);

		[SerializeField]
		Vector3 viewPosition;
		[SerializeField]
		Vector3 viewScale = Vector3.one;
	}
}

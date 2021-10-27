using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FriendSea
{
	[System.Serializable]
	public class GraphViewData<T>
	{
		[SerializeField]
		public Node[] nodes = new Node[0];
		[SerializeField]
		public Edge[] edges = new Edge[0];
		[SerializeField]
		public Group[] groups = new Group[0];

		[System.Serializable]
		public class Node
		{
			[SerializeField]
			public string id;
			[SerializeField]
			Vector2 position;
			[SerializeReference]
			public T data;
		}

		[System.Serializable]
		public class Edge
		{
			[SerializeField]
			public string outputNode;
			[SerializeField]
			public string outputPort;
			[SerializeField]
			public string inputNode;
			[SerializeField]
			public string inputPort;
		}

		[System.Serializable]
		public class Group
		{
			[SerializeField]
			string id;
			[SerializeField]
			string name;
			[SerializeField]
			string[] nodes;
		}

		[SerializeField]
		Vector3 viewPosition;
		[SerializeField]
		Vector3 viewScale = Vector3.one;
	}
}

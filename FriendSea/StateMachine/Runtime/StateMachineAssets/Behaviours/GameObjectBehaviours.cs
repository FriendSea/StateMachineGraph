using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FriendSea.StateMachine.Behaviours
{
	[DisplayName("GameObject/SetActive")]
	public partial class GameObjectSetActive : BehaviourBase
	{
		[SerializeField] bool active;
		protected override void OnEnter(IContextContainer obj) => obj.Get<GameObject>().SetActive(active);
		protected override void OnExit(IContextContainer obj) { }
		protected override void OnUpdate(IContextContainer obj) { }
	}

	[DisplayName("GameObject/SetLayer")]
	public partial class GameObjectSetLayer : BehaviourBase
	{
		[SerializeField] int layer;
		protected override void OnEnter(IContextContainer obj) => obj.Get<GameObject>().layer = layer;
		protected override void OnExit(IContextContainer obj) { }
		protected override void OnUpdate(IContextContainer obj) { }
	}

	[DisplayName("GameObject/Destroy")]
	public partial class GameObjectDestroy : BehaviourBase
	{
		[SerializeField] int layer;
		protected override void OnEnter(IContextContainer obj) => Object.Destroy(obj.Get<GameObject>());
		protected override void OnExit(IContextContainer obj) { }
		protected override void OnUpdate(IContextContainer obj) { }
	}

	[DisplayName("GameObject/Instantiate")]
	public partial class GameObjectInstantiate : BehaviourBase
	{
		[SerializeField] GameObject original;
		[SerializeField]
		Vector3 position;

		protected override void OnEnter(IContextContainer obj) {
			var pos = position;
			var rot = Quaternion.identity;
			var transform = obj.Get<Transform>();
			if (transform != null)
			{
				pos = transform.localToWorldMatrix.MultiplyPoint(pos);
				rot *= transform.rotation; 
			}
			GameObjectPool.Instantiate(original, pos, rot, null);
		} 
		protected override void OnExit(IContextContainer obj) { }
		protected override void OnUpdate(IContextContainer obj) { }
	}

	public static class GameObjectPool
	{
		static Dictionary<GameObject, GameObject> instance2prefab = new Dictionary<GameObject, GameObject>();
		static Dictionary<GameObject, Stack<GameObject>> prefab2pool = new Dictionary<GameObject, Stack<GameObject>>();

		public static GameObject Instantiate(GameObject original, Vector3 position, Quaternion rotation, Transform parent = null)
		{
			if (!prefab2pool.ContainsKey(original))
				prefab2pool.Add(original, new Stack<GameObject>());

			if (prefab2pool[original].Count <= 0)
			{
				var instance = Object.Instantiate(original, position, rotation, parent);
				instance2prefab.Add(instance, original);
				return instance;
			}
			else
			{
				var instance = prefab2pool[original].Pop();
				if(instance == null)
					return Instantiate(original, position, rotation, parent);
				instance.transform.parent = parent;
				instance.transform.SetPositionAndRotation(position, rotation);
				instance.SetActive(true);
				return instance;
			}
		}

		public static void Destroy(GameObject gameObject)
		{
			gameObject.SetActive(false);
			prefab2pool[instance2prefab[gameObject]].Push(gameObject);
		}

		public static void ClearAll()
		{
			foreach (var pool in prefab2pool.Values)
				foreach (var obj in pool)
					if (obj != null)
						Object.Destroy(obj);
			prefab2pool.Clear();
			instance2prefab.Clear();
		}
	}
}

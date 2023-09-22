using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FriendSea.StateMachine.Behaviours
{
	[DisplayName("GameObject/SetActive")]
	public class GameObjectSetActive : IBehaviour
	{
		[SerializeField] bool active;
		public void OnEnter(IContextContainer obj) => obj.Get<GameObject>().SetActive(active);
		public void OnExit(IContextContainer obj) { }
		public void OnUpdate(IContextContainer obj) { }
	}

	[DisplayName("GameObject/SetLayer")]
	public class GameObjectSetLayer : IBehaviour
	{
		[SerializeField] int layer;
		public void OnEnter(IContextContainer obj) => obj.Get<GameObject>().layer = layer;
		public void OnExit(IContextContainer obj) { }
		public void OnUpdate(IContextContainer obj) { }
	}

	[DisplayName("GameObject/Destroy")]
	public class GameObjectDestroy : IBehaviour
	{
		[SerializeField] int layer;
		public void OnEnter(IContextContainer obj) => Object.Destroy(obj.Get<GameObject>());
		public void OnExit(IContextContainer obj) { }
		public void OnUpdate(IContextContainer obj) { }
	}

	[DisplayName("GameObject/Instantiate")]
	public class GameObjectInstantiate : IBehaviour
	{
		[SerializeField] GameObject original;
		public void OnEnter(IContextContainer obj) => Object.Instantiate(original);
		public void OnExit(IContextContainer obj) { }
		public void OnUpdate(IContextContainer obj) { }
	}
}

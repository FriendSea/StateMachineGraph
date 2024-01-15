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
		protected override void OnEnter(IContextContainer obj) => Object.Instantiate(original);
		protected override void OnExit(IContextContainer obj) { }
		protected override void OnUpdate(IContextContainer obj) { }
	}
}

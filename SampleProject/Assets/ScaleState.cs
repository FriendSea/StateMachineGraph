using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FriendSea.StateMachine;

public partial class ScaleBehaviour : BehaviourBase
{
	[InjectContext]
	Transform transform;

	protected override void OnEnter(IContextContainer obj)
	{
		Debug.Log($"Entered Scale State.");
	}

	protected override void OnExit(IContextContainer obj) { }

	protected override void OnUpdate(IContextContainer obj)
	{
		transform.localScale = Vector3.one * (1f + (obj.FrameCount % 30) / 100f);
	}
}

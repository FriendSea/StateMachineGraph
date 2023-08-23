using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FriendSea.StateMachine;

public partial class ScaleBehaviour : BehaviourBase
{
	[InjectContext]
	Transform transform;

	protected override void OnEnter(IContextContainer obj, int frameCount)
	{
		Debug.Log($"Entered Scale State.");
	}

	protected override void OnExit(IContextContainer obj, int frameCount) { }

	protected override void OnUpdate(IContextContainer obj, int frameCount)
	{
		transform.localScale = Vector3.one * (1f + (frameCount % 30) / 100f);
	}
}

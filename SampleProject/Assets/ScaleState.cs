using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FriendSea.StateMachine;

public partial class ScaleBehaviour : BehaviourBase
{
	[InjectContext]
	object a;

	public override void OnEnter(IContextContainer obj, int frameCount)
	{
		Debug.Log($"Entered Scale State.");
	}

	public override void OnExit(IContextContainer obj, int frameCount) { }

	public override void OnUpdate(IContextContainer obj, int frameCount)
	{
		obj.Get<Transform>().localScale = Vector3.one * (1f + (frameCount % 30) / 100f);
	}
}

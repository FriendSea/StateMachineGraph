using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FriendSea.StateMachine;

public partial class ScaleBehaviour : IBehaviour
{
	[InjectContext]
	Transform transform;

	public void OnEnter(IContextContainer obj)
	{
		Debug.Log($"Entered Scale State.");
	}

	public void OnExit(IContextContainer obj) { }

	public void OnUpdate(IContextContainer obj)
	{
		transform.localScale = Vector3.one * (1f + (obj.FrameCount % 99999) / 100f);
	}
}

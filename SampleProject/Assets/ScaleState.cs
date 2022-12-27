using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FriendSea.StateMachine;

public class ScaleBehaviour : State.IBehaviour
{
	public void OnEnter(CachedComponents obj, int frameCount)
	{
		Debug.Log($"Entered Scale State.");
	}

	public void OnExit(CachedComponents obj, int frameCount) { }

	public void OnUpdate(CachedComponents obj, int frameCount)
	{
		obj.transform.localScale = Vector3.one * (1f + (frameCount % 30) / 100f);
	}
}

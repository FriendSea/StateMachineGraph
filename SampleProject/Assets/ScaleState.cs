using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FriendSea.StateMachine;

public class ScaleBehaviour : State.IBehaviour
{
	public void OnEnter(IContextContainer obj, int frameCount)
	{
		Debug.Log($"Entered Scale State.");
	}

	public void OnExit(IContextContainer obj, int frameCount) { }

	public void OnUpdate(IContextContainer obj, int frameCount)
	{
		obj.Get<Transform>().localScale = Vector3.one * (1f + (frameCount % 30) / 100f);
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FriendSea.StateMachine;

public class MoveBehaviour : BehaviourBase
{
	[SerializeField]
	Vector3 velocity;

	public override void OnEnter(IContextContainer obj, int frameCount)
	{
		Debug.Log($"Entered Move State.");
	}

	public override void OnExit(IContextContainer obj, int frameCount) { }

	public override void OnUpdate(IContextContainer obj, int frameCount)
	{
		obj.Get<Transform>().position += velocity * Time.fixedDeltaTime;
	}
}

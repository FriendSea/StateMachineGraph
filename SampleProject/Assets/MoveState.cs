using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FriendSea.StateMachine;

public partial class MoveBehaviour : BehaviourBase
{
	[SerializeField]
	Vector3 velocity;

	[InjectContext]
	Transform transform;

	public override void OnEnter(IContextContainer obj, int frameCount)
	{
		Debug.Log($"Entered Move State.");
	}

	public override void OnExit(IContextContainer obj, int frameCount) { }

	public override void OnUpdate(IContextContainer obj, int frameCount)
	{
		transform.position += velocity * Time.fixedDeltaTime;
	}
}

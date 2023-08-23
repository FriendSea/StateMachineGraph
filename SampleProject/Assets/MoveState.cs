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

	protected override void OnEnter(IContextContainer obj, int frameCount)
	{
		Debug.Log($"Entered Move State.");
	}

	protected override void OnExit(IContextContainer obj, int frameCount) { }

	protected override void OnUpdate(IContextContainer obj, int frameCount)
	{
		transform.position += velocity * Time.fixedDeltaTime;
	}
}

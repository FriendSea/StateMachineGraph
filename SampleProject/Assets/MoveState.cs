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

	protected override void OnEnter(IContextContainer obj)
	{
		Debug.Log($"Entered Move State.");
	}

	protected override void OnExit(IContextContainer obj) { }

	protected override void OnUpdate(IContextContainer obj)
	{
		transform.position += velocity * Time.fixedDeltaTime;
	}
}

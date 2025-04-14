using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FriendSea.StateMachine;

public partial class MoveBehaviour : IBehaviour
{
	[SerializeField]
	Vector3 velocity;

	[InjectContext]
	Transform transform;

	public void OnEnter(IContextContainer obj)
	{
		Debug.Log($"Entered Move State.");
	}

	public void OnExit(IContextContainer obj) { }

	public void OnUpdate(IContextContainer obj)
	{
		transform.position += velocity * Time.fixedDeltaTime;
	}
}

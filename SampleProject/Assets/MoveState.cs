using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FriendSea.StateMachine;

[DisplayName("Transform/Move")]
public class MoveBehaviour : State.IBehaviour
{
	[SerializeField]
	Vector3 velocity;

	public void OnEnter(IContextContainer obj, int frameCount)
	{
		Debug.Log($"Entered Move State.");
	}

	public void OnExit(IContextContainer obj, int frameCount) { }

	public void OnUpdate(IContextContainer obj, int frameCount)
	{
		obj.Get<Transform>().position += velocity * Time.fixedDeltaTime;
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FriendSea;

public class MoveBehaviour : StateMachineState.IBehaviour
{
	[SerializeField]
	Vector3 velocity;

	public void OnExit(CachedComponents obj, int frameCount) { }

	public void OnUpdate(CachedComponents obj, int frameCount)
	{
		obj.transform.position += velocity * Time.fixedDeltaTime;
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FriendSea;

public class MoveState : StateMachineStateBase
{
	[SerializeField]
	StateMachineNodeAsset nextNode;
	[SerializeField]
	int length;
	[SerializeField]
	Vector3 velocity;

	public override void OnUpdate(GameObject obj, int frameCount)
	{
		obj.transform.position += velocity * Time.fixedDeltaTime;
	}

	public override IStateMachineState<GameObject> NextState(GameObject obj, int frameCount)
	{
		if (frameCount > length)
			return nextNode;
		return this;
	}
}

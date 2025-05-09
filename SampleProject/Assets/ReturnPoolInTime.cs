using FriendSea.StateMachine.Behaviours;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReturnPoolInTime : MonoBehaviour
{
	[SerializeField]
	float time = 1f;

	float restTime;
	private void OnEnable()
	{
		restTime = time;
	}

	private void Update()
	{
		restTime -= Time.deltaTime;
		if (restTime <= 0) {
			GameObjectPool.Destroy(gameObject);
		}
	}
}

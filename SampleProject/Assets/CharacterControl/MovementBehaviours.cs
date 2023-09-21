using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FriendSea.StateMachine;
using UnityEngine.InputSystem;

class AccelByInput : BehaviourBase
{
	[SerializeField]
	InputActionProperty input;
	[InjectContext]
	Movement movement;

	protected override void OnEnter(IContextContainer obj) { }
	protected override void OnExit(IContextContainer obj) { }
	protected override void OnUpdate(IContextContainer obj) {
		movement.Velocity += (Vector3)input.action.ReadValue<Vector2>();
	}
}

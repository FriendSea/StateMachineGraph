using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FriendSea.StateMachine;
using UnityEngine.InputSystem;

partial class AccelByInput : BehaviourBase
{
	[SerializeField]
	float factor;
	[SerializeField]
	InputActionProperty input;
	[InjectContext]
	Movement movement;

	protected override void OnEnter(IContextContainer obj) {
		input.action.Enable();
	}
	protected override void OnExit(IContextContainer obj) {
		input.action.Disable();
	}
	protected override void OnUpdate(IContextContainer obj) {
		var vec = (Vector3)input.action.ReadValue<Vector2>();
		movement.Velocity += vec * factor;
	}
}

partial class MultiplyVelocity : BehaviourBase
{
	[SerializeField]
	float factor;
	[InjectContext]
	Movement movement;

	protected override void OnEnter(IContextContainer obj) { }

	protected override void OnExit(IContextContainer obj) { }

	protected override void OnUpdate(IContextContainer obj)
	{
		movement.Velocity *= factor;
	}
}

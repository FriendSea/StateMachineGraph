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

partial class HorizontalInputAccel : BehaviourBase
{
	[SerializeField]
	float factor;
	[SerializeField]
	InputActionProperty input;
	[InjectContext]
	Movement movement;

	protected override void OnEnter(IContextContainer obj)
	{
		input.action.Enable();
	}
	protected override void OnExit(IContextContainer obj)
	{
		input.action.Disable();
	}
	protected override void OnUpdate(IContextContainer obj)
	{
		var val = (float)input.action.ReadValue<float>();
		movement.Velocity += Vector3.right * val * factor;
	}
}

partial class AddVelocity : BehaviourBase
{
	[SerializeField]
	Vector3 amount;
	[InjectContext]
	Movement movement;

	protected override void OnEnter(IContextContainer obj) { }

	protected override void OnExit(IContextContainer obj) { }

	protected override void OnUpdate(IContextContainer obj)
	{
		movement.Velocity += amount;
	}
}

partial class MultiplyVelocity : BehaviourBase
{
	[SerializeField]
	Vector3 factor;
	[InjectContext]
	Movement movement;

	protected override void OnEnter(IContextContainer obj) { }

	protected override void OnExit(IContextContainer obj) { }

	protected override void OnUpdate(IContextContainer obj)
	{
		movement.Velocity = Vector3.Scale(movement.Velocity, factor);
	}
}

partial class GroundedTransition : ConditionBase
{
	[InjectContext]
	Movement movement;

	protected override bool IsValid(IContextContainer obj) => movement.IsGrounded;
}

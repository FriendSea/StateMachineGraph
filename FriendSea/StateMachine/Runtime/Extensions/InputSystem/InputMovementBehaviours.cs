using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static System.Collections.Specialized.BitVector32;

namespace FriendSea.StateMachine.Behaviours
{
	[DisplayName("Movements/Input/AddVelocity")]
	partial class AddMovementVelocityFromInput : BehaviourBase
	{
		[SerializeField]
		float factor = 1;
		[SerializeField]
		InputActionProperty input;
		[InjectContext]
		IMovement movement;

		protected override void OnEnter(IContextContainer obj)
		{
			input.action.Enable();
		}
		protected override void OnExit(IContextContainer obj) { }
		protected override void OnUpdate(IContextContainer obj)
		{
			var vec = (Vector3)input.action.ReadValue<Vector2>();
			movement.Velocity += vec * factor;
		}
	}

	[DisplayName("Movements/Input/AddHorizontalVelocity")]
	partial class AddMovementHorizontalVelocityFromInput : BehaviourBase
	{
		[SerializeField]
		float factor = 1;
		[SerializeField]
		InputActionProperty input;
		[InjectContext]
		IMovement movement;

		protected override void OnEnter(IContextContainer obj)
		{
			if (!input.action.enabled)
				input.action.Enable();
		}
		protected override void OnExit(IContextContainer obj) { }
		protected override void OnUpdate(IContextContainer obj)
		{
			var val = (float)input.action.ReadValue<float>();
			movement.Velocity += Vector3.right * val * factor;
		}
	}

	[DisplayName("Movements/Input/AddVerticalVelocity")]
	partial class AddMovementVerticalVelocityFromInput : BehaviourBase
	{
		[SerializeField]
		float factor = 1;
		[SerializeField]
		InputActionProperty input;
		[InjectContext]
		IMovement movement;

		protected override void OnEnter(IContextContainer obj)
		{
			if (!input.action.enabled)
				input.action.Enable();
		}
		protected override void OnExit(IContextContainer obj) { }
		protected override void OnUpdate(IContextContainer obj)
		{
			var val = (float)input.action.ReadValue<float>();
			movement.Velocity += Vector3.up * val * factor;
		}
	}
}

#if STATEMACHINE_USE_INPUTSYSTEM

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static System.Collections.Specialized.BitVector32;

namespace FriendSea.StateMachine.Behaviours
{
	[DisplayName("Movements/Input/AddVelocity")]
	partial class AddMovementVelocityFromInput : IBehaviour
	{
		[SerializeField]
		float factor = 1;
		[SerializeField]
		InputActionProperty input;
		[InjectContext]
		IMovement movement;

		public void OnEnter(IContextContainer obj)
		{
			input.action.Enable();
		}
		public void OnExit(IContextContainer obj) { }
		public void OnUpdate(IContextContainer obj)
		{
			var vec = (Vector3)input.action.ReadValue<Vector2>();
			movement.Velocity += vec * factor;
		}
	}

	[DisplayName("Movements/Input/AddHorizontalVelocity")]
	partial class AddMovementHorizontalVelocityFromInput : IBehaviour
	{
		[SerializeField]
		float factor = 1;
		[SerializeField]
		InputActionProperty input;
		[InjectContext]
		IMovement movement;

		public void OnEnter(IContextContainer obj)
		{
			if (!input.action.enabled)
				input.action.Enable();
		}
		public void OnExit(IContextContainer obj) { }
		public void OnUpdate(IContextContainer obj)
		{
			var val = (float)input.action.ReadValue<float>();
			movement.Velocity += Vector3.right * val * factor;
		}
	}

	[DisplayName("Movements/Input/AddVerticalVelocity")]
	partial class AddMovementVerticalVelocityFromInput : IBehaviour
	{
		[SerializeField]
		float factor = 1;
		[SerializeField]
		InputActionProperty input;
		[InjectContext]
		IMovement movement;

		public void OnEnter(IContextContainer obj)
		{
			if (!input.action.enabled)
				input.action.Enable();
		}
		public void OnExit(IContextContainer obj) { }
		public void OnUpdate(IContextContainer obj)
		{
			var val = (float)input.action.ReadValue<float>();
			movement.Velocity += Vector3.up * val * factor;
		}
	}
}

#endif

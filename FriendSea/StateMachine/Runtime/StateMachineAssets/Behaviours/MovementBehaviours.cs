using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FriendSea.StateMachine.Behaviours
{
	public interface IMovement
	{
		public Vector3 Velocity { get; set; }
	}

	[DisplayName("Movements/AddVelocity")]
	partial class AddMovementVelocity : IBehaviour
    {
		[SerializeField]
		Vector3 amount;
		[InjectContext]
		IMovement movement;

		public void OnEnter(IContextContainer obj) { }

		public void OnExit(IContextContainer obj) { }

		public void OnUpdate(IContextContainer obj)
		{
			movement.Velocity += amount;
		}
	}

	[DisplayName("Movements/MultiplyVelocity")]
	partial class MultiplyMovementVelocity : IBehaviour
	{
		[SerializeField]
		Vector3 factor;
		[InjectContext]
		IMovement movement;

		public void OnEnter(IContextContainer obj) { }

		public void OnExit(IContextContainer obj) { }

		public void OnUpdate(IContextContainer obj)
		{
			movement.Velocity = Vector3.Scale(movement.Velocity, factor);
		}
	}
}

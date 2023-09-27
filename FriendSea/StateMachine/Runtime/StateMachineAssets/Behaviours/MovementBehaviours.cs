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
	partial class AddMovementVelocity : BehaviourBase
	{
		[SerializeField]
		Vector3 amount;
		[InjectContext]
		IMovement movement;

		protected override void OnEnter(IContextContainer obj) { }

		protected override void OnExit(IContextContainer obj) { }

		protected override void OnUpdate(IContextContainer obj)
		{
			movement.Velocity += amount;
		}
	}

	[DisplayName("Movements/MultiplyVelocity")]
	partial class MultiplyMovementVelocity : BehaviourBase
	{
		[SerializeField]
		Vector3 factor;
		[InjectContext]
		IMovement movement;

		protected override void OnEnter(IContextContainer obj) { }

		protected override void OnExit(IContextContainer obj) { }

		protected override void OnUpdate(IContextContainer obj)
		{
			movement.Velocity = Vector3.Scale(movement.Velocity, factor);
		}
	}
}

using FriendSea.StateMachine.Behaviours;
using FriendSea.StateMachine.Controls;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FriendSea.StateMachine.Conditions{
	public interface IPlatformerObject
	{
		public bool IsGrounded { get; }
	}

	[DisplayName("Platformer/IsGrounded")]
	partial class GroundedTransition : Transition.ICondition
	{
		[InjectContext]
		IPlatformerObject movement;

		public bool IsValid(IContextContainer obj) => movement.IsGrounded;
	}
}

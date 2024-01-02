using FriendSea.StateMachine.Behaviours;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FriendSea.StateMachine.Conditions{

    public interface IPlatformerObject
    {
		public bool IsGrounded { get; }
	}

	[DisplayName("Platformer/IsGrounded")]
	partial class GroundedTransition : ConditionBase
	{
		[InjectContext]
		IPlatformerObject movement;

		protected override bool IsValid(IContextContainer obj) => movement.IsGrounded;
	}
}

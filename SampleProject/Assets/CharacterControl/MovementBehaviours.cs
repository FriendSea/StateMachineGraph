using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FriendSea.StateMachine;

partial class GroundedTransition : ConditionBase
{
	[InjectContext]
	Movement movement;

	protected override bool IsValid(IContextContainer obj) => movement.IsGrounded;
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FriendSea.StateMachine.Controls;

namespace FriendSea.StateMachine
{
	[DisplayName("Wait/Time")]
	public class TimeTransition : Transition.ICondition
	{
		[SerializeField]
		float length;

		public bool IsValid(IContextContainer obj) => obj.Time >= length;
	}
}
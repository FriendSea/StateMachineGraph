using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FriendSea.StateMachine.Controls;

namespace FriendSea.StateMachine.Conditions
{
	[UnityEngine.Scripting.APIUpdating.MovedFrom(false, sourceNamespace: "FriendSea.StateMachine", sourceClassName: "TimeTransition")]
	[DisplayName("Wait/Time")]
	public partial class TimeTransition : Transition.ICondition
	{
		[SerializeField]
		float length;

		public bool IsValid(IContextContainer obj) => obj.Time >= length;
	}
}
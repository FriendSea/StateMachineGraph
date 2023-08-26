using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FriendSea.StateMachine
{
	[DisplayName("WaitFrames")]
	public class FrameTransition : State.Transition.ICondition
	{
		[SerializeField]
		int length;

		public bool IsValid(IContextContainer obj) => obj.FrameCount >= length;
	}
}

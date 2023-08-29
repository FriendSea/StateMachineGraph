using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FriendSea.StateMachine.Controls;

namespace FriendSea.StateMachine
{
	[DisplayName("WaitFrames")]
	public class FrameTransition : Transition.ICondition
	{
		[SerializeField]
		int length;

		public bool IsValid(IContextContainer obj) => obj.FrameCount >= length;
	}
}

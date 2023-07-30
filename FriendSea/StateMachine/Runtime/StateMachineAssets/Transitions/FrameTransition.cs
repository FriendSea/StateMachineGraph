using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FriendSea.StateMachine
{
	public class FrameTransition : State.Transition.ICondition
	{
		[SerializeField]
		int length;

		public bool IsValid(IContextContainer obj, int frameCount) => frameCount >= length;
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FriendSea
{
	public class FrameTransition : StateMachineState.Transition.ICondition
	{
		[SerializeField]
		int length;

		public bool IsValid(CachedComponents obj, int frameCount) => frameCount >= length;
	}
}

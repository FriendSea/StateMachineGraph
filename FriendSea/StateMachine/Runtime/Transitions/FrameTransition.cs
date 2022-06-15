using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FriendSea
{
	public class FrameTransition : StateMachineState.ITransition
	{
		[SerializeField]
		int length;

		public bool ShouldTransition(CachedComponents obj, int frameCount) => frameCount >= length;
	}
}

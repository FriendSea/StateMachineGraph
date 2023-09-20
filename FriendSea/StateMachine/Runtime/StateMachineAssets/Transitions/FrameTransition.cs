using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FriendSea.StateMachine.Controls;

namespace FriendSea.StateMachine.Conditions
{
	[UnityEngine.Scripting.APIUpdating.MovedFrom(false, sourceNamespace: "FriendSea.StateMachine", sourceClassName: "FrameTransition")]
	[DisplayName("Wait/Frames")]
	public class FrameTransition : Transition.ICondition
	{
		[SerializeField]
		int length;

		public bool IsValid(IContextContainer obj) => obj.FrameCount >= length;
	}
}

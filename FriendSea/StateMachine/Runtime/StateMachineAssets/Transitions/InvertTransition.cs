using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FriendSea.StateMachine
{
	[DisplayName("Invert")]
	public class InvertTransition : State.Transition.ICondition
	{
		[SerializeReference]
		internal State.Transition.ICondition transition;

		public bool IsValid(IContextContainer obj)
		{
			return !transition.IsValid(obj);
		}
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FriendSea.StateMachine.Controls;

namespace FriendSea.StateMachine
{
	[DisplayName("Invert")]
	public class InvertTransition : Transition.ICondition
	{
		[SerializeReference]
		internal Transition.ICondition transition;

		public bool IsValid(IContextContainer obj)
		{
			return !transition.IsValid(obj);
		}
	}
}

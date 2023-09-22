using System.Collections;
using System.Collections.Generic;
using FriendSea.StateMachine.Controls;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FriendSea.StateMachine.Conditions
{
	partial class InputPressedTransition : Controls.Transition.ICondition
	{
		[SerializeField]
		InputActionProperty action;

		public bool IsValid(IContextContainer obj)
		{
			if (!action.action.enabled)
				action.action.Enable();
			return action.action.IsPressed();
		}
	}
}

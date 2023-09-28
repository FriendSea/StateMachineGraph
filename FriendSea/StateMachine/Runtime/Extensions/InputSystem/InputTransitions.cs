#if STATEMACHINE_USE_INPUTSYSTEM

using UnityEngine;
using UnityEngine.InputSystem;

namespace FriendSea.StateMachine.Conditions
{
	[DisplayName("Input/IsPressed")]
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

	[DisplayName("Input/WasPressedThisFrame")]
	partial class InputPressedThisFrameTransition : Controls.Transition.ICondition
	{
		[SerializeField]
		InputActionProperty action;

		public bool IsValid(IContextContainer obj)
		{
			if (!action.action.enabled)
				action.action.Enable();
			return action.action.WasPressedThisFrame();
		}
	}

	[DisplayName("Input/WasRereasedThisFrame")]
	partial class InputReleasedThisFrameTransition : Controls.Transition.ICondition
	{
		[SerializeField]
		InputActionProperty action;

		public bool IsValid(IContextContainer obj)
		{
			if (!action.action.enabled)
				action.action.Enable();
			return action.action.WasReleasedThisFrame();
		}
	}
}

# endif

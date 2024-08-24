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

	[DisplayName("Input/VectorX")]
	partial class InputVectorXTransition : Controls.Transition.ICondition
	{
		[SerializeField]
		InputActionProperty action;
		[SerializeField]
		float threshold;

		public bool IsValid(IContextContainer obj)
		{
			if (!action.action.enabled)
				action.action.Enable();
			return threshold > 0f ?
				action.action.ReadValue<Vector2>().x >= threshold :
				action.action.ReadValue<Vector2>().x <= threshold;
		}
	}

	[DisplayName("Input/VectorY")]
	partial class InputVectorYTransition : Controls.Transition.ICondition
	{
		[SerializeField]
		InputActionProperty action;
		[SerializeField]
		float threshold;

		public bool IsValid(IContextContainer obj)
		{
			if (!action.action.enabled)
				action.action.Enable();
			return threshold > 0f ?
				action.action.ReadValue<Vector2>().y >= threshold :
				action.action.ReadValue<Vector2>().y <= threshold;
		}
	}
}

#endif

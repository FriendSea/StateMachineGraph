#if STATEMACHINE_USE_INPUTSYSTEM

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FriendSea.StateMachine.Behaviours
{
	[DisplayName("Platformer/Input/SetDirection")]
	public partial class SetInputDirection : BehaviourBase
	{
		[SerializeField]
		InputActionProperty input;

		[InjectContext]
		IDirectionable directionable;

		protected override void OnEnter(IContextContainer obj) {
			input.action.Enable();
		}

		protected override void OnExit(IContextContainer obj) { }

		protected override void OnUpdate(IContextContainer obj)
		{
			var val = Mathf.RoundToInt(input.action.ReadValue<float>());
			if (val != 0)
				directionable.Direction = val;
		}
	}
}

#endif

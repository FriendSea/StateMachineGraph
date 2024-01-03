#if STATEMACHINE_USE_INPUTSYSTEM

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FriendSea.StateMachine.Behaviours
{
	[DisplayName("Animator/Input/SetParameter")]
	public partial class SetInputAnimatorParameter : BehaviourBase
	{
		[SerializeField] string name;
		[SerializeField] InputActionProperty input;
		[InjectContext] Animator animator;

		protected override void OnEnter(IContextContainer obj) =>
			input.action.Enable();
		protected override void OnExit(IContextContainer obj) { }
		protected override void OnUpdate(IContextContainer obj) =>
			animator.SetFloat(name, input.action.ReadValue<float>());
	}
}

#endif

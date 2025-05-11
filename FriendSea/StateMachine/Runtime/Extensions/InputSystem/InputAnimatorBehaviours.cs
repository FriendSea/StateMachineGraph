#if STATEMACHINE_USE_INPUTSYSTEM

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FriendSea.StateMachine.Behaviours
{
	[DisplayName("Animator/Input/SetParameter")]
	public partial class SetInputAnimatorParameter : IBehaviour
	{
		[SerializeField] string name;
		[SerializeField] InputActionProperty input;
		[InjectContext] Animator animator;

		public void OnEnter(IContextContainer obj) =>
			input.action.Enable();
		public void OnExit(IContextContainer obj) { }
		public void OnUpdate(IContextContainer obj) =>
			animator.SetFloat(name, input.action.ReadValue<float>());
	}

    [DisplayName("Animator/Input/SetParameterMagniture")]
    public partial class SetInputAnimatorParameterMagniture : IBehaviour
    {
        [SerializeField] string name;
        [SerializeField] InputActionProperty input;
        [InjectContext] Animator animator;

        public void OnEnter(IContextContainer obj) =>
            input.action.Enable();
        public void OnExit(IContextContainer obj) { }
        public void OnUpdate(IContextContainer obj) =>
            animator.SetFloat(name, input.action.ReadValue<Vector2>().magnitude);
    }
}

#endif

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FriendSea.StateMachine.Controls;

namespace FriendSea.StateMachine.Conditions
{
	[UnityEngine.Scripting.APIUpdating.MovedFrom(false, sourceNamespace: "FriendSea.StateMachine", sourceClassName: "InvertTransition")]
	[DisplayName("Invert")]
    public class InvertTransition : Transition.ICondition, IInjectable
	{
		[SerializeReference]
		internal Transition.ICondition transition;

		public bool IsValid(IContextContainer obj)
		{
			return !transition.IsValid(obj);
		}

        public void OnSetup(IContextContainer ctx)
        {
            (transition as IInjectable)?.OnSetup(ctx);
        }
    }
}

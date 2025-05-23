using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FriendSea.StateMachine.Controls;
using System;

namespace FriendSea.StateMachine.Conditions
{
	[UnityEngine.Scripting.APIUpdating.MovedFrom(false, sourceNamespace: "FriendSea.StateMachine", sourceClassName: "InvertTransition")]
	[DisplayName("Invert")]
    public partial class InvertTransition : Transition.ICondition, IInjectable
	{
		[SerializeReference]
		internal Transition.ICondition transition;

        public IEnumerable<Type> GetRequiredTypes()
        {
            return (transition as IInjectable)?.GetRequiredTypes() ?? Array.Empty<Type>();
        }

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

using System.Collections.Generic;
using UnityEngine;

namespace FriendSea.StateMachine
{
	public class TriggerTransition : State.Transition.ICondition
	{
		static List<TriggerTransitionLabel> activeLabels = new List<TriggerTransitionLabel>();
		public static void IssueTransiton<T>(StateMachine<T> stateMachine, TriggerTransitionLabel label) where T : class
		{
			activeLabels.Add(label);
			stateMachine.DoTransition();
			activeLabels.Remove(label);
		}

		[SerializeField]
		TriggerTransitionLabel label = null;

		public bool IsValid(IContextContainer obj, int frameCount) => activeLabels.Contains(label);
	}

	public static class StateMachineExtensionForTriggerTransition
	{
		public static void IssueTrigger<T>(this StateMachine<T> stateMachine, TriggerTransitionLabel label) where T : class =>
			TriggerTransition.IssueTransiton<T>(stateMachine, label);
	}
}

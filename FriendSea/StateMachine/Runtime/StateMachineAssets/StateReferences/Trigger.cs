using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FriendSea.StateMachine
{
	public class Trigger : ISerializableStateReference
	{
		static List<TriggerLabel> activeLabels = new List<TriggerLabel>();
		public static void IssueTransiton<T>(StateMachine<T> stateMachine, TriggerLabel label) where T : class
		{
			activeLabels.Add(label);
			stateMachine.DoTransition();
			activeLabels.Remove(label);
		}

		[SerializeField]
		internal TriggerLabel label;
		[SerializeReference]
		public ISerializableStateReference[] targets;

		public (IState<IContextContainer> state, bool isValid) GetState(IContextContainer obj)
		{
			foreach (var target in targets)
			{
				var result = target.GetState(obj);
				if (result.isValid) return (result.state, activeLabels.Contains(label));
			}
			return (null, activeLabels.Contains(label));
		}
	}
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FriendSea
{
	public interface IStateMachineNode {
		[System.Serializable]
		public class State : IStateMachineNode
		{
			[SerializeField]
			internal string name;
			[SerializeReference]
			internal StateMachineState.IBehaviour[] behaviours;
		}

		[System.Serializable]
		public class Transition : IStateMachineNode
		{
			[SerializeReference]
			internal StateMachineState.ITransition[] transitions;
		}
	}
}

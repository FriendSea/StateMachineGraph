using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FriendSea.StateMachine
{
	public class StateMachineAsset : ScriptableObject
	{
		[SerializeField]
		internal State.Transition entryState;
		[SerializeField]
		internal State.Transition fallbackState;
		[SerializeField]
		internal State[] residentStates;

		public IState<IContextContainer> GetResidentState(string guid)
		{
			foreach(var state in residentStates)
				if (state.Id == guid) return state;
			return null;
		}

		public IStateReference<IContextContainer> EntryState => entryState;
		public IStateReference<IContextContainer> FallbackState => fallbackState;
	}
}
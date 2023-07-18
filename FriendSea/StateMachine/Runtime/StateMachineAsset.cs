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

		[System.Serializable]
		internal struct ResidentState
		{
			[SerializeField]
			internal string guid;
			[SerializeField]
			internal State state;
		}

		public IState<CachedComponents> GetResidentState(string guid)
		{
			foreach(var state in residentStates)
				if (state.Id == guid) return state;
			return null;
		}

		public IStateReference<CachedComponents> EntryState => entryState;
		public IStateReference<CachedComponents> FallbackState => fallbackState;
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FriendSea.StateMachine.Controls;

namespace FriendSea.StateMachine
{
	public class StateMachineAsset : ScriptableObject
	{
		[SerializeField]
		internal Transition entryState;
		[SerializeReference]
		internal ISerializableStateReference fallbackState;
		[SerializeField]
		internal ResidentState[] residentStates;

		public ResidentState GetResidentState(string guid)
		{
			foreach(var state in residentStates)
				if (state.id == guid) return state;
			return null;
		}

		public IStateReference<IContextContainer> EntryState => entryState;
		public IStateReference<IContextContainer> FallbackState => fallbackState;
	}
}

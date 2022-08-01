using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FriendSea
{
	public class StateMachineAsset : ScriptableObject
	{
		[SerializeField]
		internal StateMachineState.Transition entryState;
		[SerializeField]
		internal StateMachineState.Transition fallbackState;

		public IStateReference<CachedComponents> EntryState => (entryState.GetState(null, 0).state != null) ?
			entryState : fallbackState;
		public IStateReference<CachedComponents> FallbackState => fallbackState;
	}
}

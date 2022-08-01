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

		public IStateReference<CachedComponents> EntryState => entryState;
		public IStateReference<CachedComponents> FallbackState => fallbackState;
	}
}

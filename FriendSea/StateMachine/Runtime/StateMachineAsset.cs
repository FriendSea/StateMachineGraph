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

		public IStateReference<CachedComponents> EntryState => entryState;
		public IStateReference<CachedComponents> FallbackState => fallbackState;
	}
}

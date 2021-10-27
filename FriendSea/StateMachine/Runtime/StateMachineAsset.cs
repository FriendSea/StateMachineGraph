using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FriendSea
{
	public class StateMachineAsset : ScriptableObject
	{
		[SerializeReference]
		StateMachineNodeAsset entryState;
		[SerializeReference]
		StateMachineNodeAsset fallbackState;

		public IStateMachineState<GameObject> EntryState => (entryState == null) ? fallbackState : entryState;
		public IStateMachineState<GameObject> FallbackState => fallbackState;
	}
}

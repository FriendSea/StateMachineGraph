using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FriendSea {
	public class StateMachineNodeAsset : ScriptableObject, StateMachineState.IStateReference
	{
		[SerializeField]
		internal StateMachineState data;

		public (IStateMachineState<CachedComponents> state, bool isValid) GetState(CachedComponents obj, int frameCount) => (data, true);
	}
}

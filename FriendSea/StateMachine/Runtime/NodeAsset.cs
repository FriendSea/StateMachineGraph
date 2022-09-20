using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FriendSea.StateMachine {
	public class NodeAsset : ScriptableObject, State.IStateReference
	{
		[SerializeField]
		internal State data;

		public (IState<CachedComponents> state, bool isValid) GetState(CachedComponents obj, int frameCount) => (data, true);
	}
}

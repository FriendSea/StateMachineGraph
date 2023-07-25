using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FriendSea.StateMachine {
	public class NodeAsset : ScriptableObject, State.IStateReference
	{
		[SerializeField]
		internal State data;

		public (IState<IContextContainer> state, bool isValid) GetState(IContextContainer obj, int frameCount) => (data, true);
	}
}

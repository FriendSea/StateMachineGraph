using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FriendSea.StateMachine {
	public class NodeAsset : ScriptableObject, ISerializableStateReference
	{
		[SerializeField]
		internal State data;

		public (IState<IContextContainer> state, bool isValid) GetState(IContextContainer obj) => (data, true);
	}
}

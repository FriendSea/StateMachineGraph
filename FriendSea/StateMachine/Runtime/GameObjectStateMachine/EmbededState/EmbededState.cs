using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FriendSea.StateMachine.Controls;

namespace FriendSea.StateMachine
{
	public class EmbededState : MonoBehaviour, ISerializableStateReference
	{
		[SerializeField]
		EmbededStateLabel label;
		[SerializeField]
		State state;

		public (IState<IContextContainer> state, bool isValid) GetState(IContextContainer obj) =>
			(state, EmbededStateReference.CurrentLabel == label);
	}
}

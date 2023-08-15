using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FriendSea.StateMachine
{
	public class EmbededStateReference : State.IStateReference
	{
		public static EmbededStateLabel CurrentLabel { get; private set; }

		[SerializeField]
		internal EmbededStateLabel label;

		public (IState<IContextContainer> state, bool isValid) GetState(IContextContainer obj, int frameCount)
		{
			CurrentLabel = label;
			return obj.Get<State.IStateReference>().GetState(obj, frameCount);
		}
	}
}

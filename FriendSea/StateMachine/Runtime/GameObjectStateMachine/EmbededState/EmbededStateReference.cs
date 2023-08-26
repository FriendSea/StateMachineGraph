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

		static List<State.IStateReference> sharedList = new List<State.IStateReference>();

		public (IState<IContextContainer> state, bool isValid) GetState(IContextContainer obj)
		{
			CurrentLabel = label;
			obj.Get<GameObject>().GetComponentsInChildren(true, sharedList);
			foreach(var reference in sharedList)
			{
				var result = reference.GetState(obj);
				if (result.isValid)
					return result;
			}
			return (null, false);
		}
	}
}

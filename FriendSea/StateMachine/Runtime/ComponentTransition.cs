using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FriendSea.StateMachine
{
	public struct ComponentTransition : State.IStateReference
	{
		static List<State.IStateReference> sharedList = new List<State.IStateReference>();

		public (IState<CachedComponents> state, bool isValid) GetState(CachedComponents obj, int frameCount)
		{
			sharedList.Clear();
			obj.gameObject.GetComponentsInChildren(sharedList);
			foreach (var reference in sharedList)
			{
				var result = reference.GetState(obj, frameCount);
				if (result.isValid)
					return result;
			}
			return (null, false);
		}
	}
}
